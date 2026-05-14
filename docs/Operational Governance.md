# 📘 Operational Governance (SLA / SLO) — RentGuard AI

Este documento define los compromisos de disponibilidad, rendimiento y confiabilidad de RentGuard AI, estableciendo los objetivos internos (SLO) y los indicadores (SLI) necesarios para cumplir con los acuerdos de servicio (SLA).

---

## 1. Disponibilidad del Sistema (Availability)

| Métrica | Objetivo (SLO) | Definición (SLI) |
| :--- | :--- | :--- |
| **Uptime de API** | 99.9% | % de solicitudes exitosas (2xx/3xx) sobre el total. |
| **Recepción WhatsApp** | 99.95% | Disponibilidad del Webhook para mensajes de Meta. |
| **Disponibilidad DB** | 99.9% | Tiempo en que SQL Server responde a consultas. |

---

## 2. Rendimiento y Latencia (Performance)

| Métrica | Objetivo (SLO) | Definición (SLI) |
| :--- | :--- | :--- |
| **Latencia API (p95)** | < 500ms | Tiempo de respuesta en el 95% de las solicitudes. |
| **Procesamiento OCR** | < 10s | Tiempo desde `Received` hasta `Processing` finalizado. |
| **Lag de Outbox (p95)** | < 30s | Tiempo desde el guardado en Outbox hasta la ejecución del handler. |

---

## 3. Integridad y Calidad (Quality)

| Métrica | Objetivo (SLO) | Definición (SLI) |
| :--- | :--- | :--- |
| **Tasa de Éxito IA** | > 90% | % de pagos aprobados automáticamente sin `PendingReview`. |
| **Error Rate (p99)** | < 0.1% | % de errores `5xx` sobre el total de tráfico. |
| **Data Isolation** | 100% | Cero incidentes de acceso cruzado entre Tenants. |

---

## 4. Gestión de Incidentes (SLA de Soporte)

| Prioridad | Descripción | Tiempo Respuesta | Resolución |
| :--- | :--- | :--- | :--- |
| **P1 - Crítico** | Sistema caído o fuga de datos. | < 15 min | < 4 horas |
| **P2 - Alto** | Fallo en validación masiva de pagos. | < 1 hora | < 8 horas |
| **P3 - Medio** | Error en Dashboard / Reporting. | < 4 horas | < 24 horas |
| **P4 - Bajo** | Ajustes estéticos o dudas de uso. | < 24 horas | < 3 días |

---

## 5. Ventanas de Mantenimiento

- **Mantenimiento Programado**: Se realizará los domingos de 02:00 a 04:00 (UTC-5).
- **Notificación**: Se notificará a los Tenants vía SPA con al menos 48 horas de antelación.
- **Exclusión**: El tiempo de mantenimiento programado se excluye del cálculo de disponibilidad (SLA).

---

## 6. Reporte y Transparencia

- **Status Page**: Se mantendrá una página pública de estado con indicadores en tiempo real.
- **Post-Mortem**: Todo incidente P1 o P2 generará un documento de análisis de causa raíz (RCA) compartido con los involucrados.
