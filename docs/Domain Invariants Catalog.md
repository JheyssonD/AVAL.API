# 📘 Domain Invariants Catalog — RentGuard AI

Este catálogo define las leyes absolutas y verdades inmutables del dominio de RentGuard AI. Estas reglas deben ser garantizadas por el **Core de Negocio** y verificadas mediante **Tests de Dominio** (TDD). Si una operación viola una de estas reglas, el sistema debe fallar de forma ruidosa y controlada.

---

## 1. Invariantes de Pagos (Payment Integrity)

- **INV-PAY-01 (Inmutabilidad del Aprobado)**: Una vez que un pago alcanza el estado `Approved`, sus atributos financieros (`Monto`, `Moneda`, `Referencia`, `Fecha`) son **Inmutables**. Solo se permiten correcciones mediante un evento de auditoría compensatorio gestionado por un administrador.
- **INV-PAY-02 (Aislamiento Forzado)**: Ningún pago puede ser creado, consultado o modificado sin un `TenantId` válido y verificado. El sistema debe lanzar una excepción crítica si se intenta persistir un pago con `TenantId` nulo o ajeno.
- **INV-PAY-03 (Unicidad de Comprobante)**: No pueden existir dos pagos registrados para el mismo contrato con el mismo hash de imagen dentro de un periodo de 90 días.

---

## 2. Invariantes de TrustScore (Reputational Integrity)

- **INV-TS-01 (Trazabilidad Total)**: Ningún puntaje de TrustScore puede cambiar sin un **Evento de Dominio** de origen (`PaymentApproved`, `PaymentRejected`, `LeaseOverdue`). No se permiten actualizaciones directas a la propiedad `Score` sin un registro en el historial.
- **INV-TS-02 (Límites de Escala)**: El valor del TrustScore debe mantenerse estrictamente en el rango `[0, 1000]`. Cualquier cálculo que resulte en un valor fuera de este rango debe ser truncado al límite correspondiente.
- **INV-TS-03 (Consistencia de Estrategia)**: El cálculo del score para un Tenant debe usar exclusivamente la estrategia (`Delta` o `Regression`) configurada en sus ajustes de Tenant. No se permite mezclar estrategias en una misma ejecución.

---

## 3. Invariantes de Contratos (Lease Integrity)

- **INV-LSE-01 (Propiedad Ocupada)**: Una propiedad no puede tener más de un contrato en estado `Active` simultáneamente. El sistema debe impedir la activación de un nuevo contrato si existe uno previo no terminado (`Terminated`).
- **INV-LSE-02 (Integridad Temporal)**: La fecha de inicio de un contrato debe ser estrictamente menor a la fecha de finalización.
- **INV-LSE-03 (Día de Pago Válido)**: El día de pago mensual de un contrato debe ser un valor entero entre `1` y `31`.

---

## 4. Invariantes de Infraestructura (Consistency Laws)

- **INV-INF-01 (Atomaticidad del Outbox)**: El guardado del estado de la entidad y su mensaje correspondiente en el Outbox debe ocurrir dentro de la misma **Transacción Atómica** (Unit of Work). Si el guardado del Outbox falla, la transacción del dominio debe revertirse (Rollback).
- **INV-INF-02 (Concurrencia Optimista)**: Toda actualización de estado en entidades transaccionales (`Payment`, `Lease`) debe verificar la `RowVersion`. Si existe un conflicto de versiones, la operación debe ser rechazada para evitar el fenómeno de "Lost Update".
- **INV-INF-03 (Propagación de Contexto)**: Todo procesamiento asíncrono (Outbox Worker) debe propagar el `TenantId` del mensaje original al `TenantContext` antes de ejecutar cualquier handler de dominio.
