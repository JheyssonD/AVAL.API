# 📘 Observability Specification — RentGuard AI

Esta especificación define la estrategia de observabilidad de RentGuard AI, garantizando que el sistema sea diagnosticable, medible y resiliente mediante el uso de los tres pilares: Métricas, Logs y Trazas.

---

## 1. Métricas (Metrics)

Se monitorizan indicadores clave para salud técnica (SLIs) y éxito de negocio (KPIs).

### 1.1 Métricas de Negocio
- `payments_received_total`: Conteo total de comprobantes recibidos vía WhatsApp.
- `payments_approved_total`: Conteo de pagos validados exitosamente.
- `ocr_confidence_average`: Promedio de confianza de Vision AI por lote.
- `trustscore_updates_total`: Frecuencia de actualizaciones de reputación.

### 1.2 Métricas de Infraestructura
- `outbox_pending_messages`: Cantidad de mensajes esperando ser procesados (indicador de backlog).
- `api_request_duration_seconds`: Latencia de los endpoints de la API.
- `db_connection_pool_usage`: Salud de la conexión a SQL Server.
- `outbox_retry_count`: Frecuencia de fallos y reintentos en el procesamiento asíncrono.

---

## 2. Logs Estructurados (Logging)

El sistema utiliza **Serilog** con formato JSON para facilitar la ingesta en herramientas como ELK Stack o Azure Monitor.

### 2.1 Estándar de Log
Cada log debe incluir:
- `Timestamp`: ISO 8601 UTC.
- `Level`: Information, Warning, Error, Critical.
- `CorrelationId`: Para rastreo de la transacción.
- `TenantId`: Para filtrado por Landlord.
- `SourceContext`: Clase o módulo que genera el log.
- `Message`: Mensaje descriptivo con placeholders.

### 2.2 Eventos Críticos a Loguear
- Cambio de estado de un pago (ej. `Processing` -> `PendingReview`).
- Violación de una invariante de dominio.
- Reintento máximo alcanzado en el Outbox.
- Acceso denegado por políticas de Multi-tenancy.

---

## 3. Trazabilidad (Tracing)

Se implementa **OpenTelemetry** para el seguimiento distribuido.

- **CorrelationId**: Unifica todos los logs y trazas de una misma operación desde el Webhook de WhatsApp hasta la base de datos.
- **CausationId**: Permite visualizar la jerarquía de eventos (ej. qué pago disparó qué actualización de TrustScore).

---

## 4. Dashboards de Control

| Dashboard | Audiencia | Métricas Clave |
| :--- | :--- | :--- |
| **Operational Health** | DevOps / SRE | CPU/RAM, Latencia API, Errores 5xx, Lag de Outbox. |
| **Validation Pipeline** | Soporte / IA | Tasa de éxito OCR, % de pagos en `PendingReview`, Tiempo promedio de validación. |
| **Business Growth** | Management | Recaudación total, Nuevos Tenants, Distribución de TrustScore. |

---

## 5. Alertas (Alerting)

| Alerta | Condición | Severidad | Canal |
| :--- | :--- | :--- | :--- |
| **Outbox Stalled** | `outbox_pending_messages > 100` por 5 min. | High | Slack / PagerDuty |
| **OCR Failure Spike** | `ocr_error_rate > 10%` en 15 min. | Medium | Email |
| **Data Isolation Breach** | Cualquier `SEC_TENANT_MISMATCH`. | Critical | SMS / Alerta Inmediata |
| **High Latency** | `api_duration_p95 > 2s`. | Medium | Dashboard |
