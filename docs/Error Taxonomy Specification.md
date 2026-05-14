# 📘 Error Taxonomy Specification — RentGuard AI

Esta especificación estandariza la clasificación, reporte y resolución de errores en RentGuard AI. El sistema utiliza el estándar **RFC 7807 (Problem Details for HTTP APIs)** para comunicar errores de forma estructurada.

---

## 1. Categorías de Errores

| Categoría | Código Base | Descripción |
| :--- | :--- | :--- |
| **Domain Error** | `ERR_DOM_XXX` | Violación de una invariante de negocio (ej. Pago duplicado). |
| **Validation Error** | `ERR_VAL_XXX` | Datos de entrada mal formados o incompletos. |
| **Infrastructure Error** | `ERR_INF_XXX` | Fallos en servicios externos, DB o red. |
| **Security Error** | `ERR_SEC_XXX` | Problemas de autenticación o violación de aislamiento multi-tenant. |
| **AI Error** | `ERR_IA_XXX` | Fallos en el procesamiento Vision AI o confianza insuficiente. |

---

## 2. Matriz de Severidad y Reintento

| Severidad | Impacto | Retryable | Acción Inmediata |
| :--- | :--- | :--- | :--- |
| **Critical** | Sistema caído o pérdida de datos. | No | Alerta inmediata a On-call. |
| **High** | Fallo en validación de pago o Outbox bloqueado. | Sí (Auto) | Notificar a Soporte Técnico. |
| **Medium** | Error en visualización de Dashboard o retraso OCR. | Sí (Auto) | Log en monitoreo. |
| **Low** | Error de formato en SPA o advertencia menor. | No | Log informativo. |

---

## 3. Taxonomía Detallada

### 3.1 Errores de Infraestructura (Retryable)
Estos errores son gestionados automáticamente por el **Outbox Worker** con Exponential Backoff.
- `INF_DB_TIMEOUT`: Tiempo de espera de base de datos excedido.
- `INF_META_API_DOWN`: No se puede conectar con el webhook de WhatsApp.
- `INF_BLOB_STORAGE_ERROR`: Fallo al guardar imagen del comprobante.

### 3.2 Errores de Negocio (Non-Retryable)
Estos errores NO deben reintentarse automáticamente ya que requieren cambio en la lógica o intervención humana.
- `DOM_INSUFFICIENT_FUNDS`: El pago reportado no cubre la deuda mínima.
- `DOM_LEASE_EXPIRED`: Intento de pago sobre un contrato terminado.
- `SEC_TENANT_MISMATCH`: Intento de acceso a datos de otro TenantId (Evento Crítico).

---

## 4. Estructura de Respuesta (RFC 7807)

Todas las APIs deben responder con este formato en caso de error:

```json
{
  "type": "https://rentguard.ai/errors/domain-error",
  "title": "Violación de Invariante de Dominio",
  "status": 400,
  "detail": "No se puede aprobar un pago sobre una propiedad ocupada por otro contrato activo.",
  "instance": "/payments/approve/guid",
  "extensions": {
    "errorCode": "ERR_DOM_001",
    "severity": "High",
    "correlationId": "guid"
  }
}
```

---

## 5. Gobierno y Responsabilidades (Ownership)

- **Errores Críticos/Infraestructura**: Responsabilidad del equipo de **SRE / DevOps**.
- **Errores de Dominio**: Responsabilidad de **Ingeniería de Backend** (ajuste de lógica).
- **Errores de Validación/IA**: Responsabilidad de **Data Science / Frontend** (mejorar UX de captura).
- **Dead Letter Queue (DLQ)**: Monitoreada diariamente por el equipo de **Soporte de Nivel 2**.
