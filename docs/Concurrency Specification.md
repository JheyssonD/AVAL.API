# 📘 Concurrency & Consistency Specification — RentGuard AI

Esta especificación define cómo RentGuard AI gestiona el acceso simultáneo a los datos y garantiza la integridad transaccional en un entorno multi-tenant de alto rendimiento.

---

## 1. Concurrencia Optimista (Optimistic Concurrency)

El sistema adopta un modelo de concurrencia optimista para maximizar el throughput sin bloquear registros innecesariamente.

### 1.1 Implementación de RowVersion
- **Mecanismo**: Todas las entidades críticas (`Payment`, `Lease`, `Property`, `TrustScore`) incluyen una propiedad `RowVersion` de tipo `byte[]` marcada como `[Timestamp]` (SQL Server `rowversion`).
- **Comportamiento**: En cada `UPDATE`, EF Core incluye el `RowVersion` original en la cláusula `WHERE`.
- **Conflicto**: Si el `RowVersion` ha cambiado en la DB, EF Core lanza una `DbUpdateConcurrencyException`.
- **Resolución**: El sistema debe atrapar esta excepción y retornar un error `ERR_DOM_CONFLICT` (HTTP 409) informando al usuario que los datos han sido modificados por otro proceso.

---

## 2. Límites Transaccionales (Transaction Boundaries)

### 2.1 Unidad de Trabajo (Unit of Work)
- Cada solicitud de comando (ej. `ApprovePayment`) debe ejecutarse dentro de una **única transacción de base de datos**.
- La transacción es gestionada por el `RentGuardDbContext`.

### 2.2 Atomaticidad Dominio-Outbox
- **Regla Inmutable**: El guardado de los cambios en las entidades de dominio y la inserción del mensaje en la tabla `OutboxMessages` **deben ocurrir dentro de la misma transacción**.
- Si cualquiera de los dos falla, se realiza un **Rollback** total. Esto garantiza que nunca se publique un evento sin haber persistido el cambio en el dominio.

---

## 3. Niveles de Aislamiento (Isolation Levels)

| Contexto | Nivel de Aislamiento | Justificación |
| :--- | :--- | :--- |
| **Operaciones CRUD (Default)** | `READ COMMITTED` | Balance ideal entre rendimiento y consistencia para la mayoría de los casos. |
| **Cálculo de TrustScore (Massive)** | `SNAPSHOT` | Evita bloqueos de lectura durante el procesamiento por lotes del Trajectory Engine. |
| **Generación de Reportes** | `READ UNCOMMITTED` | Permitido solo en réplicas de lectura para reportes no financieros. |

---

## 4. Modelo de Consistencia

RentGuard AI utiliza un modelo de **Consistencia Híbrida**:

### 4.1 Consistencia Fuerte (Strong Consistency)
- Aplicada dentro de un mismo **Bounded Context**.
- Ejemplo: Cuando se registra un pago, el estado del `Payment` se actualiza de forma inmediata y garantizada en la base de datos transaccional.

### 4.2 Consistencia Eventual (Eventual Consistency)
- Aplicada entre diferentes **Bounded Contexts** mediante eventos asíncronos.
- Ejemplo: La actualización del `TrustScore` tras la aprobación de un pago ocurre de forma eventual (segundos después) a través del `OutboxProcessor`.
- **Garantía**: El patrón Outbox asegura la entrega "At-least-once". Los suscriptores deben ser **Idempotentes** para manejar posibles re-entregas.
