# ADR 0001: decisiones de la Práctica 4

## Contexto

La Práctica 4 necesitaba integrar una API externa de feriados y dejar el
sistema ejecutable con un solo comando en un clon limpio. Las decisiones
debían conservar las reglas de las prácticas anteriores (RD-03, RD-09,
RD-10, RD-12), no exponer credenciales y permitir que un evaluador probara
todo el flujo sin depender de una cuenta SMTP ni de una clave de terceros.

El código ya estaba implementado y fusionado en `main` mediante el PR #43;
este ADR documenta las decisiones que ese código materializa.

## Decisión

### Dos `DbContext` sobre una misma base, con esquemas separados

El Core y el módulo de negocio conservan un `DbContext` cada uno, pero ambos
apuntan a la misma base de datos `Tams`. El Core usa el esquema `core` y el
negocio usa el esquema `negocio` (RD-03). Cada contexto mantiene su propia
tabla de historial de migraciones, de modo que las migraciones se aplican de
forma independiente y ninguna capa puede leer directamente las tablas de la
otra. La API recibe las dos cadenas de conexión desde variables de entorno
(RD-10); en Docker, `docker-compose.yml` las construye a partir de
`MSSQL_SA_PASSWORD` y del nombre interno del servicio `db`.

### Documentos en un volumen nombrado

Los archivos subidos se guardan en `TAMS_DOCUMENTOS_RUTA` y, en el
contenedor, esa ruta es `/app/almacen`, respaldada por el volumen nombrado
`tams_documentos` (RF-DOC-05). La base de datos solo conserva los metadatos y
el nombre físico; `docker compose down` conserva el volumen y
`docker compose down -v` lo elimina junto con los datos de SQL Server. Esto
evita que un reinicio de los contenedores pierda los archivos y mantiene
separados los datos relacionales de los binarios.

### Migraciones automáticas al arrancar, con reintentos

`MigradorBaseDatos` aplica las migraciones de Core y de Negocio al iniciar la
API (RD-09). Repite el intento hasta 30 veces, con dos segundos de espera,
para dar tiempo a que SQL Server termine de iniciar. La aplicación no registra
la cadena de conexión. El mensaje de preparación que se usa en el README es
`Migraciones de Core y Negocio aplicadas correctamente en el intento N.`

### Modo sin SMTP basado en `.env.example`

El enviador se selecciona al arrancar: si `TAMS_SMTP_HOST` está vacío, se usa
`EnviadorCorreoLog`, que escribe destinatario, asunto y cuerpo en el log y
marca el correo como enviado. `.env.example` deja todas las variables SMTP
vacías, de modo que un clon limpio puede probar registro, activación y
recuperación sin una cuenta de correo. Para el envío real solo hay que llenar
`TAMS_SMTP_*` en `.env`; las credenciales nunca se versionan (RD-10).

### Servicio `correos` con bucle de 15 segundos

`procesar-correos` es un comando de una sola pasada: procesa los correos
pendientes, marca los enviados y termina. No es un `BackgroundService` con
reintentos en memoria. El servicio `correos` del contenedor ejecuta ese
comando y espera 15 segundos antes de repetirlo. Si el SMTP no responde, el
comando termina con código 1, el correo vuelve a `Pendiente` y la siguiente
pasada lo reintenta. Esta separación conserva la operación original ya
confirmada y evita duplicados porque el reclamo se hace con una actualización
condicionada al estado `Pendiente`.

### Calendarific como proveedor de feriados

Se eligió Calendarific sobre las alternativas evaluadas:

- **Abstract API**: su plan gratuito exige año, mes y día, lo que obligaría a
  una llamada por día o a un uso muy restringido del plan.
- **HolidayAPI.com**: el plan gratuito solo cubre años pasados y no permite
  uso comercial.
- **API Ninjas**: el plan gratuito solo cubre el año actual, insuficiente
  para un año escolar que cruza de 2026 a 2027.
- **Calendarific**: permite consultar por año y país con el plan gratuito y
  devuelve los feriados nacionales necesarios para descontar días lectivos.
  La atribución exigida es «Datos de feriados proporcionados por Calendarific
  (calendarific.com)».

La clave se lee en cada llamada, no al arrancar. Así la API funciona sin
`TAMS_API_FERIADOS_CLAVE` y solo el endpoint de días lectivos responde 503
cuando falta; una clave inválida o una caída del proveedor se traduce a 502.
El cálculo hace una llamada por año calendario incluido en el rango y resta
los feriados nacionales distintos que caen de lunes a viernes.

### `[RequiereRol]` acepta un solo rol

`RequiereRolAttribute` hereda de `AuthorizeAttribute` y fija `Roles` a un
único valor; `AllowMultiple` es `false`. Por eso no se puede exigir
«Administrador o Estándar» con dos atributos. El endpoint de días lectivos se
declara solo para **Administrador**, que es el rol que puede consultar los
años escolares; un usuario Estándar recibe 403.

## Alternativas consideradas

- **Una sola base con un solo `DbContext`**: se descartó para conservar la
  separación Core/negocio y las reglas arquitectónicas de RD-03.
- **Guardar los documentos en la base de datos**: se descartó porque el
  volumen ya resuelve la persistencia y evita mezclar binarios con las tablas
  del negocio.
- **Migrar manualmente antes de arrancar**: se descartó porque un clon limpio
  debía funcionar con `docker compose up -d --build` sin pasos de EF Core.
- **Exigir SMTP desde el primer arranque**: se descartó porque impediría
  probar el flujo completo sin credenciales externas.
- **Un `BackgroundService` con reintentos**: se descartó para no añadir un
  proceso en memoria que compita con el comando y para mantener el
  procesamiento como una operación explícita y repetible.
- **Varios roles en `[RequiereRol]`**: se descartó porque el atributo actual
  no lo soporta; el endpoint queda restringido a Administrador y se documenta
  la limitación.

## Consecuencias

- El evaluador solo necesita Docker para arrancar y probar el sistema; los
  datos y documentos sobreviven a `docker compose down`.
- Sin SMTP, los enlaces y códigos quedan en el log de `correos`; no se envía
  nada a Internet.
- Sin clave de Calendarific, el resto de la API sigue operativo y el cálculo
  de días lectivos falla con un error controlado 503. Para obtener la
  respuesta 200 hace falta una clave gratuita y conexión a Internet.
- El año escolar debe crearse por SQL porque no hay endpoint de alta; el
  README deja el `INSERT` y advierte de la columna `FechaCreacion` y del
  índice único de año activo.
- La duplicación de `Duarte's Day` en 2027 es una limitación de la fuente y
  se descuenta dos veces si ambos días son hábiles; está documentada para que
  no se interprete como un error del cálculo.
- Cualquier cambio futuro en los roles o en el proveedor de feriados debe
  actualizar este ADR y las pruebas correspondientes.
