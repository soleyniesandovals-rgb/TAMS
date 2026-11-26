# Criterios de aceptación — Historial de auditoría (Asignación 4)

Cada criterio cita el requisito que cubre y dice cómo se comprueba. Los criterios de esta asignación se numeran CA-01 a CA-23 para no confundirlos con los C-xx de la Práctica 5 ni con los requisitos RF-AUD-xx. Salvo que se indique otra cosa, todas las operaciones exigen sesión válida: sin token → 401 (RD-06). Los errores se devuelven como `{ "mensaje": "..." }`, sin trazas, nombres de tipos internos, rutas como `$.campo` ni `traceId`, incluidos los errores de enlazado (RD-07, RD-08). Las fechas y horas se guardan y se devuelven en UTC con `Z`, tomando «ahora» del `TimeProvider` inyectado (RD-11).

## Ruta y formato

- `GET /api/auditoria?usuarioId=&entidad=&desde=AAAA-MM-DD&hasta=AAAA-MM-DD` — consulta del historial, solo Administrador. Todos los filtros son opcionales y se pueden combinar.
- Cada registro devuelve: `id`, `usuarioId` (quien actuó), `accion`, `entidad`, `entidadId`, `fechaHora`, `valorAnterior` y `valorNuevo`.
- Entidades: `SolicitudPermiso`, `Documento`, `Usuario`.
- Acciones: `AprobarSolicitud`, `RechazarSolicitud`, `SubirDocumento`, `EliminarDocumento`, `CambiarRol`, `DesactivarUsuario`, `ReactivarUsuario`, `RestablecerContrasena`, `RecuperarContrasena`, `CambiarContrasena`.
- `valorAnterior` y `valorNuevo` son textos cortos y legibles; pueden ir vacíos (`null`) cuando no hay un estado anterior o nuevo que mostrar.

## Registro

**CA-01 (RF-AUD-01).** Dado cualquiera de los eventos de CA-02 a CA-10, cuando la operación termina con éxito, entonces existe exactamente un registro nuevo con `usuarioId` de quien actuó, `accion`, `entidad`, `entidadId`, `fechaHora` en UTC y los valores anterior y nuevo indicados en cada criterio. *Se comprueba:* consulta como Administrador; una prueba por evento.

## Eventos

**CA-02 (RF-AUD-02).** Dado una solicitud pendiente, cuando un Administrador la aprueba, entonces se registra `AprobarSolicitud` sobre `SolicitudPermiso` con el id de la solicitud, `usuarioId` del Administrador, `valorAnterior` = `Pendiente` y `valorNuevo` = `Aplicada (<tipo de permiso>)`. *Se comprueba:* consulta; prueba unitaria.

**CA-03 (RF-AUD-02).** Dado una solicitud pendiente, cuando un Administrador la rechaza con un motivo, entonces se registra `RechazarSolicitud` con `valorAnterior` = `Pendiente` y `valorNuevo` = `Rechazada: <motivo>`. *Se comprueba:* consulta; prueba unitaria.

**CA-04 (RF-AUD-03).** Cuando un usuario sube un documento, entonces se registra `SubirDocumento` sobre `Documento` con el id del documento, `usuarioId` de quien lo subió, `valorAnterior` vacío y `valorNuevo` = `<nombre> (<tipo>, <tamaño> bytes)`. *Se comprueba:* consulta; prueba unitaria.

**CA-05 (RF-AUD-03).** Cuando el dueño o un Administrador elimina un documento (borrado lógico), entonces se registra `EliminarDocumento` con `usuarioId` de quien lo eliminó, `valorAnterior` = `Activo` y `valorNuevo` = `Eliminado`. *Se comprueba:* consulta; prueba unitaria.

**CA-06 (RF-AUD-04).** Cuando un Administrador cambia el rol de otro usuario, entonces se registra `CambiarRol` sobre `Usuario` con el id del afectado, `usuarioId` del Administrador, `valorAnterior` = el rol anterior y `valorNuevo` = el rol nuevo. Si el usuario ya tenía ese rol, la operación no cambia nada y no se registra. *Se comprueba:* consulta; prueba unitaria.

**CA-07 (RF-AUD-04).** Cuando un Administrador desactiva a un usuario, entonces se registra `DesactivarUsuario` con `valorAnterior` = `Activo` y `valorNuevo` = `Inactivo`; cuando lo reactiva, se registra `ReactivarUsuario` con `Inactivo` → `Activo`. En los dos casos `usuarioId` es el del Administrador, que el servicio recibe desde el token. *Se comprueba:* consulta; pruebas unitarias.

**CA-08 (RF-AUD-05).** Cuando un Administrador fuerza el restablecimiento de la contraseña de un usuario, entonces se registra `RestablecerContrasena` sobre `Usuario` con el id del afectado, `usuarioId` del Administrador (recibido desde el token), `valorAnterior` vacío y `valorNuevo` = `Contraseña restablecida por un Administrador`. *Se comprueba:* consulta; prueba unitaria.

**CA-09 (RF-AUD-05).** Cuando un usuario confirma la recuperación de su contraseña con un código válido (sin sesión), entonces se registra `RecuperarContrasena` sobre `Usuario` con su id, `usuarioId` = el propio usuario, `valorAnterior` vacío y `valorNuevo` = `Contraseña restablecida por el propio usuario`. *Se comprueba:* consulta; prueba unitaria.

**CA-10 (RF-AUD-05).** Cuando un usuario con sesión cambia su propia contraseña, entonces se registra `CambiarContrasena` sobre `Usuario` con su id, `usuarioId` = el propio usuario, `valorAnterior` vacío y `valorNuevo` = `Contraseña cambiada por el propio usuario`. *Se comprueba:* consulta; prueba unitaria.

**CA-11 (RF-AUD-05, RD-10).** Ningún registro guarda la contraseña, su hash ni el código de recuperación, en ningún campo. *Se comprueba:* pruebas unitarias que comprueban que esos valores no aparecen en el registro; revisión del código.

**CA-12 (RF-AUD-01).** El registro se guarda junto con la operación auditada: en el mismo guardado de la unidad de trabajo del Core o, cuando el id de la entidad todavía no existe porque lo asigna la base al guardar (la subida de documentos), dentro de una misma transacción de la base. En los dos casos, si algo falla, no queda ni la operación ni el registro. *Se comprueba:* revisión del código; prueba unitaria de la subida en la que falla el registro y no queda ni el documento ni el archivo.

**CA-13 (RF-AUD-01).** Solo se auditan las operaciones que terminan con éxito. Una operación rechazada (409, 403, 400, contraseña actual incorrecta, código usado o vencido) no crea ningún registro. *Se comprueba:* pruebas unitarias con una operación rechazada.

## Inmutabilidad

**CA-14 (RF-AUD-06).** El historial no se puede modificar ni eliminar desde la aplicación: la entidad no cambia después de creada, el repositorio solo agrega y consulta, y la API solo expone `GET /api/auditoria` (no hay PUT, PATCH ni DELETE). Los registros existentes no cambian cuando ocurren operaciones posteriores. *Se comprueba:* prueba de que las interfaces y el controlador de auditoría no tienen operaciones de editar ni borrar; prueba de que un registro sigue igual tras otra operación; `PUT` o `DELETE` sobre la ruta responden 405.

## Consulta

**CA-15 (RF-AUD-07).** Dado registros de varios usuarios, entidades y fechas, cuando un Administrador consulta `GET /api/auditoria` sin filtros, entonces recibe los registros con todos sus campos, del más reciente al más antiguo, como máximo 500. *Se comprueba:* consulta; prueba unitaria.

**CA-16 (RF-AUD-07).** Los filtros `usuarioId`, `entidad`, `desde` y `hasta` se pueden usar solos o combinados, y cada uno reduce el resultado a lo que cumple. `desde` y `hasta` incluyen los dos extremos (cuenta cualquier hora del día `hasta`). Dos rangos distintos sobre los mismos datos dan resultados distintos. Un filtro sin coincidencias responde 200 con una lista vacía. *Se comprueba:* consultas; pruebas unitarias.

**CA-17 (RF-AUD-07, RD-06).** Un usuario Estándar que consulta el historial recibe 403, aunque construya la petición a mano; sin token, 401. *Se comprueba:* consulta como Estándar y sin token.

**CA-18 (RD-07).** Entradas inválidas responden 400 con un mensaje claro: `usuarioId` que no es un entero positivo; `entidad` que no es exactamente uno de sus nombres (sin distinguir mayúsculas; un número como `0` o una combinación con comas como `Usuario,Documento` también es inválido); fecha con formato distinto de `AAAA-MM-DD`; o `desde` posterior a `hasta`. Las fechas extremas con formato válido (`0001-01-01`, `9999-12-31`) nunca provocan un error 500: responden 200. *Se comprueba:* consultas; pruebas unitarias.

## Diseño

**CA-19 (RD-09).** Los registros persisten fuera del proceso: siguen ahí después de reiniciar el contenedor. *Se comprueba:* consulta antes y después de `docker compose down` y `up -d`.

**CA-20 (RD-01, RD-03).** El historial vive en la pieza Auditoría del Core, con su propia tabla en el esquema `core` (no como campos de otras entidades), y el Core sigue sin depender de `Tams.Negocio`. *Se comprueba:* revisión del código y la prueba de arquitectura en verde.

**CA-21 (RD-12).** Cada criterio de CA-01 a CA-18 que dice «prueba» tiene al menos una prueba que corre sin levantar la aplicación ni usar SQL Server. *Se comprueba:* `dotnet test TAMS.slnx`.

## Lo que ya funcionaba

**CA-22 (revisión acumulada).** Las 212 pruebas existentes siguen pasando con `dotnet test TAMS.slnx` (solo se ajustan construcciones y dobles si cambia una firma, sin tocar aserciones), y los seis frentes del README siguen funcionando en el contenedor.

**CA-23 (README).** El README tiene una sección «Auditoría» que explica qué eventos se registran y con qué valores, cómo provocar cada evento y ver su registro, cómo consultar con cada filtro y cómo provocar cada error, y por qué el historial no se puede modificar. Los comandos funcionan en PowerShell 5.1 contra el contenedor, siguiendo el README en orden, sin fechas fijas que dependan del día en que se ejecute (la fecha de hoy se calcula en UTC) y con los comandos de cada código de error.

## Fuera de alcance (decisiones documentadas)
- Los eventos del módulo de negocio (calificaciones, asignaciones) no se auditan en esta asignación.
- Los intentos rechazados no se registran (CA-13).
- La inmutabilidad se garantiza en la aplicación; no se añade un trigger en la base.

## Cambios durante la implementación
- CA-12: se añadió el caso de la transacción. En la subida de documentos, el id del documento lo asigna la base al guardar, así que el registro no puede ir en el mismo guardado; se decidió guardar el documento y su registro dentro de una misma transacción (bloque B3, commit `d82940d`), que mantiene la regla «o los dos o ninguno».