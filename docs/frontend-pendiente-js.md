# Lo pendiente de JavaScript en la maqueta

La maqueta de `src/Tams.Api/wwwroot` era **solo HTML y CSS**: no había archivos `.js`, etiquetas `<script>` ni atributos `on*`. El bloque F1 ya conectó `usuario-actual`, `contador-notificaciones` y `cerrar-sesion` en las 26 pantallas privadas, y el bloque F2 conectó las pantallas públicas de acceso y recuperación; las filas marcadas `F1 ✅` y `F2 ✅` están hechas. Las ventanas se abren con `:target`, los avisos usan `hidden` y los botones no envían nada. Cada elemento marcado con `data-js` es, por tanto, un punto donde el frontend definitivo tendrá que conectar comportamiento. En las tablas siguientes, la columna «qué debe hacer el JavaScript» reproduce los comentarios `<!-- JS: ... -->` de cada página; el endpoint se tomó de `src/Tams.Api/Controladores`. Cuando no existe o no está claro, se escribe `POR CONFIRMAR (F0)`.

## Públicas

### `index.html` — Iniciar sesión y registro

Las dos caras viven en la misma tarjeta: `#registro` desliza el panel con `:target`, sin JavaScript. Los `data-js` de ambas caras siguen aquí; el formulario de registro ya no está en `registro.html`.

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `login` | `form #form-login` | Validará correo y contraseña, llamará a la API de sesión y guardará el token para navegar al panel | `POST /api/cuentas/login` | F2 ✅ |
| `reenviar-activacion` | `a.enlace-publico` | Pedirá a la API que envíe de nuevo el correo de activación al usuario | `POST /api/cuentas/reenviar` | F2 ✅ |
| `registro` | `form.formulario-publico` de `.cara-registro` | Validará los campos, comprobará que las contraseñas coincidan y enviará el alta a la API | `POST /api/cuentas/registro` | F2 ✅ |

### `registro.html` — Registrarse (redirección)

Ya no contiene el formulario: `<meta http-equiv="refresh" content="0; url=index.html#registro">` lleva a la cara de registro de `index.html`, donde está el `data-js="registro"`.

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| — | — | No hay elementos `data-js`; el formulario está en `index.html` | No aplica | F2 ✅ |

### `recuperar-correo.html` — Recuperar contraseña: correo

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `recuperar-correo` | `form.formulario-publico` | Validará el correo y pedirá a la API que envíe el código de recuperación | `POST /api/cuentas/recuperacion/iniciar` | F2 ✅ |

### `recuperar-codigo.html` — Recuperar contraseña: código

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `recuperar-codigo` | `form.formulario-publico` | Guardará el código en `sessionStorage` y lo enviará en el paso 3 | Sin endpoint nuevo: `POST /api/cuentas/recuperacion/confirmar` en el paso 3 | F2 ✅ |

### `recuperar-nueva.html` — Recuperar contraseña: nueva contraseña

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `recuperar-nueva` | `form.formulario-publico` | Comprobará que ambas contraseñas coincidan y enviará el cambio a la API | `POST /api/cuentas/recuperacion/confirmar` | F2 ✅ |

### `guia.html` — Guía de estilos

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| — | — | No hay elementos `data-js`; la guía solo muestra componentes y enlaces | No aplica | F1 |

### `maqueta.html` — Maqueta de TAMS

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| — | — | No hay elementos `data-js`; la maqueta solo enlaza las pantallas | No aplica | F1 |

## Docente

### `docente/inicio.html` — Inicio

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el docente | `GET /api/notificaciones` | F1 ✅ |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del docente autenticado desde la API | `GET /api/cuentas/yo` | F1 ✅ |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F1 ✅ |
| `data-ejemplo` (saludo) | `h1.titulo-pagina span[data-ejemplo]` | Mostrará el nombre del docente autenticado en el saludo | `GET /api/cuentas/yo` | F4 ✅ |
| `proximos-eventos` | `ul#proximos-eventos` | Muestra los próximos eventos propios y los feriados desde hoy hasta el fin del año activo | `GET /api/eventos/proximos` | F3 ✅ |

### `docente/materias.html` — Materias

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el docente | `GET /api/notificaciones` | F1 ✅ |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del docente autenticado desde la API | `GET /api/cuentas/yo` | F1 ✅ |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F1 ✅ |
| `lista-asignaciones` | `div#lista-asignaciones` | Cargará las asignaciones del docente desde la API | `GET /api/docente/asignaciones` | F4 ✅ |
| `ver-asignacion` | `a#ver-asignacion-matematica`, `a#ver-asignacion-programacion` | Abrirá el desglose de la asignación con `:target` | No aplica (solo `:target`) | F4 ✅ |
| `pasar-lista` | `a#pasar-lista-matematica`, `a#pasar-lista-programacion` | Abrirá la lista de asistencia de esta asignación | No aplica (navegación a `asistencia-lista.html`) | F4 ✅ |
| `calificar` | `a#calificar-matematica`, `a#calificar-programacion` | Abrirá la pantalla de calificación de esta asignación | No aplica (navegación a `calificar.html`) | F4 ✅ |

### `docente/cursos.html` — Cursos

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el docente | `GET /api/notificaciones` | F1 ✅ |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del docente autenticado desde la API | `GET /api/cuentas/yo` | F1 ✅ |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F1 ✅ |
| `lista-cursos` | `div#lista-cursos` | Cargará los cursos y secciones asignados al docente desde la API | `GET /api/docente/asignaciones` | F4 ✅ |
| `ver-curso` | `a#ver-curso-a`, `a#ver-curso-b` | Abrirá el detalle de materias del curso con `:target` | No aplica (solo `:target`) | F4 ✅ |
| `ver-estudiantes` | `a#ver-estudiantes-a`, `a#ver-estudiantes-b` | Consultará los estudiantes de este curso en la API | `GET /api/docente/cursos/{cursoId}/estudiantes` | F4 ✅ |

### `docente/curso-estudiantes.html` — Estudiantes del curso

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el docente | `GET /api/notificaciones` | F1 ✅ |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del docente autenticado desde la API | `GET /api/cuentas/yo` | F1 ✅ |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F1 ✅ |
| `lista-estudiantes` | `div#lista-estudiantes` | Cargará de la API los estudiantes matriculados en el curso y sección elegidos | `GET /api/docente/cursos/{cursoId}/estudiantes` | F4 ✅ |

### `docente/asistencia.html` — Asistencia

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el docente | `GET /api/notificaciones` | F1 ✅ |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del docente autenticado desde la API | `GET /api/cuentas/yo` | F1 ✅ |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F1 ✅ |
| `lista-asistencia` | `div#lista-asistencia` | Carga los cursos con asignaciones de asistencia desde la API | `GET /api/docente/asignaciones` | F4 ✅ |
| `ver-materias-asistencia` | `a#ver-materias-asistencia-<cursoId>` | Abre las materias del curso para elegir cuál pasar lista | No aplica (solo `:target`) | F4 ✅ |

### `docente/asistencia-lista.html` — Lista de asistencia

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el docente | `GET /api/notificaciones` | F1 ✅ |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del docente autenticado desde la API | `GET /api/cuentas/yo` | F1 ✅ |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F1 ✅ |
| `asistencia-buscar` | `button#asistencia-buscar` | Pide a la API la asistencia del curso y la fecha elegida | `GET /api/docente/asistencias?asignacionId={id}&fecha={fecha}` | F4 ✅ |
| `aviso-edicion` | `p#aviso-edicion` | Muestra la fecha y hora local de la última edición devuelta por la API | `GET` y `PUT /api/docente/asistencias` | F4b ✅ |
| `resumen-asistencia` | `p#resumen-asistencia` | Calcula y actualiza los contadores con los radios marcados en la tabla | No aplica (estado de la pantalla) | F4 ✅ |
| `asistencia-estados` | `div#tabla-asistencia` | Carga los estudiantes y envía los estados y observaciones a la API | `GET` y `PUT /api/docente/asistencias` | F4 ✅ |
| `asistencia-editar` | `button#asistencia-editar` | Habilita la edición de la asistencia ya registrada | No aplica (cambio de modo) | F4 ✅ |
| `asistencia-enviar` | `button#asistencia-enviar` | Envía la asistencia a la API y muestra el mensaje de éxito | `PUT /api/docente/asistencias` | F4 ✅ |
| `mensaje-asistencia-enviada` | `p#mensaje-asistencia-enviada` | Muestra este mensaje cuando la API confirma el envío de la asistencia | No aplica (respuesta del `PUT`) | F4 ✅ |

### `docente/calificaciones.html` — Consultar calificaciones

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el docente | `GET /api/notificaciones` | F1 ✅ |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del docente autenticado desde la API | `GET /api/cuentas/yo` | F1 ✅ |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F1 ✅ |
| `filtros-calificaciones` | `form#filtros-calificaciones` | Carga las asignaciones y llena Curso, Sección y Materia; al enviar consulta la tabla de esa asignación | `GET /api/docente/asignaciones` y `GET /api/docente/asignaciones/{asignacionId}/calificaciones` | F4 ✅ |
| `consultar-calificaciones` | `button#consultar-calificaciones` | Ejecuta la consulta con los tres filtros y pinta la tabla de solo lectura | `GET /api/docente/asignaciones/{id}/calificaciones` | F4 ✅ |

### `docente/calificar.html` — Calificar

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el docente | `GET /api/notificaciones` | F1 ✅ |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del docente autenticado desde la API | `GET /api/cuentas/yo` | F1 ✅ |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F1 ✅ |
| `tabla-calificar` | `div#tabla-calificar` | Carga las notas y el estado de la columna actual de cada estudiante | `GET /api/docente/asignaciones/{id}/calificaciones` | F4 ✅ |
| `calificar-estudiante` | `a#calificar-<estudianteId>` | Abre la ventana del estudiante con sus columnas y notas | `GET /api/docente/asignaciones/{id}/calificaciones` | F4 ✅ |
| `enviar-todas` | `a#enviar-todas` | Abre la confirmación; solo se activa si hay notas en Borrador | No aplica (solo `:target`) | F4 ✅ |
| `publicar-todas` | `a#publicar-todas` | Abre la confirmación; solo se activa si hay notas Enviada | No aplica (solo `:target`) | F4 ✅ |
| `form-calificar` | `form#form-calificar-<estudianteId>` | Muestra la nota y el estado de la columna elegida; las opciones son «etiqueta (puntaje)» y en técnicas terminan en «· mín. N» | `GET /api/docente/asignaciones/{id}/calificaciones` | F4 ✅ |
| `borrador` | `button#borrador-<estudianteId>` | Registra el borrador o cambia su puntaje | `POST /api/calificaciones` (primera vez) y `PATCH /api/calificaciones/{id}/borrador` (cambios) | F4b2 ✅ |
| `enviar` | `button#enviar-<estudianteId>` | Envía la nota; si el borrador cambió, antes lo actualiza | `PATCH /api/calificaciones/{id}/borrador` (si cambia) y `POST /api/calificaciones/{id}/enviar` | F4b2 ✅ |
| `publicar` | `button#publicar-<estudianteId>` | Publica la nota enviada; muestra el mensaje de la API si no procede | `POST /api/calificaciones/{id}/publicar` | F4b2 ✅ |
| `recuperacion` | `button#recuperacion-<estudianteId>` | Envía una nota publicada a recuperación | `POST /api/calificaciones/{id}/recuperacion` | F4b2 ✅ |
| `cerrar` | `button#cerrar-<estudianteId>` | Botón visible «Cerrar nota»: cierra la nota; en recuperación envía la nota de recuperación. El enlace «Cerrar» solo cierra la ventana | `POST /api/calificaciones/{id}/cerrar` (sin cuerpo o con `puntajeRecuperacion`) | F4b2 ✅ |
| `corregir` | `button#corregir-<estudianteId>` | Corrige el puntaje de una nota publicada | `PATCH /api/calificaciones/{id}/puntaje` | F4b2 ✅ |
| `recuperacion-nota` | `input#recuperacion-nota-<estudianteId>` | Etiqueta «Recuperación:»; campo que se muestra solo en «En recuperación» para cerrar con nota de recuperación | `POST /api/calificaciones/{id}/cerrar` | F4b2 ✅ |
| `confirmar-envio-todas` | `a#confirmar-envio-todas` | Envía en serie todas las notas en Borrador y muestra el resumen | `POST /api/calificaciones/{id}/enviar` (uno por nota) | F4 ✅ |
| `confirmar-publicacion-todas` | `a#confirmar-publicacion-todas` | Publica en serie todas las notas Enviada y muestra el resumen | `POST /api/calificaciones/{id}/publicar` (uno por nota) | F4 ✅ |

**Nota (RF-NEG):** la pantalla solo debe ofrecer el periodo o RA que toca (el
siguiente al ya publicado), pero la regla la aplica la API al registrar: si
falta la anterior, responde **409** con
`{"mensaje":"Primero debe estar publicada la nota del Pn."}` (o `RA...`). La
validación de la pantalla no debe ser la única barrera.

### `docente/solicitudes.html` — Solicitudes

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el docente | `GET /api/notificaciones` | F1 ✅ |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del docente autenticado desde la API | `GET /api/cuentas/yo` | F1 ✅ |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F1 ✅ |
| `nueva-solicitud` | `button#nueva-solicitud` | Abrirá la ventana para crear una solicitud nueva | No aplica (solo `:target`) | F4 ✅ |
| `lista-solicitudes` | `div#lista-solicitudes` | Cargará las solicitudes del docente desde la API | `GET /api/permisos` | F4 ✅ |
| `ver-solicitud` | `a#ver-solicitud-1`, `a#ver-solicitud-2` | Abrirá el detalle de la solicitud con `:target` | No aplica (solo `:target`) | F4 ✅ |
| `crear-solicitud` | `form#form-nueva-solicitud` | Validará el tipo y creará la solicitud mediante la API; sin motivo: la API solo guarda el tipo (decisión de la autora, 26 nov) | `POST /api/permisos` | F4 ✅ |
| `enviar-solicitud` | `button#enviar-solicitud` | Enviará la solicitud nueva a la API | `POST /api/permisos` | F4 ✅ |

### `docente/documentos.html` — Documentos

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el docente | `GET /api/notificaciones` | F1 ✅ |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del docente autenticado desde la API | `GET /api/cuentas/yo` | F1 ✅ |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F1 ✅ |
| `subir-documentos` | `button#subir-documentos` | Abrirá la ventana para seleccionar y subir archivos | No aplica (solo `:target`) | F3 ✅ |
| `lista-documentos` | `div#lista-documentos` | Cargará los documentos del docente desde la API | `GET /api/documentos` | F3 ✅ |
| `descargar-documento` | `a#descargar-1`, `a#descargar-2`, `a#descargar-3` | Descargará el archivo desde la API | `GET /api/documentos/{id}/descargar` | F3 ✅ |
| `eliminar-documento` | `a#eliminar-1`, `a#eliminar-2`, `a#eliminar-3` | Abrirá la ventana de confirmación antes de eliminar | No aplica (solo `:target`) | F3 ✅ |
| `mensaje-sin-documentos` | `p#mensaje-sin-documentos` | Mostrará este aviso cuando la API devuelva una lista vacía | No aplica (respuesta del `GET`) | F3 ✅ |
| `form-subir-documentos` | `form#form-subir-documentos` | Validará los archivos elegidos y los enviará a la API | `POST /api/documentos` | F3 ✅ |
| `seleccionar-archivos` | `input#archivos-documento` | Tomará la lista de archivos del input y mostrará sus nombres | No aplica (lectura local) | F3 ✅ |
| `archivos-elegidos` | `ul#archivos-elegidos` | Mostrará los nombres de los archivos seleccionados en el input | No aplica (solo vista) | F3 ✅ |
| `aceptar-documentos` | `button#aceptar-documentos` | Subirá los archivos seleccionados a la API | `POST /api/documentos` | F3 ✅ |
| `confirmar-eliminar-documento` | `a#confirmar-eliminar-1`, `a#confirmar-eliminar-2`, `a#confirmar-eliminar-3` | Eliminará el documento mediante la API | `DELETE /api/documentos/{id}` | F3 ✅ |

### `docente/notificaciones.html` — Notificaciones

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el docente | `GET /api/notificaciones` | F1 ✅ |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del docente autenticado desde la API | `GET /api/cuentas/yo` | F1 ✅ |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F1 ✅ |
| `lista-notificaciones` | `ul#lista-notificaciones` | Cargará las notificaciones del docente desde la API | `GET /api/notificaciones` | F3 ✅ |
| `marcar-leida` | `a#marcar-leida-1`, `a#marcar-leida-2` | Marcará esta notificación como leída mediante la API | `POST /api/notificaciones/{id}/leida` | F3 ✅ |
| `preferencias-notificaciones` | `form#form-preferencias` | Guardará la preferencia de correo del docente mediante la API | `GET` y `PUT /api/notificaciones/preferencias` | F3 ✅ |
| `preferencia-correo` | `button#guardar-preferencia` | Enviará la preferencia de correo a la API | `PUT /api/notificaciones/preferencias` | F3 ✅ |

### `docente/mi-cuenta.html` — Mi cuenta

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el docente | `GET /api/notificaciones` | F1 ✅ |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del docente autenticado desde la API | `GET /api/cuentas/yo` | F1 ✅ |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F1 ✅ |
| `datos-cuenta` | `section#datos-cuenta` | Cargará los datos del docente autenticado desde la API | `GET /api/cuentas/yo` | F3 ✅ |
| `materias-imparte` | `ul#materias-imparte` | Consultará en la API las asignaciones del docente | `GET /api/docente/asignaciones` | F3 ✅ |
| `cambiar-contrasena` | `button#cambiar-contrasena` | Abrirá la ventana para cambiar la contraseña | No aplica (solo `:target`) | F3 ✅ |
| `form-cambiar-contrasena` | `form#form-cambiar-contrasena` | Validará las contraseñas y pedirá el cambio a la API | `POST /api/cuentas/cambiar-contrasena` | F3 ✅ |
| `guardar-contrasena` | `button#guardar-contrasena` | Enviará el cambio de contraseña a la API | `POST /api/cuentas/cambiar-contrasena` | F3 ✅ |

## Administrador

### `admin/inicio.html` — Inicio

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el administrador | `GET /api/notificaciones` | F1 ✅ |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del administrador autenticado desde la API | `GET /api/cuentas/yo` | F1 ✅ |
| `selector-vista` | `p#selector-vista` | Solo para un Administrador con asignaciones; el backend ya lo permite (B10 ✅), falta la pantalla (F5) | `GET /api/cuentas/yo` y `GET /api/asignaciones` | F5 (backend B10 ✅) |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F1 ✅ |
| `proximos-eventos` | `ul#proximos-eventos` | Muestra los próximos eventos propios y los feriados desde hoy hasta el fin del año activo | `GET /api/eventos/proximos` | F3 ✅ |

### `admin/usuarios.html` — Usuarios

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el administrador | `GET /api/notificaciones` | F1 ✅ |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del administrador autenticado desde la API | `GET /api/cuentas/yo` | F1 ✅ |
| `selector-vista` | `p#selector-vista` | Solo para un Administrador con asignaciones; el backend ya lo permite (B10 ✅), falta la pantalla (F5) | `GET /api/cuentas/yo` y `GET /api/asignaciones` | F5 (backend B10 ✅) |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F1 ✅ |
| `buscar-usuarios` | `form#buscar-usuarios`, `button#buscar-usuarios-boton` | Filtra en cliente por nombre o correo | `GET /api/cuentas` (filtrar en cliente) | F5 ✅ |
| `tabla-usuarios` | `div#tabla-usuarios` | Carga los usuarios con ids reales en filas y ventanas | `GET /api/cuentas` | F5 ✅ |
| `cambiar-rol` | `a#rol-<id>` de cada fila | Abre `ventana-rol-<id>` con un `select` (Docente/Administrador) | No aplica (solo `:target`) | F5 ✅ |
| `cambiar-estado` | `a#estado-<id>` de cada fila | Abre `ventana-estado-<id>` con «¿Desactivar…?» o «¿Reactivar…?» | No aplica (solo `:target`) | F5 ✅ |
| `confirmar-rol` | `a#confirmar-rol-<id>` | Cambia el rol mediante la API | `PUT /api/cuentas/{id}/rol` | F5 ✅ |
| `confirmar-estado` | `a#confirmar-estado-<id>` | Desactiva o reactiva el usuario mediante la API | `POST /api/cuentas/{id}/desactivar` o `POST /api/cuentas/{id}/reactivar` | F5 ✅ |
| `enviar-codigo-recuperacion` | el correo de cada fila (`a#recuperar-<id>`) | Abre `ventana-recuperar-<id>` para enviar el código | No aplica (solo `:target`) | F5 ✅ |
| `reenviar-activacion` | `a#activar-<id>` de los usuarios inactivos | Abre `ventana-activacion-<id>` para reenviar el enlace | No aplica (solo `:target`) | F5 ✅ |
| `confirmar-codigo-recuperacion` | `a#confirmar-recuperacion-<id>` | Pide a la API que envíe el código de recuperación | `POST /api/cuentas/{id}/restablecer` | F5 ✅ |
| `confirmar-activacion` | `a#confirmar-activacion-<id>` | Pide a la API que reenvíe el enlace de activación | `POST /api/cuentas/reenviar` | F5 ✅ |

### `admin/solicitudes.html` — Solicitudes

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el administrador | `GET /api/notificaciones` | F1 ✅ |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del administrador autenticado desde la API | `GET /api/cuentas/yo` | F1 ✅ |
| `selector-vista` | `p#selector-vista` | Solo para un Administrador con asignaciones; el backend ya lo permite (B10 ✅), falta la pantalla (F5) | `GET /api/cuentas/yo` y `GET /api/asignaciones` | F5 (backend B10 ✅) |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F1 ✅ |
| `filtros-solicitudes` | `form#filtros-solicitudes` | Filtra por fecha local en cliente (desde/hasta inclusivos); vacío = sin límite | `GET /api/permisos/pendientes` (filtrar en cliente) | F5 ✅ |
| `buscar-solicitudes` | `button#solicitudes-buscar` | Ejecuta el filtro en cliente sobre la lista de pendientes | `GET /api/permisos/pendientes` | F5 ✅ |
| `tabla-solicitudes` | `div#tabla-solicitudes` | Muestra una fila por solicitud pendiente; si no hay, «No hay solicitudes pendientes.» | `GET /api/permisos/pendientes` y `GET /api/cuentas` para el nombre del solicitante | F5 ✅ |
| `ver-solicitud` | `a#ver-solicitud-<id>` | Abre `ventana-ver-solicitud-<id>` con el detalle | No aplica (solo `:target`) | F5 ✅ |
| `rechazar-solicitud` | `a#rechazar-<id>` | Abre la ventana compartida `ventana-rechazar`, guardando el id elegido | No aplica (solo `:target`) | F5 ✅ |
| `aprobar-solicitud` | `a#aprobar-<id>` | Abre la ventana compartida `ventana-aprobar`, guardando el id elegido | No aplica (solo `:target`) | F5 ✅ |
| `form-rechazar` | `form#form-rechazar` | Valida el motivo no vacío («Escribe el motivo del rechazo.») y rechaza mediante la API | `POST /api/permisos/{id}/rechazar` | F5 ✅ |
| `confirmar-rechazo` | `button#confirmar-rechazo` | Envía el rechazo con `{ "motivo": "..." }` y muestra siempre el mensaje de la API | `POST /api/permisos/{id}/rechazar` | F5 ✅ |
| `form-aprobar` | `form#form-aprobar` | Aprueba sin cuerpo ni comentario (decisión de la autora, 26 nov) | `POST /api/permisos/{id}/aprobar` | F5 ✅ |
| `confirmar-aprobacion` | `button#confirmar-aprobacion` | Envía la aprobación y muestra siempre el mensaje de la API | `POST /api/permisos/{id}/aprobar` | F5 ✅ |

### `admin/materias.html` — Materias

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el administrador | `GET /api/notificaciones` | F1 ✅ |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del administrador autenticado desde la API | `GET /api/cuentas/yo` | F1 ✅ |
| `selector-vista` | `p#selector-vista` | Solo para un Administrador con asignaciones; el backend ya lo permite (B10 ✅), falta la pantalla (F5) | `GET /api/cuentas/yo` y `GET /api/asignaciones` | F5 (backend B10 ✅) |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F1 ✅ |
| `buscar-materias` | `form#buscar-materias`, `button#buscar-materias-boton` | Filtra las materias por nombre en cliente | `GET /api/materias` (filtrar en cliente) | F5 ✅ |
| `crear-materia` | `a#crear-materia` | Abre la ventana para crear una materia nueva | No aplica (solo `:target`) | F5 ✅ |
| `tabla-materias` | `div#tabla-materias` | Carga Nombre, Técnica, Tipo, RA y Puntos (`34 · 33 · 33`, con « (mín. N)») desde la API | `GET /api/materias` | F5 ✅ |
| `form-crear-materia` | `form#form-crear-materia` | Envía los datos a la API y muestra siempre su `{mensaje}` si falla | `POST /api/materias` | F5 ✅ |
| `materia-tecnica` | `input#materia-tecnica` | Con «Técnica: No» envía `tipoTecnico: null` y `resultadosAprendizaje: []` | No aplica (solo vista) | F5 ✅ |
| `tabla-ra` | `div#tabla-ra` | Genera las filas de RA según la cantidad indicada, clonando el molde | No aplica (solo vista) | F5 ✅ |
| — | `input#ra-N-minimo` (columna Mínimo de los RA) | Captura el mínimo opcional de cada RA (0 a 100); vacío → `null` | `POST /api/materias` (campo `puntajeMinimo` de cada RA) | F5 ✅ (backend B9 ✅) |
| `suma-puntos` | `p#suma-puntos` | Solo informa «Suma: N de 100» al cambiar puntos; no bloquea el envío (decisión de la autora, 26 nov) | No aplica (validación local) | F5 ✅ |
| `aceptar-materia` | `button#aceptar-materia` | Guarda la materia y sus RA; cierra, limpia, recarga y muestra «Materia creada.» | `POST /api/materias` | F5 ✅ |

### `admin/cursos.html` — Cursos

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el administrador | `GET /api/notificaciones` | F1 ✅ |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del administrador autenticado desde la API | `GET /api/cuentas/yo` | F1 ✅ |
| `selector-vista` | `p#selector-vista` | Solo para un Administrador con asignaciones; el backend ya lo permite (B10 ✅), falta la pantalla (F5) | `GET /api/cuentas/yo` y `GET /api/asignaciones` | F5 (backend B10 ✅) |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F1 ✅ |
| `buscar-cursos` | `form#buscar-cursos`, `button#buscar-cursos-boton` | Filtra las tarjetas por curso o sección en cliente | `GET /api/cursos` (filtrar en cliente) | F5 ✅ |
| `crear-curso` | `a#crear-curso` | Abre la ventana para crear un curso | No aplica (solo `:target`) | F5 ✅ |
| `lista-cursos` | `div#lista-cursos` | Carga las tarjetas del año activo (`esActivo` de `GET /api/anios-escolares`) | `GET /api/anios-escolares` y `GET /api/cursos?anioEscolarId={id}` | F5 ✅ |
| `ver-estudiantes-curso` | `a#ver-cursos-<id>` | Navega a `curso-estudiantes.html?cursoId=<id>` | No aplica (navegación a `curso-estudiantes.html`) | F5 ✅ |
| `form-crear-curso` | `form#form-crear-curso` | Envía grado («Ner Grado»), sección, tipo y año activo preseleccionado | `POST /api/cursos` | F5 ✅ |
| `curso-tecnico` | `select#curso-tipo` | Selecciona el tipo técnico entre los dos valores del enum (opción invisible sin «Sí») | No aplica (solo vista) | F5 ✅ |
| `aceptar-curso` | `button#aceptar-curso` | Guarda el curso; cierra, limpia, recarga y muestra «Curso creado.» | `POST /api/cursos` | F5 ✅ |

### `admin/curso-estudiantes.html` — Estudiantes del curso

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el administrador | `GET /api/notificaciones` | F1 ✅ |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del administrador autenticado desde la API | `GET /api/cuentas/yo` | F1 ✅ |
| `selector-vista` | `p#selector-vista` | Solo para un Administrador con asignaciones; el backend ya lo permite (B10 ✅), falta la pantalla (F5) | `GET /api/cuentas/yo` y `GET /api/asignaciones` | F5 (backend B10 ✅) |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F1 ✅ |
| `inscribir-estudiante` | `a#inscribir-estudiante` | Abre la ventana para inscribir un estudiante en el curso | No aplica (solo `:target`) | F5 ✅ |
| `tabla-estudiantes-curso` | `div#tabla-estudiantes-curso` | Carga ##/Nombres/Apellidos (sin columna Cédula); si está vacío, «Este curso no tiene estudiantes.» | `GET /api/cursos` para el título y `GET /api/cursos/{cursoId}/estudiantes` | F5 ✅ |
| `form-inscribir-estudiante` | `form#form-inscribir-estudiante` | Envía nombre, apellido y cédula (11 dígitos con o sin guiones); valida la API | `POST /api/cursos/{cursoId}/estudiantes` | F5 ✅ |
| `aceptar-inscripcion` | `button#aceptar-inscripcion` | Inscribe al estudiante, cierra, recarga y muestra «Estudiante inscrito.» | `POST /api/cursos/{cursoId}/estudiantes` | F5 ✅ |

### `admin/asignaciones.html` — Asignaciones

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el administrador | `GET /api/notificaciones` | F1 ✅ |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del administrador autenticado desde la API | `GET /api/cuentas/yo` | F1 ✅ |
| `selector-vista` | `p#selector-vista` | Solo para un Administrador con asignaciones; el backend ya lo permite (B10 ✅), falta la pantalla (F5) | `GET /api/cuentas/yo` y `GET /api/asignaciones` | F5 (backend B10 ✅) |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F1 ✅ |
| `filtros-asignaciones` | `form#filtros-asignaciones` | Filtra por docente y curso en cliente | `GET /api/asignaciones` (filtrar en cliente) | F5 ✅ |
| `buscar-asignaciones` | `button#asignaciones-buscar` | Ejecuta el filtro en cliente sobre la lista de asignaciones | `GET /api/asignaciones` | F5 ✅ |
| `asignar-docente` | `a#asignar-docente` | Abre la ventana para asignar un docente | No aplica (solo `:target`) | F5 ✅ |
| `tabla-asignaciones` | `div#tabla-asignaciones` | Muestra Docente, Materia, Curso («grado sección»), Año y Horas | `GET /api/asignaciones` | F5 ✅ |
| `form-asignar-docente` | `form#form-asignar-docente` | Llena docentes activos (ambos roles), materias y cursos del año activo | `GET /api/cuentas`, `GET /api/materias`, `GET /api/anios-escolares`, `GET /api/cursos?anioEscolarId={id}` | F5 ✅ |
| `error-asignacion` | `p#error-asignacion` | Muestra siempre el `{mensaje}` de la API, sea cual sea el error | No aplica (respuesta de error de la API) | F5 ✅ |
| `confirmar-asignacion` | `button#confirmar-asignacion` | Envía la asignación; cierra, limpia, recarga y muestra «Asignación creada.» | `POST /api/asignaciones` | F5 ✅ |

### `admin/calendario.html` — Calendario

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el administrador | `GET /api/notificaciones` | F1 ✅ |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del administrador autenticado desde la API | `GET /api/cuentas/yo` | F1 ✅ |
| `selector-vista` | `p#selector-vista` | Solo para un Administrador con asignaciones; el backend ya lo permite (B10 ✅), falta la pantalla (F5) | `GET /api/cuentas/yo` y `GET /api/asignaciones` | F5 (backend B10 ✅) |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F1 ✅ |
| `lista-anios-escolares` | `ul#lista-anios-escolares` | Cargará los años escolares y marcará el activo desde la API | `GET /api/anios-escolares` | F5 |
| `crear-anio-escolar` | `a#crear-anio-escolar` | Abrirá la ventana para crear un año escolar | No aplica (solo `:target`) | F5 |
| `activar-anio-escolar` | `a#activar-anio-escolar` | Abrirá la confirmación para activar el año escolar elegido | No aplica (solo `:target`) | F5 |
| `navegacion-mes` | `p#navegacion-mes` | Cambiará el mes consultado a la API | `GET /api/anios-escolares/{id}/dias-lectivos` | F5 |
| `calendario-mes` | `div#calendario-mes` | Mostrará los eventos propios del mes y los feriados del calendario lectivo | `GET /api/eventos?anioEscolarId={id}` y `GET /api/anios-escolares/{id}/dias-lectivos` | F5 (backend B8 ✅) |
| `borrar-evento` | `a#borrar-feriado-9`, `a#borrar-evento-11` | Solo eventos; los feriados vienen de Calendarific y no se borran (F5 quitará el enlace en los feriados) | No aplica (solo `:target`) | F5 (backend B8 ✅) |
| `crear-evento` | `a#dia-9`, `a#dia-10`, `a#dia-11`, `a#dia-12`, `a#dia-13`, `a#dia-16`, `a#dia-17`, `a#dia-18`, `a#dia-19`, `a#dia-20` | Abrirá la ventana para crear un evento en esta fecha | No aplica (solo `:target`) | F5 (backend B8 ✅) |
| `form-crear-anio` | `form#form-crear-anio` | Validará las fechas y creará el año escolar mediante la API | `POST /api/anios-escolares` | F5 |
| `aceptar-anio` | `button#aceptar-anio` | Guardará el año escolar nuevo mediante la API | `POST /api/anios-escolares` | F5 |
| `confirmar-activar-anio` | `a#confirmar-activar-anio` | Activará el año escolar elegido mediante la API | `POST /api/anios-escolares/{id}/activar` | F5 |
| `form-crear-evento` | `form#form-crear-evento` | Guardará el evento en la fecha elegida mediante la API | `POST /api/eventos` | F5 (backend B8 ✅) |
| `guardar-evento` | `button#guardar-evento` | Guardará el evento nuevo en la fecha elegida | `POST /api/eventos` | F5 (backend B8 ✅) |
| `confirmar-borrar-evento` | `a#confirmar-borrar-evento` | Borrará el evento mediante la API | `DELETE /api/eventos/{id}` | F5 (backend B8 ✅) |

### `admin/correos.html` — Cola de correos

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el administrador | `GET /api/notificaciones` | F1 ✅ |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del administrador autenticado desde la API | `GET /api/cuentas/yo` | F1 ✅ |
| `selector-vista` | `p#selector-vista` | Solo para un Administrador con asignaciones; el backend ya lo permite (B10 ✅), falta la pantalla (F5) | `GET /api/cuentas/yo` y `GET /api/asignaciones` | F5 (backend B10 ✅) |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F1 ✅ |
| `filtros-correos` | `form#filtros-correos` | Consultará la cola de correos en la API con el estado elegido | `GET /api/correos?estado={estado}` | F5 |
| `buscar-correos` | `button#correos-buscar` | Ejecutará la búsqueda de correos en la API | `GET /api/correos?estado={estado}` | F5 |
| `tabla-correos` | `div#tabla-correos` | Cargará el resumen de la cola; la API no devuelve el cuerpo del correo | No aplica (respuesta del `GET`) | F5 |

### `admin/reportes.html` — Reportes

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el administrador | `GET /api/notificaciones` | F1 ✅ |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del administrador autenticado desde la API | `GET /api/cuentas/yo` | F1 ✅ |
| `selector-vista` | `p#selector-vista` | Solo para un Administrador con asignaciones; el backend ya lo permite (B10 ✅), falta la pantalla (F5) | `GET /api/cuentas/yo` y `GET /api/asignaciones` | F5 (backend B10 ✅) |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F1 ✅ |
| `filtros-reporte-solicitudes` | `form#filtros-reporte-solicitudes` | Consultará el reporte de solicitudes a la API | `GET /api/reportes/solicitudes-permiso?desde={desde}&hasta={hasta}` | F5 |
| `consultar-reporte-solicitudes` | `button#consultar-reporte-solicitudes` | Consultará el reporte de solicitudes en la API | `GET /api/reportes/solicitudes-permiso?desde={desde}&hasta={hasta}` | F5 |
| `tabla-reporte-solicitudes` | `div#tabla-reporte-solicitudes` | Mostrará los totales devueltos por la API | No aplica (respuesta del `GET`) | F5 |
| `filtros-reporte-asistencia` | `form#filtros-reporte-asistencia` | Consultará el reporte de asistencia a la API | `GET /api/reportes/asistencia?desde={desde}&hasta={hasta}` | F5 |
| `consultar-reporte-asistencia` | `button#consultar-reporte-asistencia` | Consultará el reporte de asistencia en la API | `GET /api/reportes/asistencia?desde={desde}&hasta={hasta}` | F5 |
| `tabla-reporte-asistencia` | `div#tabla-reporte-asistencia` | Mostrará el detalle de asistencia devuelto por la API | No aplica (respuesta del `GET`) | F5 |

### `admin/auditoria.html` — Historial de auditoría

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el administrador | `GET /api/notificaciones` | F1 ✅ |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del administrador autenticado desde la API | `GET /api/cuentas/yo` | F1 ✅ |
| `selector-vista` | `p#selector-vista` | Solo para un Administrador con asignaciones; el backend ya lo permite (B10 ✅), falta la pantalla (F5) | `GET /api/cuentas/yo` y `GET /api/asignaciones` | F5 (backend B10 ✅) |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F1 ✅ |
| `filtros-auditoria` | `form#filtros-auditoria` | Consultará el historial de auditoría en la API | `GET /api/auditoria?usuarioId={id}&entidad={entidad}&desde={desde}&hasta={hasta}` | F5 |
| `consultar-auditoria` | `button#auditoria-consultar` | Ejecutará la consulta del historial en la API | `GET /api/auditoria` | F5 |
| `tabla-auditoria` | `div#tabla-auditoria` | Mostrará el historial devuelto por la API | No aplica (respuesta del `GET`) | F5 |

### `admin/documentos.html` — Documentos

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el administrador | `GET /api/notificaciones` | F1 ✅ |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del administrador autenticado desde la API | `GET /api/cuentas/yo` | F1 ✅ |
| `selector-vista` | `p#selector-vista` | Solo para un Administrador con asignaciones; el backend ya lo permite (B10 ✅), falta la pantalla (F5) | `GET /api/cuentas/yo` y `GET /api/asignaciones` | F5 (backend B10 ✅) |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F1 ✅ |
| `subir-documentos` | `a#subir-documentos` | Abrirá la ventana para seleccionar y subir archivos | No aplica (solo `:target`) | F3 ✅ |
| `lista-documentos` | `div#lista-documentos` | Cargará los documentos del administrador desde la API | `GET /api/documentos` | F3 ✅ |
| `descargar-documento` | `a#descargar-1`, `a#descargar-2` | Descargará el archivo desde la API | `GET /api/documentos/{id}/descargar` | F3 ✅ |
| `eliminar-documento` | `a#eliminar-1`, `a#eliminar-2` | Abrirá la ventana de confirmación antes de eliminar | No aplica (solo `:target`) | F3 ✅ |
| `mensaje-sin-documentos` | `p#mensaje-sin-documentos` | Mostrará este aviso cuando la API devuelva una lista vacía | No aplica (respuesta del `GET`) | F3 ✅ |
| `form-subir-documentos` | `form#form-subir-documentos` | Validará los archivos elegidos y los enviará a la API | `POST /api/documentos` | F3 ✅ |
| `seleccionar-archivos` | `input#archivos-documento` | Tomará la lista de archivos del input y mostrará sus nombres | No aplica (lectura local) | F3 ✅ |
| `archivos-elegidos` | `ul#archivos-elegidos` | Mostrará los nombres de los archivos seleccionados en el input | No aplica (solo vista) | F3 ✅ |
| `aceptar-documentos` | `button#aceptar-documentos` | Subirá los archivos seleccionados a la API | `POST /api/documentos` | F3 ✅ |
| `confirmar-eliminar-documento` | `a#confirmar-eliminar-1`, `a#confirmar-eliminar-2` | Eliminará el documento mediante la API | `DELETE /api/documentos/{id}` | F3 ✅ |

### `admin/notificaciones.html` — Notificaciones

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el administrador | `GET /api/notificaciones` | F1 ✅ |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del administrador autenticado desde la API | `GET /api/cuentas/yo` | F1 ✅ |
| `selector-vista` | `p#selector-vista` | Solo para un Administrador con asignaciones; el backend ya lo permite (B10 ✅), falta la pantalla (F5) | `GET /api/cuentas/yo` y `GET /api/asignaciones` | F5 (backend B10 ✅) |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F1 ✅ |
| `lista-notificaciones` | `ul#lista-notificaciones` | Cargará las notificaciones del administrador desde la API | `GET /api/notificaciones` | F3 ✅ |
| `marcar-leida` | `a#marcar-leida-1`, `a#marcar-leida-2` | Marcará esta notificación como leída mediante la API | `POST /api/notificaciones/{id}/leida` | F3 ✅ |
| `preferencias-notificaciones` | `form#form-preferencias` | Guardará la preferencia de correo del administrador mediante la API | `GET` y `PUT /api/notificaciones/preferencias` | F3 ✅ |
| `preferencia-correo` | `button#guardar-preferencia` | Enviará la preferencia de correo a la API | `PUT /api/notificaciones/preferencias` | F3 ✅ |

### `admin/mi-cuenta.html` — Mi cuenta

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el administrador | `GET /api/notificaciones` | F1 ✅ |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del administrador autenticado desde la API | `GET /api/cuentas/yo` | F1 ✅ |
| `selector-vista` | `p#selector-vista` | Solo para un Administrador con asignaciones; el backend ya lo permite (B10 ✅), falta la pantalla (F5) | `GET /api/cuentas/yo` y `GET /api/asignaciones` | F5 (backend B10 ✅) |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F1 ✅ |
| `datos-cuenta` | `section#datos-cuenta` | Cargará los datos del administrador autenticado desde la API | `GET /api/cuentas/yo` | F3 ✅ |
| `cambiar-contrasena` | `button#cambiar-contrasena` | Abrirá la ventana para cambiar la contraseña | No aplica (solo `:target`) | F3 ✅ |
| `form-cambiar-contrasena` | `form#form-cambiar-contrasena` | Validará las contraseñas y pedirá el cambio a la API | `POST /api/cuentas/cambiar-contrasena` | F3 ✅ |
| `guardar-contrasena` | `button#guardar-contrasena` | Enviará el cambio de contraseña a la API | `POST /api/cuentas/cambiar-contrasena` | F3 ✅ |

## Por confirmar en el código

1. **Eventos del calendario (B8 ✅).** Ya hay controlador y rutas reales: `GET /api/eventos?anioEscolarId={id}`, `GET /api/eventos/proximos`, `POST /api/eventos` y `DELETE /api/eventos/{id}`. Los eventos propios se guardan en el módulo de negocio; los feriados vienen de Calendarific y no se guardan ni se borran. Las filas de inicio y de calendario ya no están pendientes de confirmación: falta conectarlas en F3/F5.
2. **Validación suelta del código de recuperación.** Decidido (F2): no se añade endpoint; el código se guarda en sessionStorage y se valida en el paso 3 con /recuperacion/confirmar
3. **Filtros de listas.** Decidido (F4b2): se filtra en cliente. `calificaciones.html` toma Curso, Sección y Materia de `GET /api/docente/asignaciones` y consulta solo la asignación elegida con `GET /api/docente/asignaciones/{id}/calificaciones`; no se añadieron parámetros nuevos a la API.
4. **Borradores de calificación (B13 y F4b2 ✅).** El botón «Borrador» registra la nota con `POST /api/calificaciones` y, si ya existe en `Borrador`, la cambia con `PATCH /api/calificaciones/{id}/borrador`: no necesita el permiso `corregir-calificacion` y no cambia el estado. Ya no está pendiente de confirmación.
5. **Selector de vista (B10/F5).** El backend ya lo permite: un Administrador activo puede recibir asignaciones y usar los endpoints del docente solo sobre las suyas. Falta la pantalla (F5): el selector no es una llamada a la API por sí mismo, depende del rol y de las asignaciones del administrador; se documentó con `GET /api/cuentas/yo` y `GET /api/asignaciones`.
6. **Reparto por bloques.** F1 = comunes de todas las páginas: `contador-notificaciones`, `usuario-actual`, `cerrar-sesion` (y la base: sesión, llamadas a la API). F2 = públicas: `index`, `registro`, `recuperar-correo`, `recuperar-codigo`, `recuperar-nueva`. F3 = inicio (docente y administrador), `mi-cuenta`, `documentos` y `notificaciones` de los dos roles. F4 = resto de páginas del docente (`materias`, `cursos`, `curso-estudiantes`, `asistencia`, `asistencia-lista`, `calificar`, `calificaciones`, `solicitudes`). F5a = `usuarios`, `solicitudes`, `materias`, `cursos`, `curso-estudiantes` y `asignaciones` del administrador: las seis pantallas están conectadas ✅ con `js/admin-*.js`. F5b = `calendario`, `correos`, `reportes` y `auditoria` del administrador: siguen pendientes. Estas filas llevan un bloque B: `aviso-edicion` → F4b ✅ (pantalla conectada en F4b1); `borrador`, `enviar`, `publicar`, `recuperacion`, `cerrar`, `corregir` y `recuperacion-nota` → F4b2 ✅ (pantallas `calificar.html` y `calificaciones.html` conectadas); en el calendario, `crear-evento`, `form-crear-evento`, `guardar-evento`, `borrar-evento`, `confirmar-borrar-evento` y `calendario-mes` → F5b (el backend ya lo permite desde B8); en el inicio de los dos roles, `proximos-eventos` → F3 ✅; la columna Mínimo de los RA → F5a ✅ (conectada en `materias.html`; el backend ya lo acepta desde B9); `selector-vista` → F5 (el backend ya lo permite desde B10). Todo lo demás (años escolares, eventos, cola de correos, reportes y auditoría…) va en F5b.
7. **Decisiones del frontend del Administrador (F5a, 26 nov).** Aprobación de solicitudes sin comentario; sin columna Cédula en la lista de estudiantes del curso; reenvío de activación solo para usuarios Inactivos; `materia-tipo` y `curso-tipo` son `select` con los dos valores del enum y texto legible; grados «1er Grado» … «6to Grado»; `suma-puntos` solo informa «Suma: N de 100» y no bloquea el envío; búsquedas y filtros en cliente, sin parámetros nuevos en la API; la pantalla siempre muestra el `{mensaje}` de la API, sin reglas de negocio en JavaScript; el año escolar activo se toma de `GET /api/anios-escolares` (`esActivo`), no de un endpoint de docente.
