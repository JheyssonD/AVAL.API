# 📘 Architecture Decision Records (ADR) — RentGuard AI

Este documento registra las decisiones arquitectónicas críticas tomadas durante el diseño y desarrollo de RentGuard AI, detallando el contexto, las opciones consideradas y la justificación final.

---

## ADR 001: Patrón SQL Outbox para Mensajería Asíncrona

### Contexto
Necesitamos garantizar la consistencia eventual entre el módulo de Pagos y el de TrustScore sin comprometer la performance de la API ni arriesgar la pérdida de eventos en caso de fallos de red.

### Decisión
Implementar el patrón **SQL Outbox** utilizando una tabla dedicada en la base de datos transaccional de SQL Server, en lugar de utilizar un Message Broker externo como Kafka o RabbitMQ en esta fase.

### Justificación
- **Atomicidad**: Permite guardar el cambio de estado del pago y el evento en la misma transacción de base de datos.
- **Simplicidad**: Elimina la necesidad de gestionar una infraestructura adicional (Kafka) y el riesgo de "Dual Writes".
- **Costo**: Aprovecha la infraestructura existente de SQL Server.

### Consecuencias
- Requiere un **Worker en segundo plano** (`OutboxProcessor`) para encuestar la tabla.
- Introduce una latencia de segundos (consistencia eventual), la cual es aceptable para el cálculo de TrustScore.

---

## ADR 002: Estrategia Híbrida de Persistencia (EF Core + Dapper)

### Contexto
El sistema requiere un equilibrio entre la productividad del desarrollador, el cumplimiento de reglas multi-tenant y la eficiencia en cálculos masivos.

### Decisión
Utilizar **EF Core** como motor principal para operaciones CRUD y lógica de dominio, y **Dapper** exclusivamente para consultas de solo lectura de alta complejidad o procesamiento por lotes.

### Justificación
- **EF Core**: Facilita el uso de **Global Query Filters** para asegurar el aislamiento multi-tenant y el control de concurrencia optimista (`RowVersion`).
- **Dapper**: Permite ejecutar SQL puro optimizado para el motor de trayectoria (Trajectory Engine) sin el overhead de materialización de EF.

---

## ADR 003: Multi-tenancy — Base de Datos Compartida, Esquema Compartido

### Contexto
La plataforma debe soportar múltiples propietarios (Landlords) manteniendo costos de infraestructura bajos y facilidad de mantenimiento.

### Decisión
Implementar multi-tenancy mediante una **Base de Datos Única y Esquema Único**, diferenciando los datos mediante una columna `TenantId` en cada tabla.

### Justificación
- **Escalabilidad de Costos**: Un solo recurso de SQL Server puede servir a cientos de Tenants.
- **Mantenimiento**: Las migraciones de esquema se aplican una sola vez para todos los clientes.
- **Seguridad**: El aislamiento se garantiza mediante filtros globales a nivel de `DbContext`.

---

## ADR 004: Arquitectura Monolítica Modular (Modulith)

### Contexto
Buscamos una arquitectura que permita la evolución futura hacia microservicios pero que no añada complejidad operativa innecesaria en el MVP.

### Decisión
Organizar el código en **Módulos Lógicamente Separados** dentro de un mismo proceso (Presentation -> Business -> Domain).

### Justificación
- **Despliegue simple**: Una sola unidad de despliegue (Docker/App Service).
- **Consistencia**: Comunicación interna rápida.
- **Refactorización fácil**: Los límites entre módulos están definidos por interfaces, permitiendo extraer un módulo a un microservicio en el futuro si es necesario.
