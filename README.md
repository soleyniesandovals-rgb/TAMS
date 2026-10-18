### Estructura del proyecto (resumen)

- `src/Core/ControlAcceso`: módulo de autenticación, autorización, cuentas, sesiones y recuperación.
- `src/Core/Permisos`: módulo de gestión de permisos (solicitudes y permisos activos). RF-GP-01 a RF-GP-07.
- `src/Tams.Api`: API web ASP.NET Core.
- `src/Tams.Negocio`: módulo de negocio de TAMS.
- `tests/Tests.Arquitectura`: pruebas de arquitectura (RD-03).
- `tests/Tests.Permisos`: pruebas unitarias del servicio de permisos (dobles en memoria, RD-12).

[...]

## Pruebas

Para ejecutar toda la suite de pruebas desde un clon limpio:

```powershell
dotnet test TAMS.slnx
```

- Las pruebas de `tests/Tests.Permisos` usan implementaciones falsas en memoria (`ICoreUnidadDeTrabajo`, repositorios y `TimeProvider`), por lo que **no requieren SQL Server encendido ni variables de entorno** (RD-12).
- Las pruebas arquitectónicas se ejecutan junto a las unitarias en el mismo comando.

## Cómo provocar cada criterio de aceptación RF-GP

Todos los casos usan los endpoints reales de `PermisosController` (ruta `api/permisos`). Requiere un token JWT válido para el actor indicado.

> Nota: `Invoke-RestMethod` lanza excepción en respuestas no-2xx; para obtener el código exacto conviene usar `Invoke-WebRequest` con `try/catch`. En la tabla se indica el código HTTP esperado para cada caso.

### RF-GP-01: crear solicitud de permiso
- Método: `POST`
- Ruta: `/api/permisos`
- Autenticado como: **Estandar** (cualquier usuario autenticado)
- Body (JSON):
  ```json
  { "tipoPermiso": "acceso-reportes" }
  ```
- Código esperado: **201** (devuelve resumen de la solicitud creada; `estado = "Pendiente"`)

### RF-GP-02: listar solo solicitudes propias
- Método: `GET`
- Ruta: `/api/permisos`
- Autenticado como: **Estandar** (solo ve sus propias solicitudes)
- Body: ninguno
- Código esperado: **200** (lista únicamente las solicitudes del usuario autenticado)

### RF-GP-03: listar solicitudes pendientes (solo Administrador)
- Método: `GET`
- Ruta: `/api/permisos/pendientes`
- Autenticado como: **Administrador**
- Body: ninguno
- Código esperado: **200** (todas las solicitudes con `estado = "Pendiente"`)
- Caso de rechazo por autorización: si lo intenta un **Estandar** → **403** (el atributo `[RequiereRol]` bloquea antes del servicio)

### RF-GP-04: rechazar solicitud (motivo obligatorio)
1. Crear una solicitud pendiente (Estandar) → `POST /api/permisos` (201)
2. Intentar rechazar SIN motivo (Administrador, `POST /api/permisos/{id}/rechazar` con `{"motivo": ""}`) → **400** con mensaje `"El rechazo requiere un motivo."`
3. Rechazar CON motivo válido (Administrador, `{"motivo": "No corresponde al cargo."}`) → **200** (estado `"Rechazada"`, `motivoRechazo` guardado)

### RF-GP-05: aprobación aplica permiso automáticamente
1. Crear solicitud pendiente (Estandar) → 201
2. Verificar acceso ANTES de aprobar (Estandar, `GET /api/permisos/acceso/{tipoPermiso}`) → **403** (`Forbid()`)
3. Aprobar (Administrador, `POST /api/permisos/{id}/aprobar`) → **200** (aplicación automática en la misma llamada)
4. Verificar acceso DESPUÉS de aprobar (Estandar, `GET /api/permisos/acceso/{tipoPermiso}`) → **200** (`{"mensaje":"Acceso concedido."}`)

### RF-GP-06: transiciones válidas únicas (máquina de estados)
1. Aprobar una solicitud pendiente → 200 (pasa a `Aplicada` en la misma operación). 
2. Intentar aprobar **la misma** solicitud nuevamente (Administrador) → **409** (transición `Aplicada → Aprobada` no permitida por `TransicionesPermiso`; mensaje `"No se permite pasar de Aplicada a Aprobada..."`).
3. Intentar rechazar una solicitud ya `Rechazada` (o ya `Aplicada`) cuando la transición no está habilitada → **409** con `TransicionNoPermitidaExcepcion` propagada tal cual.

### RF-GP-07: nadie resuelve su propia solicitud
1. El propio Administrador crea una solicitud (`POST /api/permisos` autenticado como Administrador) → 201
2. El mismo Administrador intenta aprobarla (`POST /api/permisos/{id}/aprobar`) → **409** con mensaje `"No puedes resolver tu propia solicitud."` (chequeo antes de validar la transición)
3. El mismo Administrador intenta rechazarla (`POST /api/permisos/{id}/rechazar` con motivo) → **409** con el mismo mensaje.
