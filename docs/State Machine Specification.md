# 📘 State Machine Specification — RentGuard AI

Esta especificación define las máquinas de estado finitas (FSM) para las entidades críticas del sistema. El objetivo es prevenir estados inválidos y transiciones ilegales que puedan comprometer la integridad financiera o reputacional.

---

## 1. Payment State Machine

La gestión de pagos es el flujo más crítico. Los estados aseguran que un comprobante pase por validación de IA y humana antes de impactar el TrustScore.

```mermaid
stateDiagram-v2
    [*] --> Received: Webhook WhatsApp
    Received --> Processing: IA Analysis Start
    Processing --> Approved: AI Confidence >= 85%
    Processing --> PendingReview: AI Confidence < 85% / Discrepancy
    Processing --> Duplicate: Hash Collision Detected
    PendingReview --> Approved: Manual Landlord Approval
    PendingReview --> Rejected: Manual Landlord Rejection
    Approved --> [*]: Final State
    Rejected --> [*]: Final State
    Duplicate --> [*]: Final State
```

### Reglas de Transición (Payment)
- **Approved** y **Rejected** son estados terminales (inmutables).
- Un pago solo puede llegar a **Approved** desde `Processing` (automático) o `PendingReview` (manual).
- El estado **Duplicate** bloquea cualquier procesamiento posterior de TrustScore.

---

## 2. Outbox Message State Machine

Garantiza la consistencia eventual y la entrega "at-least-once" de eventos de dominio.

```mermaid
stateDiagram-v2
    [*] --> Pending: Domain Event Saved
    Pending --> Processing: Worker Pick-up
    Processing --> Published: Success
    Processing --> Failed: Error / Retry Limit
    Failed --> Pending: Retry (Exponential Backoff)
    Failed --> DeadLetter: Max Retries Reached (5)
    Published --> [*]: Clean-up Candidate
```

### Reglas de Transición (Outbox)
- **DeadLetter** requiere intervención manual del administrador para re-encolar o descartar.
- El Worker solo selecciona mensajes en estado `Pending` o `Failed` (con tiempo de espera).

---

## 3. Lease State Machine (Contratos)

Define el ciclo de vida del arrendamiento y la vinculación propiedad-inquilino.

```mermaid
stateDiagram-v2
    [*] --> Draft: Creation
    Draft --> Active: Signed / First Payment
    Active --> Overdue: Payment Date Passed + Grace
    Overdue --> Active: Debt Settled
    Active --> Terminated: Contract End / Eviction
    Overdue --> Terminated: Eviction / Legal
    Terminated --> [*]
```

### Reglas de Transición (Lease)
- Un contrato **Terminated** libera la propiedad para un nuevo `Draft`.
- El paso a **Overdue** es un proceso automático disparado por el sistema de monitoreo diario.

---

## 4. TrustScore Status (Reputación)

Aunque el score es un valor numérico, su "salud" se categoriza para triggers de UX.

```mermaid
stateDiagram-v2
    [*] --> Neutral: New Tenant (80 pts)
    Neutral --> Elite: Score > 800
    Elite --> Neutral: Score <= 800
    Neutral --> AtRisk: Score < 300
    AtRisk --> Neutral: Score >= 300
    AtRisk --> Blacklisted: Fraud / Score 0
    Blacklisted --> [*]: Irreversible
```

### Reglas de Transición (TrustScore)
- **Blacklisted** es un estado administrativo que impide la creación de nuevos contratos en cualquier propiedad del sistema.
- Las transiciones de salud disparan notificaciones específicas vía WhatsApp (ej. Felicitaciones por nivel Elite).
