# Criterios de aceptación — Notificaciones, cola de correos y reportes (Práctica 5)

Cada criterio cita el requisito que cubre y dice cómo se comprueba. Salvo que se indique otra cosa, todas las operaciones exigen sesión válida: sin token → 401 (RD-06). Los errores se devuelven como `{ "mensaje": "..." }` sin trazas (RD-08). Las fechas y horas de las respuestas nuevas van en UTC con `Z` (RD-11).

## Rutas acordadas
- `GET /api/notificaciones` — notificaciones propias.
- `POST /api/notificaciones/{id}/leida` — marcar una propia como leída.
- `GET /api/notificaciones/preferencias` y `PUT /api/notificaciones/preferencias` con `{ "correoActivo": true|false }`.
- `GET /api/correos?estado=` — cola de correos (solo Administrador).
- `GET /api/reportes/solicitudes-permiso?desde=AAAA-MM-DD&hasta=AAAA-MM-DD`
- `GET /api/reportes/asistencia?desde=AAAA-MM-DD&hasta=AAAA-MM-DD`

Cada notificación devuelve: `id`, `tipo`, `titulo`, `mensaje`, `leida`, `fechaCreacion`, `fechaLectura`. Tipos: `SolicitudCreada`, `SolicitudAprobada`, `SolicitudRechazada`, `AsignacionCreada`. El correo de una notificación usa el título como asunto y el mensaje como cuerpo.

## Notificaciones

**C-01 (RF-NOT-01).** Dado dos Administradores activos, un Administrador desactivado y dos usuarios Estándar, cuando un Estándar crea una solicitud de permiso, entonces cada Administrador activo tiene exactamente una notificación nueva sin leer de tipo `SolicitudCreada` que nombra el tipo de permiso y al solicitante, y ni el Administrador desactivado ni ningún Estándar la tiene. *Se comprueba:* `GET /api/notificaciones` con cada usuario; prueba unitaria.

**C-02 (RF-NOT-01).** Dado que no hay ningún Administrador activo, cuando un Estándar crea una solicitud, entonces la solicitud se crea igual (201) y no se crea ninguna notificación. *Se comprueba:* prueba unitaria.

**C-03 (RF-NOT-02).** Dado una solicitud pendiente, cuando un Administrador la aprueba, entonces el solicitante tiene una notificación `SolicitudAprobada` que nombra el tipo de permiso; cuando un Administrador rechaza otra con un motivo, entonces el solicitante tiene una notificación `SolicitudRechazada` cuyo mensaje incluye ese motivo exacto. Nadie más recibe esas notificaciones. *Se comprueba:* `GET /api/notificaciones` del solicitante y del otro Estándar; pruebas unitarias.

**C-04 (RF-NOT-01, 02, 08).** Dado cualquiera de los eventos anteriores, la solicitud, sus notificaciones y sus correos se guardan en la misma operación: si el guardado falla, no queda ninguno de ellos. *Se comprueba:* revisión del código (un solo guardado de la unidad de trabajo del Core).

**C-05 (RF-NOT-03).** Dado un docente Estándar activo, cuando un Administrador crea una asignación de una materia a ese docente en un curso, entonces el docente tiene una notificación `AsignacionCreada` cuyo mensaje nombra la materia, el grado, la sección y las horas semanales, y ningún otro usuario la tiene. El evento está descrito en el README con los pasos para provocarlo. *Se comprueba:* `GET /api/notificaciones` del docente; prueba unitaria. *Limitación aceptada:* la asignación se guarda primero (en la base del negocio) y después la notificación (en la del Core); si la notificación falla, la asignación queda creada y el fallo se registra en el log.

**C-06 (RF-NOT-04).** Dado que un usuario tiene notificaciones leídas y no leídas, cuando consulta `GET /api/notificaciones`, entonces recibe solo las suyas, de la más reciente a la más antigua, cada una con `leida` true o false, y no recibe ninguna de otro usuario. *Se comprueba:* consulta con dos usuarios distintos; prueba unitaria.

**C-07 (RF-NOT-05).** Dado una notificación propia sin leer, cuando el usuario la marca como leída, entonces responde 200, la notificación queda con `leida = true` y `fechaLectura` registrada; marcarla otra vez responde 200 sin cambiar la fecha. *Se comprueba:* `POST .../leida` y `GET /api/notificaciones`; prueba unitaria.

**C-08 (RF-NOT-05).** Dado una notificación de otro usuario, cuando un usuario intenta marcarla como leída, entonces responde 404 «La notificación no existe.» (no revela que existe) y la notificación del otro sigue sin leer. Un id inexistente también responde 404. *Se comprueba:* con dos usuarios; prueba unitaria.

**C-09 (RF-NOT-06).** Dado un usuario con el correo activado (valor por defecto), cuando recibe una notificación, entonces aparece en su bandeja interna y además se encola un correo a su dirección en `core.CorreosEnCola` con estado `Pendiente`. *Se comprueba:* bandeja y `GET /api/correos` como Administrador; prueba unitaria.

**C-10 (RF-NOT-07).** Dado un usuario que puso `correoActivo = false`, cuando recibe una notificación, entonces aparece en su bandeja interna y no se encola ningún correo para ella. Al volver a `true`, las siguientes notificaciones encolan correo otra vez. `GET /api/notificaciones/preferencias` devuelve el valor actual (true si nunca lo cambió). *Se comprueba:* bandeja y cola antes y después; pruebas unitarias.

**C-11 (RF-NOT-07).** La preferencia solo afecta a las notificaciones: con el correo desactivado, los correos de cuenta (activación y recuperación de contraseña) se siguen encolando. *Se comprueba:* pedir recuperación con el correo desactivado y ver el código en el log; prueba unitaria.

**C-12 (RD-01, RD-03).** Las notificaciones y la preferencia se guardan en tablas propias de la pieza Notificaciones del Core (no como campos de `Usuario`), y el Core sigue sin depender de `Tams.Negocio`. *Se comprueba:* revisión del código y la prueba de arquitectura en verde.

## Cola de correos

**C-13 (RF-NOT-08).** Dado un servidor de correo que no responde, cuando un usuario realiza una operación que encola correo (registro, crear o resolver una solicitud, crear una asignación), entonces la operación responde con su código normal de éxito y el correo queda `Pendiente`. *Se comprueba:* prueba unitaria con un enviador que falla; README con el procedimiento manual.

**C-14 (RF-NOT-09).** Los correos los envía solo el comando `procesar-correos` (en el contenedor, el servicio `correos` cada 15 s), sin volver a ejecutar la operación que los creó. *Se comprueba:* revisión del código; log del servicio `correos`.

**C-15 (RF-NOT-10).** Dado un correo cuyo envío falla siempre, cuando el procesador se ejecuta tres veces, entonces el correo tiene `intentos = 3`, estado `Fallido`, `ultimoError` con un texto corto (sin traza) y la fecha del último intento; en ejecuciones posteriores no se vuelve a intentar. Después de uno o dos fallos queda `Pendiente` con sus intentos contados. El máximo de 3 intentos está en una sola constante y declarado en el README. *Se comprueba:* pruebas unitarias; procedimiento manual del README (SMTP apagado).

**C-16 (RF-NOT-10).** Dado tres correos pendientes donde el segundo falla, cuando el procesador se ejecuta una vez, entonces el primero y el tercero quedan `Enviado` y el segundo `Pendiente` con un intento: un fallo no corta la pasada. *Se comprueba:* prueba unitaria.

**C-17 (RF-NOT-11).** Dado correos en los tres estados, cuando un Administrador consulta `GET /api/correos`, entonces recibe por cada correo: `id`, `destinatario`, `asunto`, `estado`, `intentos`, `fechaCreacion`, `fechaUltimoIntento`, `fechaEnvio` y `ultimoError`; con `?estado=Fallido` recibe solo los fallidos; un estado inválido responde 400. El cuerpo del correo no se incluye (puede tener enlaces o códigos). Un Estándar recibe 403 y sin token 401. *Se comprueba:* consulta como Administrador y como Estándar; pruebas unitarias.

**C-18 (RF-NOT-12).** Dado correos pendientes, cuando el procesador se ejecuta dos veces seguidas, entonces cada correo se envía una sola vez. *Se comprueba:* prueba unitaria que cuenta las llamadas al enviador; las pruebas existentes de la cola siguen pasando.

**C-19 (RF-NOT-13, RD-10).** Las credenciales SMTP solo se leen de variables de entorno: no aparecen en ningún archivo del repositorio ni en el historial. *Se comprueba:* `git log --all -S` con el valor de cada credencial (sin resultados).

## Reportes

**C-20 (RF-REP-01, 03).** Dado solicitudes de permiso en distintos estados y meses, cuando un Administrador consulta el reporte de solicitudes con un rango, entonces recibe una fila por combinación de mes (`AAAA-MM`, según la fecha de creación en UTC) y estado, con su `cantidad`, y no el listado de solicitudes: con 6 solicitudes en 2 meses y 2 estados, a lo sumo 4 filas. La agrupación la hace la base de datos (`GROUP BY`), no un listado completo resumido en memoria. *Se comprueba:* consulta y prueba unitaria; revisión del código.

**C-21 (RF-REP-02, 03).**  Dado registros de asistencia, cuando se consulta el reporte de asistencia con un rango, entonces responde «¿qué porcentaje de asistencia tiene cada estudiante en cada materia?»: una fila por estudiante y asignación con `materiaNombre`, `grado`, `seccion`, `nombre`, `apellido`, `presentes`, `tardanzas`, `ausencias`, `excusas`, `total` y `porcentajeAsistencia` = (presentes + tardanzas) / total × 100, redondeado a 2 decimales. No incluye la cédula. *Se comprueba:* con datos conocidos (por ejemplo, 3 presentes, 1 tardanza y 1 ausencia → 80.00); prueba unitaria.

**C-22 (RF-REP-04).** Dado solicitudes de dos Estándar distintos, cuando un Estándar consulta el reporte de solicitudes, entonces los conteos incluyen solo sus propias solicitudes. Dado asignaciones de dos docentes, cuando un docente consulta el reporte de asistencia, entonces solo aparecen sus asignaciones. El Administrador ve todo en ambos. *Se comprueba:* consulta con dos usuarios; pruebas unitarias.

**C-23 (RF-REP-05).** Dado los mismos datos, cuando se consulta cada reporte con dos rangos distintos, entonces los resultados son distintos. *Se comprueba:* dos consultas; prueba unitaria.

**C-24 (RF-REP-05).** El rango incluye **los dos extremos**: un registro con fecha igual a `hasta` (en cualquier hora de ese día) aparece en el resultado, y uno del día siguiente no. *Se comprueba:* prueba unitaria con un registro el último día del rango.

**C-25 (RD-07).** En los dos reportes, `desde` y `hasta` son obligatorios con formato `AAAA-MM-DD`; si falta alguno, el formato es inválido o `desde` es posterior a `hasta`, responde 400 con un mensaje claro. Un rango sin datos responde 200 con una lista vacía. *Se comprueba:* consultas; pruebas unitarias.

## Lo que ya funcionaba

**C-26 (revisión acumulada).** Las 147 pruebas existentes siguen pasando con `dotnet test TAMS.slnx`, y los seis frentes del README siguen funcionando en el contenedor.

**C-27 (README).** El README tiene una sección «Notificaciones» (cómo provocar cada evento, cómo ejecutar el procesador, cómo provocar un fallo de envío y el máximo de intentos) y una sección «Reportes» (qué pregunta responde el reporte del negocio y cómo consultar los dos con un rango de fechas), con comandos que funcionan en PowerShell 5.1 contra el contenedor.

## Fuera de alcance (limitación documentada)
Si el proceso se cae entre el envío y el marcado, el correo queda en `Enviando` y no se recupera solo (haría falta un tiempo de espera que el enunciado no pide).