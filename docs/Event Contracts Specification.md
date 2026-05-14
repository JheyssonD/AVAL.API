# 📘 Event Contracts Specification — RentGuard AI

Esta especificación define el estándar de comunicación asíncrona en RentGuard AI mediante el patrón **Event-Driven Architecture**. Todos los eventos persistidos en el Outbox y publicados deben cumplir con estos contratos para garantizar la trazabilidad y la evolución del sistema.

---

## 1. Estándar de Sobre (Envelope)

Cada evento debe estar envuelto en un "Sobre" de metadatos estandarizado.

| Campo | Tipo | Descripción |
| :--- | :--- | :--- |
| `EventId` | Guid | Identificador único del evento. |
| `Type` | String | Nombre completo del tipo de evento (ej. `PaymentApprovedEvent`). |
| `Version` | Integer | Versión del esquema del evento (inicia en 1). |
| `OccurredOn` | DateTime | Timestamp UTC de creación del evento. |
| `TenantId` | Guid | Identificador del Tenant origen para aislamiento. |
| `CorrelationId`| Guid | ID de la transacción original (ej. request de API o mensaje de WhatsApp). |
| `CausationId` | Guid | ID del mensaje o evento que causó este evento. |
| `Payload` | JSON | El contenido específico del evento de dominio. |

---

## 2. Eventos Definidos (v1)

### 2.1 PaymentApprovedEvent
Disparado cuando un pago es validado exitosamente (automática o manualmente).

```json
{
  "PaymentId": "guid",
  "LeaseId": "guid",
  "ResidentId": "guid",
  "Amount": 1000.00,
  "Currency": "USD",
  "ApprovedBy": "system|admin_id",
  "ApprovalDate": "2026-05-14T15:00:00Z"
}
```

### 2.2 PaymentRejectedEvent
Disparado cuando un pago falla la validación o es marcado como fraudulento.

```json
{
  "PaymentId": "guid",
  "Reason": "Ilegible | Discrepancia | Fraude",
  "RejectedBy": "system|admin_id",
  "EvidenceHash": "sha256"
}
```

### 2.3 TrustScoreUpdatedEvent
Disparado tras el recalculo exitoso de la reputación de un inquilino.

```json
{
  "ResidentId": "guid",
  "PreviousScore": 80,
  "NewScore": 95,
  "Reason": "PaymentApproved",
  "CalculationMethod": "Delta | Trajectory"
}
```

---

## 3. Estrategia de Versionado

Para evitar "Breaking Changes" en sistemas asíncronos:

1. **Cambios Compatibles**: Adición de campos opcionales. El consumidor debe ignorar campos que no conoce.
2. **Cambios Incompatibles**: Renombrado de campos o cambio de tipos. Requiere incremento de la propiedad `Version`.
3. **Upcasting**: Si un handler recibe un evento `v1` pero el dominio espera `v2`, el middleware de mensajería debe "elevar" el evento mapeando los campos faltantes con valores por defecto.

---

## 4. Trazabilidad (Distributed Tracing)

- **CorrelationId**: Debe ser generado en el punto de entrada (API / Webhook) y viajar a través de todos los servicios, outbox y handlers. Esto permite rastrear una solicitud de WhatsApp hasta la actualización final del TrustScore en los logs.
- **CausationId**: Permite construir el "Grafo de Causalidad". Si el evento B fue disparado por el evento A, el `CausationId` de B será el `EventId` de A.
