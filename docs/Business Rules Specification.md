# 📘 Business Rules Specification — RentGuard AI

Este documento define el catálogo de reglas de negocio (BRD) de RentGuard AI. Estas reglas son inmutables y sirven como base para la implementación de invariantes de dominio y validaciones de sistema.

---

## 1. TrustScore (Sistema Reputacional)

### 1.1 Límites y Escalas
- **Rango de Puntaje**: [0, 1000].
- **Puntaje Inicial (New Tenant)**: 80 puntos.
- **Techo de Confianza**: Ningún evento individual puede hacer que el score supere 1000.
- **Suelo de Riesgo**: Ningún evento puede reducir el score por debajo de 0.

### 1.2 Reglas de Puntuación (Bonificaciones)
- **BR-TS-01 (Pago Anticipado)**: Si el pago se aprueba > 48 horas antes de la fecha de vencimiento: **+15 puntos**.
- **BR-TS-02 (Pago Puntual)**: Si el pago se aprueba entre 48h antes y el final del día de vencimiento: **+10 puntos**.
- **BR-TS-03 (Pago en Período de Gracia)**: Si el pago se aprueba durante los 5 días naturales posteriores al vencimiento: **+2 puntos**.

### 1.3 Reglas de Puntuación (Penalizaciones)
- **BR-TS-04 (Pago Tardío)**: Si el pago se aprueba entre el día 6 y 15 después del vencimiento: **-20 puntos**.
- **BR-TS-05 (Pago Muy Tardío)**: Si el pago se aprueba después de 15 días del vencimiento: **-50 puntos**.
- **BR-TS-06 (Pago Fraudulento)**: Detección confirmada de alteración de comprobante: **Set Score = 0 + Bloqueo de Cuenta**.

### 1.4 Estrategias de Cálculo (Patrón Strategy)
- **STR-01 (Delta Simple)**: Aplicación directa de los puntos arriba mencionados sobre el saldo actual.
- **STR-02 (Trayectoria de Regresión)**: Cálculo de la pendiente (m) de los últimos 6 meses de snapshots.
    - Tendencia Positiva (m > 0): Multiplicador de bonificación x1.2.
    - Tendencia Negativa (m < 0): Multiplicador de penalización x1.5.

---

## 2. Gestión de Pagos

### 2.1 Validación Financiera
- **BR-PY-01 (Margen de Redondeo)**: Se acepta una diferencia de hasta el **0.1%** del monto esperado para compensar comisiones bancarias.
- **BR-PY-02 (Pagos Parciales)**:
    - Se registran en estado `Approved` (como recibidos parcialmente).
    - El contrato no se marca como "Pagado" hasta que el acumulado iguale o supere el monto de renta.
    - Puntuación: **0 puntos** (no bonifica hasta completar).
- **BR-PY-03 (Pagos Excedentes)**:
    - El monto sobrante se acredita automáticamente como `CreditBalance` para el inquilino para el siguiente mes.
- **BR-PY-04 (Detección de Duplicados)**:
    - Se genera un SHA-256 de la imagen del comprobante.
    - Si el hash existe en los últimos 90 días para el mismo Tenant: Estado = `Duplicate`.

### 2.2 Período de Gracia
- **BR-PY-05 (Definición de Gracia)**: 5 días naturales a partir del día siguiente de la fecha de vencimiento estipulada en el contrato.

---

## 3. Inteligencia Artificial (Vision AI)

### 3.1 Procesamiento OCR
- **BR-IA-01 (Umbral de Confianza)**: El confidence score del OCR debe ser **>= 85%** en los campos `Monto` y `Referencia`.
- **BR-IA-02 (Fallback de Confianza)**: Si confidence < 85%: Estado = `PendingReview`.
- **BR-IA-03 (Detección de Fecha)**: Si el OCR no detecta la fecha en el comprobante, se asume la fecha del servidor (timestamp de recepción).

### 3.2 Revisión Manual (Human-in-the-loop)
- **BR-IA-04 (Activación de Revisión)**: Un pago entra en `PendingReview` si:
    1. El monto extraído difiere > 0.1% del esperado.
    2. El confidence score es bajo.
    3. Se detecta posible fraude o imagen ilegible.

---

## 4. Inquilinos (Tenants) y Onboarding

### 4.1 Identidad y Propiedad
- **BR-TN-01 (Identidad WhatsApp)**: El número de WhatsApp es el identificador único del residente. Un número no puede estar vinculado a dos residentes activos diferentes.
- **BR-TN-02 (Aislamiento de Datos)**: Un Landlord solo puede ver pagos y contratos vinculados a sus propios inmuebles.
- **BR-TN-03 (Onboarding SPA)**: Los inquilinos pueden navegar por el catálogo de propiedades en el SPA sin registro previo, pero requieren vinculación de contrato para reportar pagos.

---

## 5. Invariantes de Dominio (Reglas Inmutables)

1. Un pago en estado `Approved` es **final** y no puede ser modificado (excepto por auditoría de Admin con log).
2. Todo cálculo de TrustScore debe dispararse exclusivamente tras un evento `PaymentApproved` o `LeaseExpired`.
3. El `TenantId` (Software Tenant) es **obligatorio** en toda transacción de base de datos para garantizar el cumplimiento de multi-tenancy.
