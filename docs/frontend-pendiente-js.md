# Lo pendiente de JavaScript en la maqueta

La maqueta de `src/Tams.Api/wwwroot` es **solo HTML y CSS**: no hay archivos `.js`, etiquetas `<script>` ni atributos `on*`. Las ventanas se abren con `:target`, los avisos usan `hidden` y los botones no envían nada. Cada elemento marcado con `data-js` es, por tanto, un punto donde el frontend definitivo tendrá que conectar comportamiento. En las tablas siguientes, la columna «qué debe hacer el JavaScript» reproduce los comentarios `<!-- JS: ... -->` de cada página; el endpoint se tomó de `src/Tams.Api/Controladores`. Cuando no existe o no está claro, se escribe `POR CONFIRMAR (F0)`.

## Públicas

### `index.html` — Iniciar sesión

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `login` | `form #form-login` | Validará correo y contraseña, llamará a la API de sesión y guardará el token para navegar al panel | `POST /api/cuentas/login` | F1 |
| `reenviar-activacion` | `a.enlace-publico` | Pedirá a la API que envíe de nuevo el correo de activación al usuario | `POST /api/cuentas/reenviar` | F1 |

### `registro.html` — Registrarse

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `registro` | `form.formulario-publico` | Validará los campos, comprobará que las contraseñas coincidan y enviará el alta a la API | `POST /api/cuentas/registro` | F1 |

### `recuperar-correo.html` — Recuperar contraseña: correo

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `recuperar-correo` | `form.formulario-publico` | Validará el correo y pedirá a la API que envíe el código de recuperación | `POST /api/cuentas/recuperacion/iniciar` | F1 |

### `recuperar-codigo.html` — Recuperar contraseña: código

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `recuperar-codigo` | `form.formulario-publico` | Validará el código y lo comprobará con la API de recuperación | `POR CONFIRMAR (F0)` — no hay endpoint para validar el código suelto; `POST /api/cuentas/recuperacion/confirmar` también cambia la contraseña | F1 |

### `recuperar-nueva.html` — Recuperar contraseña: nueva contraseña

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `recuperar-nueva` | `form.formulario-publico` | Comprobará que ambas contraseñas coincidan y enviará el cambio a la API | `POST /api/cuentas/recuperacion/confirmar` | F1 |

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
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el docente | `GET /api/notificaciones` | F2 |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del docente autenticado desde la API | `GET /api/cuentas/yo` | F2 |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F2 |
| `recientes` | `div#recientes` | Las abiertas recientemente se guardan en el navegador; si no hay, se muestran todas las asignaciones | `GET /api/docente/asignaciones` como respaldo | F2 |

### `docente/materias.html` — Materias

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el docente | `GET /api/notificaciones` | F2 |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del docente autenticado desde la API | `GET /api/cuentas/yo` | F2 |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F2 |
| `lista-asignaciones` | `div#lista-asignaciones` | Cargará las asignaciones del docente desde la API | `GET /api/docente/asignaciones` | F2 |
| `ver-asignacion` | `a#ver-asignacion-matematica`, `a#ver-asignacion-programacion` | Abrirá el desglose de la asignación con `:target` | No aplica (solo `:target`) | F2 |
| `pasar-lista` | `a#pasar-lista-matematica`, `a#pasar-lista-programacion` | Abrirá la lista de asistencia de esta asignación | No aplica (navegación a `asistencia-lista.html`) | F2 |
| `calificar` | `a#calificar-matematica`, `a#calificar-programacion` | Abrirá la pantalla de calificación de esta asignación | No aplica (navegación a `calificar.html`) | F2 |

### `docente/cursos.html` — Cursos

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el docente | `GET /api/notificaciones` | F2 |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del docente autenticado desde la API | `GET /api/cuentas/yo` | F2 |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F2 |
| `lista-cursos` | `div#lista-cursos` | Cargará los cursos y secciones asignados al docente desde la API | `GET /api/docente/asignaciones` | F2 |
| `ver-curso` | `a#ver-curso-a`, `a#ver-curso-b` | Abrirá el detalle de materias del curso con `:target` | No aplica (solo `:target`) | F2 |
| `ver-estudiantes` | `a#ver-estudiantes-a`, `a#ver-estudiantes-b` | Consultará los estudiantes de este curso en la API | `GET /api/docente/cursos/{cursoId}/estudiantes` | F2 |

### `docente/curso-estudiantes.html` — Estudiantes del curso

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el docente | `GET /api/notificaciones` | F2 |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del docente autenticado desde la API | `GET /api/cuentas/yo` | F2 |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F2 |
| `lista-estudiantes` | `div#lista-estudiantes` | Cargará de la API los estudiantes matriculados en el curso y sección elegidos | `GET /api/docente/cursos/{cursoId}/estudiantes` | F2 |

### `docente/asistencia.html` — Asistencia

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el docente | `GET /api/notificaciones` | F2 |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del docente autenticado desde la API | `GET /api/cuentas/yo` | F2 |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F2 |
| `lista-asistencia` | `div#lista-asistencia` | Cargará los cursos con asignaciones de asistencia desde la API | `GET /api/docente/asignaciones` | F2 |
| `ver-materias-asistencia` | `a#ver-materias-asistencia-a`, `a#ver-materias-asistencia-b` | Abrirá las materias del curso para elegir cuál pasar lista | No aplica (solo `:target`) | F2 |

### `docente/asistencia-lista.html` — Lista de asistencia

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el docente | `GET /api/notificaciones` | B7 |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del docente autenticado desde la API | `GET /api/cuentas/yo` | B7 |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | B7 |
| `asistencia-buscar` | `button#asistencia-buscar` | Pedirá a la API la asistencia del curso y la fecha elegida | `GET /api/docente/asistencias?asignacionId={id}&fecha={fecha}` | B7 |
| `aviso-edicion` | `p#aviso-edicion` | Llega con el bloque B7 | `PUT /api/docente/asistencias` | B7 |
| `resumen-asistencia` | `p#resumen-asistencia` | Calculará y actualizará los contadores con lo que devuelva la API | `GET /api/docente/asistencias?asignacionId={id}&fecha={fecha}` | B7 |
| `asistencia-estados` | `div#tabla-asistencia` | Cargará los estudiantes y enviará los estados y observaciones a la API | `GET` y `PUT /api/docente/asistencias` | B7 |
| `asistencia-editar` | `button#asistencia-editar` | Habilitará la edición de la asistencia ya registrada mediante la API | `PUT /api/docente/asistencias` | B7 |
| `asistencia-enviar` | `button#asistencia-enviar` | Enviará la asistencia a la API y mostrará el mensaje de éxito | `PUT /api/docente/asistencias` | B7 |
| `mensaje-asistencia-enviada` | `p#mensaje-asistencia-enviada` | Mostrará este mensaje cuando la API confirme el envío de la asistencia | No aplica (respuesta del `PUT`) | B7 |

### `docente/calificaciones.html` — Consultar calificaciones

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el docente | `GET /api/notificaciones` | F2 |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del docente autenticado desde la API | `GET /api/cuentas/yo` | F2 |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F2 |
| `filtros-calificaciones` | `form#filtros-calificaciones` | Consultará en la API las calificaciones según los filtros elegidos | `GET /api/calificaciones/mias` (filtrar en cliente: curso, sección y materia) | F2 |
| `consultar-calificaciones` | `button#consultar-calificaciones` | Ejecutará la consulta a la API con curso, sección y materia | `GET /api/calificaciones/mias` | F2 |

### `docente/calificar.html` — Calificar

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el docente | `GET /api/notificaciones` | F2 |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del docente autenticado desde la API | `GET /api/cuentas/yo` | F2 |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F2 |
| `tabla-calificar` | `div#tabla-calificar` | Cargará las notas de los estudiantes desde la API | `GET /api/calificaciones/mias` | F2 |
| `calificar-estudiante` | `a#calificar-ana`, `a#calificar-luis` | Abrirá la ventana de calificación del estudiante con sus notas de la API | `GET /api/calificaciones/mias` | F2 |
| `enviar-todas` | `button#enviar-todas` | Abrirá la ventana de confirmación para enviar todas las calificaciones | No aplica (solo `:target`) | F2 |
| `publicar-todas` | `button#publicar-todas` | Abrirá la ventana de confirmación para publicar todas las calificaciones | No aplica (solo `:target`) | F2 |
| `form-calificar` | `form#form-calificar-ana`, `form#form-calificar-luis` | Guardará la nota del periodo elegido mediante la API | `POST /api/calificaciones` | F2 |
| `borrador` | `button#borrador-ana`, `button#borrador-luis` | Guardará la nota como borrador sin publicarla | `POST /api/calificaciones` | F2 |
| `enviar` | `button#enviar-ana`, `button#enviar-luis` | Enviará la nota a revisión mediante la API | `POST /api/calificaciones/{id}/enviar` | F2 |
| `publicar` | `button#publicar-ana`, `button#publicar-luis` | Publicará la nota solo cuando esté aprobada | `POST /api/calificaciones/{id}/publicar` | F2 |
| `confirmar-envio-todas` | `a#confirmar-envio-todas` | Enviará todas las calificaciones a la API | `POST /api/calificaciones/{id}/enviar` (uno por nota) | F2 |
| `confirmar-publicacion-todas` | `a#confirmar-publicacion-todas` | Publicará todas las calificaciones aprobadas mediante la API | `POST /api/calificaciones/{id}/publicar` (uno por nota) | F2 |

### `docente/solicitudes.html` — Solicitudes

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el docente | `GET /api/notificaciones` | F2 |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del docente autenticado desde la API | `GET /api/cuentas/yo` | F2 |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F2 |
| `nueva-solicitud` | `button#nueva-solicitud` | Abrirá la ventana para crear una solicitud nueva | No aplica (solo `:target`) | F2 |
| `lista-solicitudes` | `div#lista-solicitudes` | Cargará las solicitudes del docente desde la API | `GET /api/permisos` | F2 |
| `ver-solicitud` | `a#ver-solicitud-1`, `a#ver-solicitud-2` | Abrirá el detalle de la solicitud con `:target` | No aplica (solo `:target`) | F2 |
| `crear-solicitud` | `form#form-nueva-solicitud` | Validará el tipo y el motivo y creará la solicitud mediante la API | `POST /api/permisos` | F2 |
| `enviar-solicitud` | `button#enviar-solicitud` | Enviará la solicitud nueva a la API | `POST /api/permisos` | F2 |

### `docente/documentos.html` — Documentos

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el docente | `GET /api/notificaciones` | F2 |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del docente autenticado desde la API | `GET /api/cuentas/yo` | F2 |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F2 |
| `subir-documentos` | `button#subir-documentos` | Abrirá la ventana para seleccionar y subir archivos | No aplica (solo `:target`) | F2 |
| `lista-documentos` | `div#lista-documentos` | Cargará los documentos del docente desde la API | `GET /api/documentos` | F2 |
| `descargar-documento` | `a#descargar-1`, `a#descargar-2`, `a#descargar-3` | Descargará el archivo desde la API | `GET /api/documentos/{id}/descargar` | F2 |
| `eliminar-documento` | `a#eliminar-1`, `a#eliminar-2`, `a#eliminar-3` | Abrirá la ventana de confirmación antes de eliminar | No aplica (solo `:target`) | F2 |
| `mensaje-sin-documentos` | `p#mensaje-sin-documentos` | Mostrará este aviso cuando la API devuelva una lista vacía | No aplica (respuesta del `GET`) | F2 |
| `form-subir-documentos` | `form#form-subir-documentos` | Validará los archivos elegidos y los enviará a la API | `POST /api/documentos` | F2 |
| `seleccionar-archivos` | `input#archivos-documento` | Tomará la lista de archivos del input y mostrará sus nombres | No aplica (lectura local) | F2 |
| `archivos-elegidos` | `ul#archivos-elegidos` | Mostrará los nombres de los archivos seleccionados en el input | No aplica (solo vista) | F2 |
| `aceptar-documentos` | `button#aceptar-documentos` | Subirá los archivos seleccionados a la API | `POST /api/documentos` | F2 |
| `confirmar-eliminar-documento` | `a#confirmar-eliminar-1`, `a#confirmar-eliminar-2`, `a#confirmar-eliminar-3` | Eliminará el documento mediante la API | `DELETE /api/documentos/{id}` | F2 |

### `docente/notificaciones.html` — Notificaciones

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el docente | `GET /api/notificaciones` | F4 |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del docente autenticado desde la API | `GET /api/cuentas/yo` | F4 |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F4 |
| `lista-notificaciones` | `ul#lista-notificaciones` | Cargará las notificaciones del docente desde la API | `GET /api/notificaciones` | F4 |
| `marcar-leida` | `a#marcar-leida-1`, `a#marcar-leida-2` | Marcará esta notificación como leída mediante la API | `POST /api/notificaciones/{id}/leida` | F4 |
| `preferencias-notificaciones` | `form#form-preferencias` | Guardará la preferencia de correo del docente mediante la API | `GET` y `PUT /api/notificaciones/preferencias` | F4 |
| `preferencia-correo` | `button#guardar-preferencia` | Enviará la preferencia de correo a la API | `PUT /api/notificaciones/preferencias` | F4 |

### `docente/mi-cuenta.html` — Mi cuenta

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el docente | `GET /api/notificaciones` | F1 |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del docente autenticado desde la API | `GET /api/cuentas/yo` | F1 |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F1 |
| `datos-cuenta` | `section#datos-cuenta` | Cargará los datos del docente autenticado desde la API | `GET /api/cuentas/yo` | F1 |
| `materias-imparte` | `ul#materias-imparte` | Consultará en la API las asignaciones del docente | `GET /api/docente/asignaciones` | F1 |
| `cambiar-contrasena` | `button#cambiar-contrasena` | Abrirá la ventana para cambiar la contraseña | No aplica (solo `:target`) | F1 |
| `form-cambiar-contrasena` | `form#form-cambiar-contrasena` | Validará las contraseñas y pedirá el cambio a la API | `POST /api/cuentas/cambiar-contrasena` | F1 |
| `guardar-contrasena` | `button#guardar-contrasena` | Enviará el cambio de contraseña a la API | `POST /api/cuentas/cambiar-contrasena` | F1 |

## Administrador

### `admin/inicio.html` — Inicio

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el administrador | `GET /api/notificaciones` | F3 |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del administrador autenticado desde la API | `GET /api/cuentas/yo` | F3 |
| `selector-vista` | `p#selector-vista` | Solo para un Administrador con asignaciones; llega con el bloque B10 | `GET /api/cuentas/yo` y `GET /api/asignaciones` | B10 |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F3 |
| `proximos-eventos` | `ul#proximos-eventos` | Feriados y días lectivos de la API; los eventos propios llegan con el bloque B8 | `GET /api/anios-escolares/{id}/dias-lectivos`; `POR CONFIRMAR (F0)` para los eventos propios | B8 |

### `admin/usuarios.html` — Usuarios

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el administrador | `GET /api/notificaciones` | F3 |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del administrador autenticado desde la API | `GET /api/cuentas/yo` | F3 |
| `selector-vista` | `p#selector-vista` | Solo para un Administrador con asignaciones; llega con el bloque B10 | `GET /api/cuentas/yo` y `GET /api/asignaciones` | B10 |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F3 |
| `buscar-usuarios` | `form#buscar-usuarios`, `button#buscar-usuarios-boton` | Buscará usuarios en la API por nombre o correo | `GET /api/cuentas` (filtrar en cliente) | F3 |
| `tabla-usuarios` | `div#tabla-usuarios` | Cargará los usuarios y sus roles/estados desde la API | `GET /api/cuentas` | F3 |
| `cambiar-rol` | `button#rol-u001`, `button#rol-u002`, `button#rol-u003` | Abrirá la ventana para cambiar el rol del usuario | No aplica (solo `:target`) | F3 |
| `cambiar-estado` | `button#estado-u001`, `button#estado-u002`, `button#estado-u003` | Abrirá la ventana para desactivar o reactivar el usuario | No aplica (solo `:target`) | F3 |
| `ver-usuario` | `button#ver-u001`, `button#ver-u002`, `button#ver-u003` | Abrirá el detalle del usuario | No aplica (solo `:target`) | F3 |
| `confirmar-rol` | `a#confirmar-rol-u001`, `a#confirmar-rol-u002`, `a#confirmar-rol-u003` | Cambiará el rol mediante la API | `PUT /api/cuentas/{id}/rol` | F3 |
| `confirmar-estado` | `a#confirmar-estado-u001`, `a#confirmar-estado-u002`, `a#confirmar-estado-u003` | Desactivará o reactivará el usuario mediante la API | `POST /api/cuentas/{id}/desactivar` o `POST /api/cuentas/{id}/reactivar` | F3 |
| `enviar-codigo-recuperacion` | `a#recuperar-u001`, `a#recuperar-u003` | Abrirá la confirmación para enviar el código de recuperación | No aplica (solo `:target`) | F3 |
| `reenviar-activacion` | `a#activar-u002` | Abrirá la confirmación para reenviar la activación | No aplica (solo `:target`) | F3 |
| `confirmar-codigo-recuperacion` | `a#confirmar-recuperacion` | Pedirá a la API que envíe el código de recuperación | `POST /api/cuentas/{id}/restablecer` | F3 |
| `confirmar-activacion` | `a#confirmar-activacion` | Pedirá a la API que reenvíe el enlace de activación | `POST /api/cuentas/reenviar` | F3 |

### `admin/solicitudes.html` — Solicitudes

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el administrador | `GET /api/notificaciones` | F3 |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del administrador autenticado desde la API | `GET /api/cuentas/yo` | F3 |
| `selector-vista` | `p#selector-vista` | Solo para un Administrador con asignaciones; llega con el bloque B10 | `GET /api/cuentas/yo` y `GET /api/asignaciones` | B10 |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F3 |
| `filtros-solicitudes` | `form#filtros-solicitudes` | Consultará las solicitudes en la API con los filtros elegidos | `GET /api/permisos/pendientes` (filtrar en cliente) | F3 |
| `buscar-solicitudes` | `button#solicitudes-buscar` | Ejecutará la búsqueda de solicitudes en la API | `GET /api/permisos/pendientes` | F3 |
| `tabla-solicitudes` | `div#tabla-solicitudes` | Mostrará las solicitudes devueltas por la API | No aplica (respuesta del `GET`) | F3 |
| `ver-solicitud` | `a#ver-solicitud-1`, `a#ver-solicitud-2` | Abrirá el detalle de la solicitud | No aplica (solo `:target`) | F3 |
| `rechazar-solicitud` | `a#rechazar-1`, `a#rechazar-2` | Abrirá la ventana para rechazar con motivo obligatorio | No aplica (solo `:target`) | F3 |
| `aprobar-solicitud` | `a#aprobar-1`, `a#aprobar-2` | Abrirá la ventana de confirmación para aprobar | No aplica (solo `:target`) | F3 |
| `form-rechazar` | `form#form-rechazar` | Validará el motivo y rechazará la solicitud mediante la API | `POST /api/permisos/{id}/rechazar` | F3 |
| `confirmar-rechazo` | `button#confirmar-rechazo` | Enviará el rechazo y su motivo obligatorio a la API | `POST /api/permisos/{id}/rechazar` | F3 |
| `form-aprobar` | `form#form-aprobar` | Aprobará la solicitud en la API con el comentario opcional | `POST /api/permisos/{id}/aprobar` | F3 |
| `confirmar-aprobacion` | `button#confirmar-aprobacion` | Enviará la aprobación y el comentario opcional a la API | `POST /api/permisos/{id}/aprobar` | F3 |

### `admin/materias.html` — Materias

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el administrador | `GET /api/notificaciones` | F3 |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del administrador autenticado desde la API | `GET /api/cuentas/yo` | F3 |
| `selector-vista` | `p#selector-vista` | Solo para un Administrador con asignaciones; llega con el bloque B10 | `GET /api/cuentas/yo` y `GET /api/asignaciones` | B10 |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F3 |
| `buscar-materias` | `form#buscar-materias`, `button#buscar-materias-boton` | Buscará materias en la API por nombre | `GET /api/materias` (filtrar en cliente) | F3 |
| `crear-materia` | `a#crear-materia` | Abrirá la ventana para crear una materia nueva | No aplica (solo `:target`) | F3 |
| `tabla-materias` | `div#tabla-materias` | Cargará las materias y sus resultados de aprendizaje desde la API | `GET /api/materias` | F3 |
| `form-crear-materia` | `form#form-crear-materia` | Validará los datos y creará la materia mediante la API | `POST /api/materias` | B9 |
| `materia-tecnica` | `input#materia-tecnica` | Permitirá capturar los RA solo cuando la materia sea técnica | No aplica (solo vista) | B9 |
| `tabla-ra` | `div#tabla-ra` | Generará las filas de RA según la cantidad indicada | No aplica (solo vista) | B9 |
| `suma-puntos` | `p#suma-puntos` | Recalculará la suma y validará que sea exactamente 100 | No aplica (validación local) | B9 |
| `aceptar-materia` | `button#aceptar-materia` | Guardará la materia y sus RA mediante la API | `POST /api/materias` | B9 |

### `admin/cursos.html` — Cursos

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el administrador | `GET /api/notificaciones` | F3 |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del administrador autenticado desde la API | `GET /api/cuentas/yo` | F3 |
| `selector-vista` | `p#selector-vista` | Solo para un Administrador con asignaciones; llega con el bloque B10 | `GET /api/cuentas/yo` y `GET /api/asignaciones` | B10 |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F3 |
| `buscar-cursos` | `form#buscar-cursos`, `button#buscar-cursos-boton` | Buscará cursos en la API por curso o sección | `GET /api/cursos` (filtrar en cliente) | F3 |
| `crear-curso` | `a#crear-curso` | Abrirá la ventana para crear un curso | No aplica (solo `:target`) | F3 |
| `lista-cursos` | `div#lista-cursos` | Cargará los cursos y secciones del año activo desde la API | `GET /api/cursos`, `GET /api/docente/anio-escolar-activo` | F3 |
| `ver-estudiantes-curso` | `a#ver-cursos-a`, `a#ver-cursos-b` | Abrirá la lista de estudiantes del curso | No aplica (navegación a `curso-estudiantes.html`) | F3 |
| `form-crear-curso` | `form#form-crear-curso` | Validará los datos y creará el curso mediante la API | `POST /api/cursos` | F3 |
| `curso-tecnico` | `input#curso-tecnico` | Mostrará u ocultará el tipo de técnico según la casilla | No aplica (solo vista) | F3 |
| `aceptar-curso` | `button#aceptar-curso` | Guardará el curso nuevo mediante la API | `POST /api/cursos` | F3 |

### `admin/curso-estudiantes.html` — Estudiantes del curso

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el administrador | `GET /api/notificaciones` | F3 |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del administrador autenticado desde la API | `GET /api/cuentas/yo` | F3 |
| `selector-vista` | `p#selector-vista` | Solo para un Administrador con asignaciones; llega con el bloque B10 | `GET /api/cuentas/yo` y `GET /api/asignaciones` | B10 |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F3 |
| `inscribir-estudiante` | `a#inscribir-estudiante` | Abrirá la ventana para inscribir un estudiante en el curso | No aplica (solo `:target`) | F3 |
| `tabla-estudiantes-curso` | `div#tabla-estudiantes-curso` | Cargará los estudiantes matriculados en el curso desde la API | `GET /api/cursos/{cursoId}/estudiantes` | F3 |
| `form-inscribir-estudiante` | `form#form-inscribir-estudiante` | Validará los 11 dígitos y inscribirá al estudiante mediante la API | `POST /api/cursos/{cursoId}/estudiantes` | F3 |
| `aceptar-inscripcion` | `button#aceptar-inscripcion` | Inscribirá al estudiante en el curso mediante la API | `POST /api/cursos/{cursoId}/estudiantes` | F3 |

### `admin/asignaciones.html` — Asignaciones

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el administrador | `GET /api/notificaciones` | F3 |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del administrador autenticado desde la API | `GET /api/cuentas/yo` | F3 |
| `selector-vista` | `p#selector-vista` | Solo para un Administrador con asignaciones; llega con el bloque B10 | `GET /api/cuentas/yo` y `GET /api/asignaciones` | B10 |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F3 |
| `filtros-asignaciones` | `form#filtros-asignaciones` | Consultará las asignaciones en la API con los filtros elegidos | `GET /api/asignaciones` (filtrar en cliente) | F3 |
| `buscar-asignaciones` | `button#asignaciones-buscar` | Ejecutará la búsqueda de asignaciones en la API | `GET /api/asignaciones` | F3 |
| `asignar-docente` | `a#asignar-docente` | Abrirá la ventana para asignar un docente | No aplica (solo `:target`) | F3 |
| `tabla-asignaciones` | `div#tabla-asignaciones` | Mostrará las asignaciones devueltas por la API | No aplica (respuesta del `GET`) | F3 |
| `form-asignar-docente` | `form#form-asignar-docente` | Validará la asignación y la creará mediante la API | `POST /api/asignaciones` | B10 |
| `error-asignacion` | `p#error-asignacion` | Mostrará este mensaje si la API rechaza la combinación de materia técnica y curso básico | No aplica (respuesta de error de la API) | B10 |
| `confirmar-asignacion` | `button#confirmar-asignacion` | Enviará la asignación nueva a la API | `POST /api/asignaciones` | B10 |

### `admin/calendario.html` — Calendario

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el administrador | `GET /api/notificaciones` | B8 |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del administrador autenticado desde la API | `GET /api/cuentas/yo` | B8 |
| `selector-vista` | `p#selector-vista` | Solo para un Administrador con asignaciones; llega con el bloque B10 | `GET /api/cuentas/yo` y `GET /api/asignaciones` | B10 |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | B8 |
| `lista-anios-escolares` | `ul#lista-anios-escolares` | Cargará los años escolares y marcará el activo desde la API | `GET /api/anios-escolares` | B8 |
| `crear-anio-escolar` | `a#crear-anio-escolar` | Abrirá la ventana para crear un año escolar | No aplica (solo `:target`) | B8 |
| `activar-anio-escolar` | `a#activar-anio-escolar` | Abrirá la confirmación para activar el año escolar elegido | No aplica (solo `:target`) | B8 |
| `navegacion-mes` | `p#navegacion-mes` | Cambiará el mes consultado a la API | `GET /api/anios-escolares/{id}/dias-lectivos` | B8 |
| `calendario-mes` | `div#calendario-mes` | Crear y borrar eventos llegan con el bloque B8 | `POR CONFIRMAR (F0)` — no hay controlador de eventos | B8 |
| `borrar-evento` | `a#borrar-feriado-9`, `a#borrar-evento-11` | Abrirá la ventana para borrar este feriado o evento | No aplica (solo `:target`) | B8 |
| `crear-evento` | `a#dia-9`, `a#dia-10`, `a#dia-11`, `a#dia-12`, `a#dia-13`, `a#dia-16`, `a#dia-17`, `a#dia-18`, `a#dia-19`, `a#dia-20` | Abrirá la ventana para crear un evento en esta fecha | No aplica (solo `:target`) | B8 |
| `form-crear-anio` | `form#form-crear-anio` | Validará las fechas y creará el año escolar mediante la API | `POST /api/anios-escolares` | B8 |
| `aceptar-anio` | `button#aceptar-anio` | Guardará el año escolar nuevo mediante la API | `POST /api/anios-escolares` | B8 |
| `confirmar-activar-anio` | `a#confirmar-activar-anio` | Activarará el año escolar mediante la API | `POST /api/anios-escolares/{id}/activar` | B8 |
| `form-crear-evento` | `form#form-crear-evento` | Guardará el evento en la fecha elegida mediante la API | `POR CONFIRMAR (F0)` | B8 |
| `guardar-evento` | `button#guardar-evento` | Guardará el evento nuevo en la fecha elegida | `POR CONFIRMAR (F0)` | B8 |
| `confirmar-borrar-evento` | `a#confirmar-borrar-evento` | Borrará el evento mediante la API | `POR CONFIRMAR (F0)` | B8 |

### `admin/correos.html` — Cola de correos

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el administrador | `GET /api/notificaciones` | F4 |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del administrador autenticado desde la API | `GET /api/cuentas/yo` | F4 |
| `selector-vista` | `p#selector-vista` | Solo para un Administrador con asignaciones; llega con el bloque B10 | `GET /api/cuentas/yo` y `GET /api/asignaciones` | B10 |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F4 |
| `filtros-correos` | `form#filtros-correos` | Consultará la cola de correos en la API con el estado elegido | `GET /api/correos?estado={estado}` | F4 |
| `buscar-correos` | `button#correos-buscar` | Ejecutará la búsqueda de correos en la API | `GET /api/correos?estado={estado}` | F4 |
| `tabla-correos` | `div#tabla-correos` | Cargará el resumen de la cola; la API no devuelve el cuerpo del correo | No aplica (respuesta del `GET`) | F4 |

### `admin/reportes.html` — Reportes

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el administrador | `GET /api/notificaciones` | F5 |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del administrador autenticado desde la API | `GET /api/cuentas/yo` | F5 |
| `selector-vista` | `p#selector-vista` | Solo para un Administrador con asignaciones; llega con el bloque B10 | `GET /api/cuentas/yo` y `GET /api/asignaciones` | B10 |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F5 |
| `filtros-reporte-solicitudes` | `form#filtros-reporte-solicitudes` | Consultará el reporte de solicitudes a la API | `GET /api/reportes/solicitudes-permiso?desde={desde}&hasta={hasta}` | F5 |
| `consultar-reporte-solicitudes` | `button#consultar-reporte-solicitudes` | Consultará el reporte de solicitudes en la API | `GET /api/reportes/solicitudes-permiso?desde={desde}&hasta={hasta}` | F5 |
| `tabla-reporte-solicitudes` | `div#tabla-reporte-solicitudes` | Mostrará los totales devueltos por la API | No aplica (respuesta del `GET`) | F5 |
| `filtros-reporte-asistencia` | `form#filtros-reporte-asistencia` | Consultará el reporte de asistencia a la API | `GET /api/reportes/asistencia?desde={desde}&hasta={hasta}` | F5 |
| `consultar-reporte-asistencia` | `button#consultar-reporte-asistencia` | Consultará el reporte de asistencia en la API | `GET /api/reportes/asistencia?desde={desde}&hasta={hasta}` | F5 |
| `tabla-reporte-asistencia` | `div#tabla-reporte-asistencia` | Mostrará el detalle de asistencia devuelto por la API | No aplica (respuesta del `GET`) | F5 |

### `admin/auditoria.html` — Historial de auditoría

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el administrador | `GET /api/notificaciones` | F5 |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del administrador autenticado desde la API | `GET /api/cuentas/yo` | F5 |
| `selector-vista` | `p#selector-vista` | Solo para un Administrador con asignaciones; llega con el bloque B10 | `GET /api/cuentas/yo` y `GET /api/asignaciones` | B10 |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F5 |
| `filtros-auditoria` | `form#filtros-auditoria` | Consultará el historial de auditoría en la API | `GET /api/auditoria?usuarioId={id}&entidad={entidad}&desde={desde}&hasta={hasta}` | F5 |
| `consultar-auditoria` | `button#auditoria-consultar` | Ejecutará la consulta del historial en la API | `GET /api/auditoria` | F5 |
| `tabla-auditoria` | `div#tabla-auditoria` | Mostrará el historial devuelto por la API | No aplica (respuesta del `GET`) | F5 |

### `admin/documentos.html` — Documentos

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el administrador | `GET /api/notificaciones` | F3 |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del administrador autenticado desde la API | `GET /api/cuentas/yo` | F3 |
| `selector-vista` | `p#selector-vista` | Solo para un Administrador con asignaciones; llega con el bloque B10 | `GET /api/cuentas/yo` y `GET /api/asignaciones` | B10 |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F3 |
| `subir-documentos` | `a#subir-documentos` | Abrirá la ventana para seleccionar y subir archivos | No aplica (solo `:target`) | F3 |
| `lista-documentos` | `div#lista-documentos` | Cargará los documentos del administrador desde la API | `GET /api/documentos` | F3 |
| `descargar-documento` | `a#descargar-1`, `a#descargar-2` | Descargará el archivo desde la API | `GET /api/documentos/{id}/descargar` | F3 |
| `eliminar-documento` | `a#eliminar-1`, `a#eliminar-2` | Abrirá la ventana de confirmación antes de eliminar | No aplica (solo `:target`) | F3 |
| `mensaje-sin-documentos` | `p#mensaje-sin-documentos` | Mostrará este aviso cuando la API devuelva una lista vacía | No aplica (respuesta del `GET`) | F3 |
| `form-subir-documentos` | `form#form-subir-documentos` | Validará los archivos elegidos y los enviará a la API | `POST /api/documentos` | F3 |
| `seleccionar-archivos` | `input#archivos-documento` | Tomará la lista de archivos del input y mostrará sus nombres | No aplica (lectura local) | F3 |
| `archivos-elegidos` | `ul#archivos-elegidos` | Mostrará los nombres de los archivos seleccionados en el input | No aplica (solo vista) | F3 |
| `aceptar-documentos` | `button#aceptar-documentos` | Subirá los archivos seleccionados a la API | `POST /api/documentos` | F3 |
| `confirmar-eliminar-documento` | `a#confirmar-eliminar-1`, `a#confirmar-eliminar-2` | Eliminará el documento mediante la API | `DELETE /api/documentos/{id}` | F3 |

### `admin/notificaciones.html` — Notificaciones

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el administrador | `GET /api/notificaciones` | F4 |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del administrador autenticado desde la API | `GET /api/cuentas/yo` | F4 |
| `selector-vista` | `p#selector-vista` | Solo para un Administrador con asignaciones; llega con el bloque B10 | `GET /api/cuentas/yo` y `GET /api/asignaciones` | B10 |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F4 |
| `lista-notificaciones` | `ul#lista-notificaciones` | Cargará las notificaciones del administrador desde la API | `GET /api/notificaciones` | F4 |
| `marcar-leida` | `a#marcar-leida-1`, `a#marcar-leida-2` | Marcará esta notificación como leída mediante la API | `POST /api/notificaciones/{id}/leida` | F4 |
| `preferencias-notificaciones` | `form#form-preferencias` | Guardará la preferencia de correo del administrador mediante la API | `GET` y `PUT /api/notificaciones/preferencias` | F4 |
| `preferencia-correo` | `button#guardar-preferencia` | Enviará la preferencia de correo a la API | `PUT /api/notificaciones/preferencias` | F4 |

### `admin/mi-cuenta.html` — Mi cuenta

| data-js | elemento | qué debe hacer el JavaScript | endpoint probable de la API | bloque |
| --- | --- | --- | --- | --- |
| `contador-notificaciones` | `a#contador-notificaciones` | Consultará a la API cuántas notificaciones no leídas tiene el administrador | `GET /api/notificaciones` | F1 |
| `usuario-actual` | `span#usuario-actual` | Tomará el nombre, el rol y el correo del administrador autenticado desde la API | `GET /api/cuentas/yo` | F1 |
| `selector-vista` | `p#selector-vista` | Solo para un Administrador con asignaciones; llega con el bloque B10 | `GET /api/cuentas/yo` y `GET /api/asignaciones` | B10 |
| `cerrar-sesion` | `a#cerrar-sesion` | Cerrará la sesión en la API y volverá a la pantalla pública | `POST /api/cuentas/logout` | F1 |
| `datos-cuenta` | `section#datos-cuenta` | Cargará los datos del administrador autenticado desde la API | `GET /api/cuentas/yo` | F1 |
| `cambiar-contrasena` | `button#cambiar-contrasena` | Abrirá la ventana para cambiar la contraseña | No aplica (solo `:target`) | F1 |
| `form-cambiar-contrasena` | `form#form-cambiar-contrasena` | Validará las contraseñas y pedirá el cambio a la API | `POST /api/cuentas/cambiar-contrasena` | F1 |
| `guardar-contrasena` | `button#guardar-contrasena` | Enviará el cambio de contraseña a la API | `POST /api/cuentas/cambiar-contrasena` | F1 |

## Por confirmar en el código

1. **Eventos del calendario.** Los comentarios de `admin/calendario.html` y `admin/inicio.html` hablan de crear, borrar y mostrar eventos propios (B8), pero no hay `EventsController` ni rutas para ellos; solo existen años escolares y días lectivos. Por eso esas filas dicen `POR CONFIRMAR (F0)`.
2. **Validación suelta del código de recuperación.** `recuperar-codigo.html` pide comprobar el código, pero `POST /api/cuentas/recuperacion/confirmar` valida el código **y** cambia la contraseña a la vez. Falta decidir si se añade un endpoint de solo validación o si la pantalla pasa directamente a `recuperar-nueva.html`.
3. **Filtros de listas.** `GET /api/cuentas`, `/api/materias`, `/api/cursos`, `/api/asignaciones`, `/api/permisos/pendientes` y `/api/calificaciones/mias` no exponen todos los filtros que aparecen en la maqueta; se asumió filtrado en cliente. Confirmar si se prefieren parámetros de consulta nuevos.
4. **Borradores de calificación.** El comentario `guardará la nota como borrador sin publicarla` se cubre con `POST /api/calificaciones`, que registra la nota en estado inicial. Confirmar si hace falta una acción explícita de «guardar borrador» distinta de `POST /api/calificaciones/{id}/recuperacion`.
5. **Selector de vista (B10).** No es una llamada a la API por sí mismo: depende del rol y de las asignaciones del administrador. Se documentó con `GET /api/cuentas/yo` y `GET /api/asignaciones`, pero su regla exacta de visibilidad debe confirmarse con el dueño del bloque B10.
6. **Agrupación F1–F5.** El repositorio no define formalmente esos bloques. Se usó esta correspondencia: F1 = pantallas públicas y cuenta, F2 = docente, F3 = administración, F4 = notificaciones y cola de correos, F5 = reportes y auditoría. B7 = asistencia, B8 = calendario/eventos, B9 = resultados de aprendizaje, B10 = asignaciones/acceso, según los comentarios de las propias páginas.
