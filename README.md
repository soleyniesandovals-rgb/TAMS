# TAMS

Sistema de Gestión Académica Docente para la administración de asignaciones, cursos, horarios y calificaciones. Proyecto de Programación III.

## Requisitos previos

- .NET SDK 10
- SQL Server (local o accesible)
- La herramienta de migraciones de EF Core: `dotnet tool install --global dotnet-ef`
- Un servidor SMTP para el envío de correos (ver [Servidor SMTP](#servidor-smtp))

## Variables de entorno

Ninguna credencial está en el repositorio (RD-10). Defínelas con:

```powershell
[System.Environment]::SetEnvironmentVariable("NOMBRE","valor","User")
```

Cierra y abre VS Code por completo después de definir o cambiar cualquiera de estas.

| Variable | Para qué sirve |
|---|---|
| `TAMS_NEGOCIO_CONNECTION_STRING` | Cadena de conexión del esquema `negocio` (módulo TAMS) |
| `TAMS_CORE_CONNECTION_STRING` | Cadena de conexión del esquema `core` (Control de acceso) |
| `TAMS_JWT_SECRETO` | Clave de firma de los tokens JWT de sesión (Base64, mínimo 32 bytes) |
| `TAMS_SMTP_HOST` | Host del servidor SMTP |
| `TAMS_SMTP_PORT` | Puerto del servidor SMTP |
| `TAMS_SMTP_USUARIO` | Usuario de autenticación SMTP |
| `TAMS_SMTP_CONTRASENA` | Contraseña o token de autenticación SMTP |
| `TAMS_SMTP_REMITENTE` | Dirección "De" con la que se envían los correos |

> **Nota:** Solo se documenta el nombre y propósito; nunca el valor.

### Servidor SMTP

- Cualquier SMTP que entregue a destinatarios arbitrarios sirve.
- Se recomienda Gmail con contraseña de aplicación (requiere verificación en 2 pasos; se genera en [myaccount.google.com/apppasswords](https://myaccount.google.com/apppasswords)):
  - `TAMS_SMTP_HOST=smtp.gmail.com`
  - `TAMS_SMTP_PORT=587`
  - `TAMS_SMTP_USUARIO` y `TAMS_SMTP_REMITENTE` = la misma dirección de Gmail
  - `TAMS_SMTP_CONTRASENA` = la clave de 16 letras **SIN espacios**
- El dominio de demostración de Mailtrap solo entrega al dueño de la cuenta ("Demo domains can only be used to send emails to account owners"), así que solo sirve para registrar ese correo.
- Para definir la contraseña sin dejarla en el historial de PowerShell:
  ```powershell
  $sec = Read-Host -AsSecureString
  [System.Environment]::SetEnvironmentVariable("TAMS_SMTP_CONTRASENA", [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($sec)), "User")
  ```

## Cómo arrancar el proyecto

```powershell
git clone https://github.com/soleyniesandovals-rgb/TAMS.git
cd TAMS
# define las 8 variables de entorno de arriba, cierra y abre VS Code

dotnet build TAMS.slnx
dotnet test TAMS.slnx   # 70 pruebas en verde

# aplica las migraciones (crea las tablas si no existen)
dotnet ef database update --project src/Tams.Negocio --startup-project src/Tams.Api
dotnet ef database update --project src/Core --startup-project src/Tams.Api

cd src\Tams.Api
dotnet run
```

> Usa `dotnet run` (perfil `http`), no F5 desde Visual Studio: el perfil
> `https` redirige y rompe los enlaces de activación/recuperación, que
> apuntan a `http://localhost:5132`.

La API queda escuchando en `http://localhost:5132`.

## Crear el primer Administrador

El registro siempre crea cuentas con rol Estándar, y nadie puede autopromoverse (es una regla de diseño: así nunca se puede quedar el sistema sin Administradores por un cambio de rol accidental). Para tener el primer Administrador en una base limpia, sigue estos pasos exactos:

1. Define las variables de entorno (`TAMS_CORE_CONNECTION_STRING`, `TAMS_NEGOCIO_CONNECTION_STRING`, `TAMS_JWT_SECRETO`, `TAMS_SMTP_HOST`, `TAMS_SMTP_PORT`, `TAMS_SMTP_USUARIO`, `TAMS_SMTP_CONTRASENA`, `TAMS_SMTP_REMITENTE`), cierra y abre VS Code.
2. Compila y aplica las migraciones:
   ```powershell
   dotnet build TAMS.slnx
   dotnet ef database update --project src/Tams.Negocio --startup-project src/Tams.Api
   dotnet ef database update --project src/Core --startup-project src/Tams.Api
   ```
3. Arranca la API: `cd src\Tams.Api; dotnet run` (perfil `http`).
4. Regístrate normalmente con `POST /api/cuentas/registro`. Usa un correo REAL que tu servidor SMTP acepte (no `admin@ejemplo.com`). Ejemplo completo en PowerShell:
   ```powershell
   $body = @{ nombre = "Admin"; correo = "TU_CORREO"; contrasena = "Admin123!" } | ConvertTo-Json
   Invoke-RestMethod -Method Post -Uri http://localhost:5132/api/cuentas/registro -ContentType "application/json" -Body $body
   ```
5. Procesa la cola de correos para enviar el correo de activación:
   ```powershell
   cd src\Tams.Api
   dotnet run -- procesar-correos
   ```
6. Activa la cuenta abriendo el enlace recibido (`GET /api/cuentas/activar?codigo=...`) o usando el código. Verifica con `POST /api/cuentas/login` antes/después si lo deseas. Ejemplo de login en PowerShell:
   ```powershell
   $r = Invoke-RestMethod -Method Post -Uri http://localhost:5132/api/cuentas/login -ContentType "application/json" -Body (@{ correo = "TU_CORREO"; contrasena = "Admin123!" } | ConvertTo-Json)
   $h = @{ Authorization = "Bearer $($r.token)" }
   ```
7. Asciéndela manualmente, una sola vez, con SQL (usando la misma conexión que `TAMS_CORE_CONNECTION_STRING`). Ejecuta el `UPDATE` con SQL Server Object Explorer de Visual Studio, SSMS o `sqlcmd`, conectado a la base indicada por `TAMS_CORE_CONNECTION_STRING`. La columna `Rol` guarda texto (`'Administrador'` o `'Estandar'`, sin tilde). Usa el mismo correo que registraste:
   ```sql
   UPDATE core.Usuarios SET Rol = 'Administrador' WHERE Correo = 'TU_CORREO';
   ```
   Después del `UPDATE` hay que iniciar sesión de nuevo con `POST /api/cuentas/login` para obtener un token con el rol Administrador, porque el token anterior conserva el rol viejo.
   A partir de ese momento ya puedes usar un token de Administrador para las operaciones protegidas.

> Si la base está completamente limpia y acabas de crearla, tras los pasos anteriores solo ese usuario queda con rol `Administrador` y el resto se registrarán como `Estándar`. Nadie puede autopromoverse.

## Procesar la cola de correos

Los correos (activación, recuperación de contraseña) nunca se envían dentro de la misma operación que los origina: se encolan en `core.CorreosEnCola` y un comando aparte los envía.

```powershell
cd src\Tams.Api
dotnet run -- procesar-correos
```

Ejecutarlo varias veces no duplica envíos (es idempotente). Se puede correr con el servidor SMTP apagado: la operación que generó el correo ya terminó bien, y el correo queda pendiente hasta que el SMTP vuelva a estar disponible.

## Pruebas

Para ejecutar **toda** la suite desde un clon limpio (un solo comando):

```powershell
dotnet test TAMS.slnx
```

| Proyecto | Casos | Qué cubre |
|---|---:|---|
| `tests/Tests.Core` | 5 | Core / Control de acceso |
| `tests/Tests.Permisos` | 20 | Servicio de permisos con dobles en memoria y `TimeProvider` (RD-12) |
| `tests/Tests.Arquitectura` | 1 | Regla arquitectónica RD-03: Core no depende de `Tams.Negocio` |
| `tests/Tests.Negocio` | 40 | Suite de la máquina de estados del negocio, solo dominio |
| `tests/Tests.Integracion` | 4 | Integración con `GestionPermisosServicio` y `CalificacionesServicio` reales; solo la persistencia es EF InMemory |
| **Total** | **70** | |

Ninguna prueba necesita SQL Server encendido ni variables de entorno: las suites unitarias usan objetos en memoria y `Tests.Integracion` usa el proveedor InMemory de EF Core. `Tests.Core` no lee configuración ni cadenas de conexión.

**Resultado esperado desde un clon limpio:** `Correctas!` en los cinco proyectos y **0 errores**.

## Defectos (Práctica 2)

| # | Defecto | Prueba que lo detecta | Comportamiento que comprueba | Archivo | Línea donde se planta el defecto |
|---|---|---|---|---|---|
| 1 | El filtro de pendientes usa `"estado es Pendiente"` → `"no es Rechazada"`. | `Tests.Permisos.ListarSolicitudesTests.RF_GP_02_ListarPendientes_Administrador_SoloRecibeLasPendientes` | Siembra solicitudes en `Pendiente`, `Aprobada`, `Aplicada` y `Rechazada`. Con el filtro correcto, solo la solicitud `Pendiente` debe aparecer; si se devuelve también `Aprobada`/`Aplicada` (distintas de `Rechazada`), la prueba falla (solo llega `Pendiente`). | `src/Core/Permisos/Application/Servicios/GestionPermisosServicio.cs` | 72 |
| 2 | Agregar transición `Aplicada → Aprobada`. | `Tests.Permisos.TransicionesTests.RF_GP_06_Aprobar_DesdeAplicada_TransicionProhibidaYEstadoNoCambia` y `Tests.Permisos.TransicionesTests.RF_GP_06_Tabla_Aplicada_NoPermitePasarAAprobada` | Una solicitud en `Aplicada` no debe poder aprobarse de nuevo. La primera prueba espera `TransicionNoPermitidaExcepcion` al aprobar y que el estado siga `Aplicada`; la segunda verifica en `TransicionesPermiso` que `Aplicada` es terminal (`[]`) y `EsValida(Aplicada, Aprobada)=false`. Si `Aplicada` pasa a `Aprobada`, ambas fallan. | `src/Core/Permisos/Domain/Maquinas/TransicionesPermiso.cs` | 38 |
| 3 | Quitar la llamada que aplica el permiso al aprobar. | `Tests.Permisos.AplicarPermisoTests.RF_GP_05_Aprobar_AplicaElPermisoYDejaLaSolicitudAplicada` | Al aprobar se debe aplicar el permiso en la misma operación: tras aprobar, `TienePermisoAsync` debe ser `true`, la solicitud debe quedar `Aplicada` y debe crearse un `PermisoActivo` con `UsuarioId`, `TipoPermiso` y `SolicitudPermisoId`. Si se omite la concesión del permiso, la prueba falla. | `src/Core/Permisos/Application/Servicios/GestionPermisosServicio.cs` | 101 |
| 4 | En rechazar, `"estado es Pendiente"` → `"estado no es Aplicada"`. | `Tests.Permisos.TransicionesTests.RF_GP_06_Rechazar_DesdeAprobada_TransicionProhibidaYEstadoNoCambia` y `Tests.Permisos.TransicionesTests.RF_GP_06_Rechazar_DesdeRechazada_TransicionProhibidaYEstadoNoCambia` | Al rechazar desde `Aprobada` o desde `Rechazada` se debe lanzar `TransicionNoPermitidaExcepcion` y el estado no debe cambiar. La condición errónea solo bloquea `Aplicada`, por lo que rechazar desde `Aprobada` o `Rechazada` se permitiría indebidamente y las pruebas fallarían. | `src/Core/Permisos/Application/Servicios/GestionPermisosServicio.cs` | 127 |
| 5 | Estado inicial `Pendiente` → `Aprobada`. | `Tests.Permisos.CrearSolicitudTests.RF_GP_01_Crear_Solicitud_NacePendiente`, `Tests.Permisos.AprobarSolicitudTests.RF_GP_03_Aprobar_RegistraQuienResuelveYCuando`, `Tests.Permisos.AplicarPermisoTests.RF_GP_05_Aprobar_AplicaElPermisoYDejaLaSolicitudAplicada`, `Tests.Permisos.AutoResolucionTests.RF_GP_07_Aprobar_PropiaSolicitud_LanzaReglaNegocio409YEstadoNoCambia`, `Tests.Permisos.AutoResolucionTests.RF_GP_07_Rechazar_PropiaSolicitud_LanzaReglaNegocio409YEstadoNoCambia` | Al crear la solicitud debe nacer en `Pendiente` (sin resolutor, sin fecha, sin motivo). Si nace en `Aprobada`, las pruebas que esperan `Pendiente`, que verifican trazabilidad/resolución o que esperan `ReglaNegocioExcepcion` por auto-resolución/transiciones fallan según corresponda. | `src/Core/Permisos/Application/Servicios/GestionPermisosServicio.cs` | 47 |
| 6 | Quitar la verificación de auto-resolución **solo en rechazar**. | `Tests.Permisos.AutoResolucionTests.RF_GP_07_Rechazar_PropiaSolicitud_LanzaReglaNegocio409YEstadoNoCambia` | El Administrador no puede resolver su propia solicitud ni al rechazarla. Debe lanzarse `ReglaNegocioExcepcion` con código **409** y mensaje `"No puedes resolver tu propia solicitud."`, con la solicitud manteniéndose en `Pendiente` y sin motivo guardado. Si se elimina solo la llamada en `RechazarAsync`, esta prueba falla. | `src/Core/Permisos/Application/Servicios/GestionPermisosServicio.cs` | 118 |

## Cómo provocar cada criterio de aceptación

### Control de acceso

**Registro y activación**
- `POST /api/cuentas/registro` con un correo nuevo → se crea inactiva. Repetir con el mismo correo → se rechaza.
- `POST /api/cuentas/login` antes de activar → rechazado, indica cuenta inactiva.
- Procesar la cola, abrir el enlace recibido (`GET /api/cuentas/activar?codigo=...`) → activa la cuenta.
- Abrir el mismo enlace una segunda vez → se rechaza.
- `POST /api/cuentas/reenviar` con un correo que existe y con uno que no → misma respuesta en ambos casos.

**Sesión**
- `POST /api/cuentas/login` con credenciales correctas → devuelve un JWT.
- Con contraseña incorrecta y con correo inexistente → mismo mensaje de rechazo en los dos casos.
- `GET /api/cuentas/yo` sin token → 401. Con token válido → datos del usuario.
- `POST /api/cuentas/logout` y luego reusar el mismo token en `/yo` → 401 (la sesión quedó invalidada).
- 5 logins seguidos con contraseña incorrecta, luego un sexto intento (aunque la contraseña sea correcta) → rechazado, bloqueo de 15 minutos.

**Roles y administración** (requieren un token de Administrador, ver sección "Crear el primer Administrador")
- Con un token de Estándar, invocar `GET /api/cuentas` o `PUT /api/cuentas/{id}/rol` directamente (sin pasar por ninguna interfaz) → 403.
- `PUT /api/cuentas/{id}/rol` como Administrador, sobre otro usuario → cambia el rol.
- Un Administrador intentando cambiarse su propio rol → 409.
- `POST /api/cuentas/{id}/desactivar` → el usuario no puede iniciar sesión y su sesión abierta queda invalidada.
- Un Administrador intentando desactivarse a sí mismo → 409.
- `GET /api/cuentas` → lista sin hashes ni tokens.

**Recuperación de contraseña**
- `POST /api/cuentas/recuperacion/iniciar` con un correo que existe y con uno que no → misma respuesta.
- Procesar la cola, recibir el código, `POST /api/cuentas/recuperacion/confirmar` → cambia la contraseña. Reusar el mismo código → se rechaza.
- Login con la contraseña anterior tras confirmar → 401. Con la nueva → funciona.
- `POST /api/cuentas/{id}/restablecer` como Administrador → la contraseña anterior del usuario deja de servir de inmediato, incluso antes de que confirme un código nuevo.
- `POST /api/cuentas/cambiar-contrasena` con sesión, indicando la actual → cambia la contraseña y cierra las sesiones anteriores.

**Correo por cola**
- Apagar el acceso al SMTP (o usar credenciales inválidas) y registrar un usuario → la operación termina bien, el correo queda "Pendiente".
- Ejecutar `dotnet run -- procesar-correos` dos veces seguidas → no duplica el envío.

**Máquina de estados de negocio**
- Ver `docs/maquina-de-estados.md`: tabla de transiciones de `Calificacion`, con la transición prohibida (`Publicada → Borrador`) y el estado terminal (`Cerrada`).

## Cómo provocar cada criterio de aceptación RF-GP

Todos los casos usan los endpoints reales de `PermisosController` (ruta `api/permisos`). Requiere un token JWT válido para el actor indicado.

> Nota: `Invoke-RestMethod` lanza excepción en respuestas no-2xx; para obtener el código exacto conviene usar `Invoke-WebRequest` con `try/catch`. En la tabla se indica el código HTTP esperado para cada caso.

### RF-GP-01: crear solicitud de permiso
- Método: `POST`
- Ruta: `/api/permisos`
- Autenticado como: **Estandar** (cualquier usuario autenticado)
- Body (JSON):
  ```json
    { "TipoPermiso": "acceso-reportes" }
  ```
- Código esperado: **201** (devuelve resumen de la solicitud creada; `estado = "Pendiente"`)

### RF-GP-02: el Administrador consulta las solicitudes pendientes; un Estándar no ve las solicitudes de otros usuarios
1. Como Administrador: `GET /api/permisos/pendientes` → **200**, con las solicitudes en estado `Pendiente` (y solo esas).
2. Como Estándar: `GET /api/permisos/pendientes` → **403** (el atributo `[RequiereRol(RolUsuario.Administrador)]` bloquea antes del servicio).
3. Como Estándar: `GET /api/permisos` → **200**, y solo aparecen las solicitudes creadas por ese usuario, no las de otros usuarios.

### RF-GP-03: el Administrador aprueba una solicitud pendiente y queda registrado quién aprobó y cuándo
1. Crear una solicitud pendiente como Estándar: `POST /api/permisos` con `{ "TipoPermiso": "acceso-reportes" }` → **201**.
2. Aprobarla como Administrador: `POST /api/permisos/{id}/aprobar` → **200**.
3. Comprobar quién aprobó y cuándo: revisar la respuesta `SolicitudPermisoResumen` devuelta en JSON con los campos: `id`, `solicitanteId`, `tipoPermiso`, `estado`, `fechaCreacion`, `resueltaPorId`, `fechaResolucion`, `motivoRechazo`. Tras la aprobación, como la misma operación aplica el permiso automáticamente, el `estado` devuelto es **`"Aplicada"`** (no `"Aprobada"`). `resueltaPorId` debe contener el id del Administrador que aprobó y `fechaResolucion` la fecha/hora en UTC en la que se resolvió (tomada del `TimeProvider` inyectado).

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
3. `POST /api/permisos/{id}/rechazar` sobre una solicitud en estado `Rechazada` → **409** (transición `Rechazada → Rechazada` no habilitada en la máquina de estados; `TransicionNoPermitidaExcepcion` propagada tal cual).

### RF-GP-07: nadie resuelve su propia solicitud
1. El propio Administrador crea una solicitud (`POST /api/permisos` autenticado como Administrador) → 201
2. El mismo Administrador intenta aprobarla (`POST /api/permisos/{id}/aprobar`) → **409** con mensaje `"No puedes resolver tu propia solicitud."` (chequeo antes de validar la transición)
3. El mismo Administrador intenta rechazarla (`POST /api/permisos/{id}/rechazar` con motivo) → **409** con el mismo mensaje.

## Defectos (Práctica 3)

| # | Defecto | Qué se planta | Archivo y línea exacta | Pruebas que lo detectan | Comportamiento que comprueban |
|---|---|---|---|---|---|
| 1 | `Publicada → Borrador` pasa a permitida. | Agregar `EstadoCalificacion.Borrador` a la lista de destinos de `Publicada`. | `src/Tams.Negocio/Domain/Maquinas/TransicionesCalificacion.cs`: línea 39 (cierre de la lista de destinos de `Publicada`). | `Tests.Negocio.TransicionesProhibidasTests.Transicion_Publicada_a_Borrador_lanzaExcepcion` y `Tests.Negocio.TransicionesProhibidasTests.Transicion_EntreCualquierParNoPermitido_lanzaExcepcionYNoCambiaElEstado(Publicada, Borrador)`. | La prueba concreta espera `TransicionNoPermitidaExcepcion` al intentar `Publicada → Borrador` y que el estado siga en `Publicada`; la teoría recorre todos los pares no permitidos. Con el defecto, la transición se acepta y ambas fallan. |
| 2 | Salida desde `Cerrada`. | Agregar un destino a la lista de salidas de `Cerrada`. | `src/Tams.Negocio/Domain/Maquinas/TransicionesCalificacion.cs`: línea 45. | `Tests.Negocio.TransicionesProhibidasTests.Transicion_Cerrada_a_CualquierEstado_lanzaExcepcion`, `Tests.Negocio.TransicionesProhibidasTests.Estado_Cerrada_esTerminal` y `Tests.Negocio.TransicionesProhibidasTests.Transicion_EntreCualquierParNoPermitido_lanzaExcepcionYNoCambiaElEstado(Cerrada, Borrador)`. | La teoría intenta salir de `Cerrada` hacia los cinco estados y espera excepción; `Estado_Cerrada_esTerminal` exige `EsTerminal(Cerrada)` y una lista de destinos vacía. Con una salida permitida, las tres pruebas fallan. |
| 3 | `Borrador → Enviada` sin verificar el rango 0 a 100. | Dejar la condición de `Borrador → Enviada` en `true`, sin comprobar `puntaje >= 0 && puntaje <= 100`. | `src/Tams.Negocio/Domain/Maquinas/TransicionesCalificacion.cs`: línea 91. | `Tests.Negocio.CondicionesTransicionTests.Transicion_Borrador_a_Enviada_conPuntajeFueraDeRango_lanzaExcepcion(100.01)` y `Tests.Negocio.CondicionesTransicionTests.Transicion_Borrador_a_Enviada_conPuntajeFueraDeRango_lanzaExcepcion(-1)`. | Con `100.01` y con `-1` espera `TransicionNoPermitidaExcepcion` y que el estado siga en `Borrador`; sin la comprobación, la transición se acepta y la prueba falla en ambos datos. |
| 4 | Se elimina la consulta a Gestión de permisos en `CorregirPuntajeAsync`. | Borrar la línea que llama a `permisos.TienePermisoAsync(...)` antes de modificar el puntaje. | `src/Tams.Negocio/Application/Servicios/CalificacionesServicio.cs`: línea 240. | En `Tests.Integracion`: `Tests.Integracion.CorregirPuntajeIntegracionTests.Corregir_sinSolicitudDePermiso_seRechazaYElPuntajeNoCambia`, `Tests.Integracion.CorregirPuntajeIntegracionTests.Corregir_conSolicitudPendiente_seRechazaYElPuntajeNoCambia` y `Tests.Integracion.CorregirPuntajeIntegracionTests.Corregir_conSolicitudRechazada_seRechazaYElPuntajeNoCambia`. | Las tres esperan `ReglaNegocioExcepcion` con código **403** y que el puntaje siga en 85. Al no consultar el permiso, la corrección continúa y las tres fallan. |

## Datos de demostración

El script `docs/datos-demo.sql` prepara un escenario mínimo para probar las operaciones de calificaciones. No crea usuarios ni contraseñas.

### Antes de ejecutar

1. Aplica las migraciones de los dos esquemas:
   ```powershell
   dotnet ef database update --project src/Tams.Negocio --startup-project src/Tams.Api
   dotnet ef database update --project src/Core --startup-project src/Tams.Api
   ```
2. Ten un docente ya registrado y activado en `core.Usuarios`, con rol `Estandar` (`Estándar` en la interfaz). El script no lo crea.
3. En SSMS, abre `docs/datos-demo.sql`, selecciona en la lista de bases la base de TAMS que contiene los esquemas `core` y `negocio`, y cambia la línea 16:
   ```sql
   DECLARE @CorreoDocente nvarchar(320) = N'correo.del.docente@ejemplo.com';
   ```
4. Ejecuta el script completo con **F5**. La consulta debe apuntar a esa misma base de TAMS.

Si el correo no existe en `core.Usuarios`, el script se detiene con el error **50001** y no inserta nada. Si la asignación de Matemática y el curso ya pertenece a otro docente, se detiene con el error **50002**.

El script es **idempotente**: se puede ejecutar varias veces sin duplicar los datos. Reutiliza el centro, el año escolar activo, el curso, las materias, los estudiantes y la asignación si ya existen.

### Qué crea en una base limpia

| Dato | Filas | Id en una base limpia |
|---|---:|---|
| Centro `Centro Demo TAMS` | 1 | 1 |
| Año escolar activo `2026-2027` | 1 | 1 |
| Curso `1er Grado`, sección `A` | 1 | 1 |
| Materias: `Matemática` (no técnica) y `Programación` (técnica, 3 RA) | 2 | 1 y 2 |
| Estudiantes: `Ana Pérez` y `Luis Gómez`, en ese curso | 2 | 1 y 2 |
| Asignación del docente a `Matemática` en el curso, 4 horas semanales | 1 | 1 |

El script no crea calificaciones: esas se registran con la API. Si la base no está limpia, reutiliza los registros existentes y los ids pueden ser distintos de 1 y 2.

## Estructura del proyecto

```
src/
  Core/              Especificación fija del curso (Control de acceso)
    Permisos/        Módulo de gestión de permisos: solicitudes y permisos activos (RF-GP-01 a RF-GP-07) (en src/Core/Permisos)
  Tams.Negocio/       Módulo de negocio: gestión académica
  Tams.Api/          API web, punto de entrada único
tests/
  Tests.Arquitectura/ Verifica que Core no dependa de Tams.Negocio (RD-03)
  Tests.Core/         Pruebas del Core (Control de acceso)
  Tests.Permisos/     Pruebas unitarias del servicio de permisos (dobles en memoria, RD-12)
docs/
  maquina-de-estados.md
  bitacora-asignacion-1.md
  bitacora-fase0-diagrama.md
  bitacora-practica-1.md
  auditoria-practica2-permisos.md
```

---

## Diagrama de componentes

```mermaid
%%{init: {
  "theme": "base",
  "themeVariables": {
    "background": "#ffffff",
    "primaryTextColor": "#1f2937",
    "lineColor": "#64748b",
    "fontFamily": "Arial"
  },
  "flowchart": {
    "curve": "basis",
    "htmlLabels": true,
    "nodeSpacing": 35,
    "rankSpacing": 55
  }
}}%%

flowchart TB

    subgraph APP["TAMS — un solo contenedor"]
        direction TB

        TAMSAPP["<b>TAMS</b><br/>Gestión de operaciones académicas<br/><i>C# · módulo de negocio</i>"]

        subgraph INTERFACES["Interfaces provistas por el Core"]
            direction LR
            IACC["IControlAcceso"]
            IPERM["IPermisos"]
            INOTI["INotificaciones"]
            IDOC["IDocumentos"]
            IREP["IReportes"]
            IAUD["IAuditoria"]
        end

        subgraph CORE["CORE — especificación fija"]
            direction LR

            subgraph SEGURIDAD["Seguridad"]
                direction TB
                ACC["<b>Control de acceso</b><br/>Autentica y asigna rol<br/><i>C# · capa de servicio</i>"]
                PERM["<b>Gestión de permisos</b><br/>Resuelve solicitudes de acceso elevado<br/><i>C# · capa de servicio</i>"]
                AUD["<b>Auditoría</b><br/>Registra quién hizo qué y cuándo<br/><i>C# · solo lectura/escritura</i>"]
            end

            subgraph SERVICIOS["Servicios funcionales"]
                direction TB
                NOTI["<b>Notificaciones</b><br/>Entrega avisos internos y por correo<br/><i>C# · capa de servicio</i>"]
                DOC["<b>Manejador de documentos</b><br/>Sube, lista y borra documentos<br/><i>C# · capa de servicio</i>"]
                REPO["<b>Reportes</b><br/>Agrega datos filtrados por rol<br/><i>C# · capa de servicio</i>"]
            end

            subgraph CORREO["Cola de correos"]
                direction TB
                COLA["<b>Cola de correos</b><br/>Guarda correos pendientes<br/><i>C# · tabla CorreoEnCola</i>"]
                PROC["<b>Procesador de correos</b><br/>Envía correos con reintentos<br/><i>BackgroundService</i>"]
            end
        end
    end

    MAIL(["Servidor de correo<br/>(sistema externo)"])

    %% TAMS -> interfaces
    TAMSAPP -->|"quién es y qué rol"| IACC
    TAMSAPP -->|"crea y consulta sus solicitudes"| IPERM
    TAMSAPP -->|"notificar evento propio"| INOTI
    TAMSAPP -->|"adjuntar documentos (opcional)"| IDOC
    TAMSAPP -->|"alimenta reporte de negocio"| IREP
    TAMSAPP -.->|"registrar evento"| IAUD

    %% interfaces -> Core
    IACC --> ACC
    IPERM --> PERM
    INOTI --> NOTI
    IDOC --> DOC
    IREP --> REPO
    IAUD --> AUD

    %% dependencias internas del Core, un solo sentido
    PERM -->|"consulta identidad y rol"| ACC
    PERM -->|"avisa al solicitante"| NOTI
    DOC -->|"valida autorización"| ACC
    REPO -->|"valida rol de quien consulta"| ACC
    ACC -->|"solicita aviso de recuperación"| NOTI

    ACC -.->|"registra evento"| AUD
    PERM -.->|"registra evento"| AUD
    DOC -.->|"registra evento"| AUD

    NOTI -->|"encola correo"| COLA
    COLA -->|"entrega pendientes"| PROC
    PROC -->|"envía correo"| MAIL

    classDef tams fill:#dcfce7,stroke:#15803d,stroke-width:3px,color:#14532d;
    classDef interface fill:#fef3c7,stroke:#d97706,stroke-width:2px,color:#78350f;
    classDef core fill:#dbeafe,stroke:#2563eb,stroke-width:2px,color:#1e3a8a;
    classDef external fill:#ffedd5,stroke:#ea580c,stroke-width:2px,color:#7c2d12;

    class TAMSAPP tams;
    class IACC,IPERM,INOTI,IDOC,IREP,IAUD interface;
    class ACC,PERM,AUD,NOTI,DOC,REPO,COLA,PROC core;
    class MAIL external;
```

### Cómo leer el diagrama

| Elemento | Significado |
|---|---|
| Recuadro verde (`TAMS`) | El módulo de negocio: tu dominio, tus entidades y tu máquina de estados. |
| Recuadros amarillos | Las interfaces que expone el Core. Es lo único que TAMS puede usar de él. |
| Recuadros azules | Las seis piezas del Core, con su responsabilidad y su tecnología. |
| Recuadro naranja (`Servidor de correo`) | Un sistema externo, fuera de la aplicación. |
| Flecha sólida (`-->`) | Una llamada directa: quien envía la flecha necesita una respuesta de quien la recibe para completar su operación. |
| Flecha punteada (`-.->`) | Un registro que no bloquea el flujo: se avisa a Auditoría, pero la operación no depende de esa respuesta. |
| Texto sobre la flecha | El propósito de esa comunicación, no el nombre técnico del método. |

No se dibuja ninguna base de datos ni volumen como componente: cada pieza
del Core guarda sus propios datos, de forma independiente y sin que otra
pieza pueda leerlos directamente (RD-01, RD-03).
