🛡️ RentGuard AI — MVP Development Specification
Version 1.0 — Google Antigravity AI Development Guide
1. Objetivo del Documento

Este documento define las reglas obligatorias para que Google Antigravity AI implemente el MVP de RentGuard AI.

El objetivo NO es experimentar arquitecturas complejas.

El objetivo es construir un MVP:

mantenible
modular
seguro
extensible
operacionalmente simple

Toda generación de código debe obedecer estrictamente este documento.

2. Objetivo Real del MVP

El MVP valida el siguiente flujo:

captura → validación → reputación → trazabilidad
3. Hipótesis del MVP

El MVP existe para validar:

Los inquilinos usan WhatsApp para reportar pagos.
Los landlords perciben valor en automatización.
Un TrustScore básico mejora trazabilidad operacional.
4. Restricciones Obligatorias
4.1 Prohibido
Prohibido	Razón
Microservicios	complejidad prematura
Kubernetes	innecesario
Event sourcing	fuera MVP
CQRS distribuido	sobreingeniería
Redis obligatorio	no requerido
SignalR avanzado	fuera alcance
Arquitectura serverless compleja	no necesaria
4.2 Obligatorio
Obligatorio	Estado
Modular Monolith	✅
Vertical Slice	✅
Feature-based organization	✅
SQL Outbox	✅
Optimistic Concurrency	✅
Manual Review	✅
Private Core Submodule	✅
5. Arquitectura Oficial
5.1 Arquitectura Base
Angular SPA
     ↓
Presentation/API Host
     ↓
Contracts Layer
     ↓
Private Core Modules
     ↓
SQL Server
     ↓
SQL Outbox Worker
     ↓
Vision AI Provider
5.2 Filosofía Arquitectónica

La arquitectura separa:

Capa	Responsabilidad
Skeleton Público	infraestructura
Core Privado	lógica negocio
6. Estructura de Repositorios
6.1 Repositorio Principal
RentGuardAI/
├── Presentation/
│   ├── API/
│   ├── Contracts/
│   └── src/
│       └── RentGuard.Core.Business/
│
├── RentGuard.Frontend/
├── infrastructure/
├── docker/
├── docs/
└── scripts/
6.2 Core Privado

La carpeta:

RentGuard.Core.Business

es un Git Submodule privado.

Contiene
dominio
lógica financiera
scoring
workflows
módulos negocio
NO contiene
frontend
infraestructura
pipelines
despliegue
7. Contracts Layer
7.1 Objetivo

La carpeta:

Presentation/Contracts

define:

DTOs
interfaces
eventos
requests
responses
7.2 Prohibido
Prohibido	Razón
lógica negocio	rompe boundaries
cálculos	fuga dominio
servicios	acoplamiento
8. Arquitectura Backend
8.1 Estilo Oficial
Modular Monolith
Vertical Slice Architecture
Feature-Based Organization
8.2 Estructura Obligatoria
RentGuard.Core.Business/
├── Shared/
├── Modules/
│   ├── Payments/
│   ├── TrustScore/
│   ├── WhatsApp/
│   ├── AIValidation/
│   ├── Review/
│   └── Identity/
8.3 Patrón Obligatorio

Cada feature debe implementar:

Request
→ Endpoint
→ Handler
→ Domain
→ Response
8.4 Organización de Features
Payments/
├── CreatePayment/
│   ├── CreatePaymentRequest.cs
│   ├── CreatePaymentEndpoint.cs
│   ├── CreatePaymentHandler.cs
│   ├── CreatePaymentValidator.cs
│   └── CreatePaymentResponse.cs
8.5 Prohibiciones
Prohibido	Razón
Controllers gigantes	complejidad
Services god-object	acoplamiento
lógica financiera en API	rompe dominio
helpers globales gigantes	pérdida modular
9. Arquitectura Frontend
9.1 Filosofía

El frontend:

renderiza estado
coordina UI
consume contratos

El frontend NO contiene:

reglas financieras
scoring
validaciones críticas
9.2 Estructura Obligatoria
src/app/
├── core/
├── shared/
└── features/
9.3 Segmentación Obligatoria
feature/
├── pages/
├── components/
├── dialogs/
├── grids/
├── services/
└── state/
9.4 Prohibiciones Frontend
Prohibido	Razón
HTML inline en TS	mantenibilidad
templates gigantes	complejidad
modales inline	acoplamiento
múltiples responsabilidades visuales	baja escalabilidad
lógica negocio en componentes	rompe arquitectura
9.5 Separación de Archivos
Obligatorio
component/
├── component.ts
├── component.html
├── component.scss
└── component.spec.ts
Prohibido
template: `...`
styles: [`...`]
9.6 Angular Rules
Obligatorio
Regla	Estado
Signals en services	✅
readonly signals	✅
OnPush everywhere	✅
AsyncPipe/toSignal	✅
Prohibido
Prohibido	Razón
any	pérdida tipado
manual subscriptions	memory leaks
state complejo en páginas	acoplamiento
10. Scope MVP
Incluido
Feature	MVP
WhatsApp webhook	✅
Upload comprobantes	✅
OCR/Vision AI	✅
Manual Review	✅
TrustScore básico	✅
Dashboard landlord	✅
SQL Outbox	✅
Audit logs básicos	✅
Excluido
Feature	Razón
antifraude avanzado	no existe data
analytics avanzados	fuera MVP
tiempo real complejo	bajo valor
scoring financiero	fuera alcance
11. Business Rules
11.1 TrustScore
Puntaje Inicial
100 puntos
Rango

0≤TrustScore≤1000

Fórmula

Score
new
	​

=max(0,min(1000,Score
old
	​

+Δ))

Eventos
Evento	Δ
Pago anticipado	+15
Pago puntual	+10
Pago tardío	-20
Pago parcial	0
Pago rechazado	-5
12. Payment Lifecycle
Estados
Received
→ Processing
→ PendingReview
→ Approved
→ Rejected
→ Failed
Restricciones
Regla	Estado
Approved editable	Prohibido
Approved delete	Prohibido
Approved recalculation	Prohibido
Rejected → Approved	Solo Admin
13. AI Validation Policy
Escenario	Acción
Confidence < 85%	PendingReview
Diferencia > 0.5%	PendingReview
Duplicado	Rejected
Formato inválido	rechazo inmediato
13.1 Regla Crítica

Vision AI no tiene autoridad de negocio.

La IA:

NO aprueba pagos
NO modifica scores
NO toma decisiones financieras
14. Invariantes del Dominio
Obligatorio
Invariante	Estado
Approved immutable	✅
Score history obligatorio	✅
MessageId único	✅
comprobante hash único	✅
15. Persistencia y Concurrencia
Base de Datos
Requerido	Estado
SQL Server	✅
EF Core	✅
Migrations	✅
índices explícitos	✅
Concurrencia
Estrategia	Estado
RowVersion	✅
Optimistic Concurrency	✅
16. SQL Outbox
16.1 Objetivo

Garantizar:

consistencia
retries
persistencia eventos
desacoplamiento futuro
16.2 Tabla Obligatoria
OutboxMessages
Campos mínimos
Campo	Requerido
Id	✅
EventType	✅
Payload	✅
Status	✅
RetryCount	✅
NextAttemptAt	✅
16.3 Worker Responsibilities
Responsabilidad	Estado
polling	✅
retries	✅
dead marking	✅
structured logging	✅
17. Seguridad MVP
Obligatorio
Seguridad	Estado
HTTPS	✅
JWT	✅
HMAC Meta	✅
Tenant filtering	✅
Input validation	✅
Prohibido
Prohibido	Razón
secrets hardcoded	inseguridad
bypass tenant filters	fuga datos
credenciales en código	riesgo crítico
18. Observabilidad
Obligatorio

Todos los logs deben incluir:

Campo	Obligatorio
CorrelationId	✅
TenantId	✅
PaymentId	cuando exista
Prohibido
Console.WriteLine
console.log

como mecanismo operativo.

19. Non-Goals MVP
El MVP NO busca:
reemplazar entidades financieras
validar legalmente pagos
scoring crediticio oficial
antifraude avanzado
automatización 100%
underwriting financiero
20. SLA Operacional MVP
Operación	SLA
WhatsApp ACK	< 5 segundos
OCR Processing	< 45 segundos
Validación completa	< 2 minutos
21. Filosofía Final de Ingeniería
Regla Fundamental

“El MVP debe ser suficientemente simple para evolucionar y suficientemente sólido para no colapsar.”

Prioridades
claridad
mantenibilidad
integridad financiera
trazabilidad
modularidad
simplicidad operacional
Regla Final

Toda complejidad adicional requiere:

necesidad operacional real
evidencia de negocio
justificación técnica clara

No se permite complejidad basada únicamente en preferencias arquitectónicas.