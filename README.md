# TAMS
Sistema de Gesti├│n Acad├®mica Docente para la administraci├│n de asignaciones, cursos, horarios y calificaciones. Proyecto de Programaci├│n III.

## Requisitos previos

- .NET SDK 10
- SQL Server (local o accesible)
- La herramienta de migraciones de EF Core: `dotnet tool install --global dotnet-ef`
- Una cuenta de Mailtrap (o cualquier servidor SMTP) para el env├¡o de correos

## Variables de entorno

Ninguna credencial est├í en el repositorio (RD-10). Def├¡nelas con:

```powershell
[System.Environment]::SetEnvironmentVariable("NOMBRE","valor","User")
```

Cierra y abre VS Code por completo despu├®s de definir o cambiar cualquiera de estas.

| Variable | Para qu├® sirve |
|---|---|
| `TAMS_NEGOCIO_CONNECTION_STRING` | Cadena de conexi├│n del esquema `negocio` (m├│dulo TAMS) |
| `TAMS_CORE_CONNECTION_STRING` | Cadena de conexi├│n del esquema `core` (Control de acceso) |
| `TAMS_JWT_SECRETO` | Clave de firma de los tokens JWT de sesi├│n (Base64, m├¡nimo 32 bytes) |
| `TAMS_SMTP_HOST` | Host del servidor SMTP |
| `TAMS_SMTP_PORT` | Puerto del servidor SMTP |
| `TAMS_SMTP_USUARIO` | Usuario de autenticaci├│n SMTP |
| `TAMS_SMTP_CONTRASENA` | Contrase├▒a o token de autenticaci├│n SMTP |
| `TAMS_SMTP_REMITENTE` | Direcci├│n "De" con la que se env├¡an los correos |

## C├│mo arrancar el proyecto

```powershell
git clone https://github.com/soleyniesandovals-rgb/TAMS.git
cd TAMS
# define las 8 variables de entorno de arriba, cierra y abre VS Code

dotnet build TAMS.slnx
dotnet test TAMS.slnx   # prueba de arquitectura (RD-03): Passed: 1

# aplica las migraciones (crea las tablas si no existen)
dotnet ef database update --project src/Tams.Negocio --startup-project src/Tams.Api
dotnet ef database update --project src/Core --startup-project src/Tams.Api

cd src\Tams.Api
dotnet run
```

> Usa `dotnet run` (perfil `http`), no F5 desde Visual Studio: el perfil
> `https` redirige y rompe los enlaces de activaci├│n/recuperaci├│n, que
> apuntan a `http://localhost:5132`.

La API queda escuchando en `http://localhost:5132`.

## Crear el primer Administrador

El registro siempre crea cuentas con rol Est├índar, y nadie puede autopromoverse (es una regla de dise├▒o: as├¡ nunca se puede quedar el sistema sin Administradores por un cambio de rol accidental). Para tener el primer Administrador:

1. Reg├¡strate normalmente: `POST /api/cuentas/registro`.
2. Procesa la cola de correos y activa la cuenta con el enlace recibido (ver abajo).
3. Asci├®ndela manualmente, una sola vez, con SQL:

```sql
UPDATE core.Usuarios SET Rol = 'Administrador' WHERE Correo = 'tu_correo@ejemplo.com';
```

## Procesar la cola de correos

Los correos (activaci├│n, recuperaci├│n de contrase├▒a) nunca se env├¡an dentro de la misma operaci├│n que los origina: se encolan en `core.CorreosEnCola` y un comando aparte los env├¡a.

```powershell
cd src\Tams.Api
dotnet run -- procesar-correos
```

Ejecutarlo varias veces no duplica env├¡os (es idempotente). Se puede correr con el servidor SMTP apagado: la operaci├│n que gener├│ el correo ya termin├│ bien, y el correo queda pendiente hasta que el SMTP vuelva a estar disponible.

## Pruebas

Para ejecutar toda la suite de pruebas desde un clon limpio:

```powershell
dotnet test TAMS.slnx
```

- Las pruebas de `tests/Tests.Permisos` usan implementaciones falsas en memoria (`ICoreUnidadDeTrabajo`, repositorios y `TimeProvider`), por lo que **no requieren SQL Server encendido ni variables de entorno** (RD-12).
- Las pruebas arquitectónicas se ejecutan junto a las unitarias en el mismo comando.

## C├│mo provocar cada criterio de aceptaci├│n

**Registro y activaci├│n**
- `POST /api/cuentas/registro` con un correo nuevo ÔåÆ se crea inactiva. Repetir con el mismo correo ÔåÆ se rechaza.
- `POST /api/cuentas/login` antes de activar ÔåÆ rechazado, indica cuenta inactiva.
- Procesar la cola, abrir el enlace recibido (`GET /api/cuentas/activar?codigo=...`) ÔåÆ activa la cuenta.
- Abrir el mismo enlace una segunda vez ÔåÆ se rechaza.
- `POST /api/cuentas/reenviar` con un correo que existe y con uno que no ÔåÆ misma respuesta en ambos casos.

**Sesi├│n**
- `POST /api/cuentas/login` con credenciales correctas ÔåÆ devuelve un JWT.
- Con contrase├▒a incorrecta y con correo inexistente ÔåÆ mismo mensaje de rechazo en los dos casos.
- `GET /api/cuentas/yo` sin token ÔåÆ 401. Con token v├ílido ÔåÆ datos del usuario.
- `POST /api/cuentas/logout` y luego reusar el mismo token en `/yo` ÔåÆ 401 (la sesi├│n qued├│ invalidada).
- 5 logins seguidos con contrase├▒a incorrecta, luego un sexto intento (aunque la contrase├▒a sea correcta) ÔåÆ rechazado, bloqueo de 15 minutos.

**Roles y administraci├│n** (requieren un token de Administrador, ver arriba)
- Con un token de Est├índar, invocar `GET /api/cuentas` o `PUT /api/cuentas/{id}/rol` directamente (sin pasar por ninguna interfaz) ÔåÆ 403.
- `PUT /api/cuentas/{id}/rol` como Administrador, sobre otro usuario ÔåÆ cambia el rol.
- Un Administrador intentando cambiarse su propio rol ÔåÆ 409.
- `POST /api/cuentas/{id}/desactivar` ÔåÆ el usuario no puede iniciar sesi├│n y su sesi├│n abierta queda invalidada.
- Un Administrador intentando desactivarse a s├¡ mismo ÔåÆ 409.
- `GET /api/cuentas` ÔåÆ lista sin hashes ni tokens.

**Contrase├▒as**
- `POST /api/cuentas/recuperacion/iniciar` con un correo que existe y con uno que no ÔåÆ misma respuesta.
- Procesar la cola, recibir el c├│digo, `POST /api/cuentas/recuperacion/confirmar` ÔåÆ cambia la contrase├▒a. Reusar el mismo c├│digo ÔåÆ se rechaza.
- Login con la contrase├▒a anterior tras confirmar ÔåÆ 401. Con la nueva ÔåÆ funciona.
- `POST /api/cuentas/{id}/restablecer` como Administrador ÔåÆ la contrase├▒a anterior del usuario deja de servir de inmediato, incluso antes de que confirme un c├│digo nuevo.
- `POST /api/cuentas/cambiar-contrasena` con sesi├│n, indicando la actual ÔåÆ cambia la contrase├▒a y cierra las sesiones anteriores.

**Correo por cola**
- Apagar el acceso al SMTP (o usar credenciales inv├ílidas) y registrar un usuario ÔåÆ la operaci├│n termina bien, el correo queda "Pendiente".
- Ejecutar `dotnet run -- procesar-correos` dos veces seguidas ÔåÆ no duplica el env├¡o.

**M├íquina de estados de negocio**
- Ver `docs/maquina-de-estados.md`: tabla de transiciones de `Calificacion`, con la transici├│n prohibida (`Publicada ÔåÆ Borrador`) y el estado terminal (`Cerrada`).

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
- Aclaración: `GET /api/permisos` (sin `/pendientes`) es accesible para **cualquier usuario autenticado** (Estandar o Administrador) y siempre devuelve **solo las solicitudes propias** del que consulta; no debe confundirse con el endpoint de pendientes, que es exclusivo del Administrador.

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

## Estructura del proyecto

```
src/
  Core/              Especificaci├│n fija del curso (Control de acceso)
  Permisos/          MÃ³dulo de gestiÃ³n de permisos: solicitudes y permisos activos (RF-GP-01 a RF-GP-07)
  Tams.Negocio/       M├│dulo de negocio: gesti├│n acad├®mica
  Tams.Api/          API web, punto de entrada ├║nico
tests/
  Tests.Arquitectura/ Verifica que Core no dependa de Tams.Negocio (RD-03)
  Tests.Permisos/     Pruebas unitarias del servicio de permisos (dobles en memoria, RD-12)
docs/
  maquina-de-estados.md
  bitacora-asignacion-1.md
  bitacora-fase0-diagrama.md
  bitacora-practica-1.md
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

    subgraph APP["TAMS ÔÇö un solo contenedor"]
        direction TB

        TAMSAPP["<b>TAMS</b><br/>Gesti├│n de operaciones acad├®micas<br/><i>C# ┬À m├│dulo de negocio</i>"]

        subgraph INTERFACES["Interfaces provistas por el Core"]
            direction LR
            IACC["IControlAcceso"]
            IPERM["IPermisos"]
            INOTI["INotificaciones"]
            IDOC["IDocumentos"]
            IREP["IReportes"]
            IAUD["IAuditoria"]
        end

        subgraph CORE["CORE ÔÇö especificaci├│n fija"]
            direction LR

            subgraph SEGURIDAD["Seguridad"]
                direction TB
                ACC["<b>Control de acceso</b><br/>Autentica y asigna rol<br/><i>C# ┬À capa de servicio</i>"]
                PERM["<b>Gesti├│n de permisos</b><br/>Resuelve solicitudes de acceso elevado<br/><i>C# ┬À capa de servicio</i>"]
                AUD["<b>Auditor├¡a</b><br/>Registra qui├®n hizo qu├® y cu├índo<br/><i>C# ┬À solo lectura/escritura</i>"]
            end

            subgraph SERVICIOS["Servicios funcionales"]
                direction TB
                NOTI["<b>Notificaciones</b><br/>Entrega avisos internos y por correo<br/><i>C# ┬À capa de servicio</i>"]
                DOC["<b>Manejador de documentos</b><br/>Sube, lista y borra documentos<br/><i>C# ┬À capa de servicio</i>"]
                REPO["<b>Reportes</b><br/>Agrega datos filtrados por rol<br/><i>C# ┬À capa de servicio</i>"]
            end

            subgraph CORREO["Cola de correos"]
                direction TB
                COLA["<b>Cola de correos</b><br/>Guarda correos pendientes<br/><i>C# ┬À tabla CorreoEnCola</i>"]
                PROC["<b>Procesador de correos</b><br/>Env├¡a correos con reintentos<br/><i>BackgroundService</i>"]
            end
        end
    end

    MAIL(["Servidor de correo<br/>(sistema externo)"])

    %% TAMS -> interfaces
    TAMSAPP -->|"qui├®n es y qu├® rol"| IACC
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
    DOC -->|"valida autorizaci├│n"| ACC
    REPO -->|"valida rol de quien consulta"| ACC
    ACC -->|"solicita aviso de recuperaci├│n"| NOTI

    ACC -.->|"registra evento"| AUD
    PERM -.->|"registra evento"| AUD
    DOC -.->|"registra evento"| AUD

    NOTI -->|"encola correo"| COLA
    COLA -->|"entrega pendientes"| PROC
    PROC -->|"env├¡a correo"| MAIL

    classDef tams fill:#dcfce7,stroke:#15803d,stroke-width:3px,color:#14532d;
    classDef interface fill:#fef3c7,stroke:#d97706,stroke-width:2px,color:#78350f;
    classDef core fill:#dbeafe,stroke:#2563eb,stroke-width:2px,color:#1e3a8a;
    classDef external fill:#ffedd5,stroke:#ea580c,stroke-width:2px,color:#7c2d12;

    class TAMSAPP tams;
    class IACC,IPERM,INOTI,IDOC,IREP,IAUD interface;
    class ACC,PERM,AUD,NOTI,DOC,REPO,COLA,PROC core;
    class MAIL external;
```

### C├│mo leer el diagrama

| Elemento | Significado |
|---|---|
| Recuadro verde (`TAMS`) | El m├│dulo de negocio: tu dominio, tus entidades y tu m├íquina de estados. |
| Recuadros amarillos | Las interfaces que expone el Core. Es lo ├║nico que TAMS puede usar de ├®l. |
| Recuadros azules | Las seis piezas del Core, con su responsabilidad y su tecnolog├¡a. |
| Recuadro naranja (`Servidor de correo`) | Un sistema externo, fuera de la aplicaci├│n. |
| Flecha s├│lida (`-->`) | Una llamada directa: quien env├¡a la flecha necesita una respuesta de quien la recibe para completar su operaci├│n. |
| Flecha punteada (`-.->`) | Un registro que no bloquea el flujo: se avisa a Auditor├¡a, pero la operaci├│n no depende de esa respuesta. |
| Texto sobre la flecha | El prop├│sito de esa comunicaci├│n, no el nombre t├®cnico del m├®todo. |

No se dibuja ninguna base de datos ni volumen como componente: cada pieza
del Core guarda sus propios datos, de forma independiente y sin que otra
pieza pueda leerlos directamente (RD-01, RD-03).
