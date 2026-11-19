# Bitácora de la Práctica 5

Soleyni Sandoval

## 1. Cómo trabajé

Trabajé con tres roles claramente separados. Claude, el asistente de claude.ai, propuso las decisiones de diseño, redactó los borradores de AGENTS.md y de los criterios, escribió los prompts y revisó cada entrega. OpenCode escribió el código, las pruebas y el README. Yo aprobé las decisiones, creé y confirmé AGENTS.md y los criterios, creé las migraciones, reordené y renombré commits, verifiqué todo a mano y mezclé los PR.

Usé una conversación nueva de OpenCode por bloque. Cuando reutilicé una conversación en el B2 y en el B5, el agente se detuvo a mitad del trabajo o al empezar; comprobé el estado con Git y retomé cada bloque en una conversación nueva.

Antes de cada PR verifiqué el resultado a mano: con SQL Server real cuando el caso lo pedía, con un SMTP inexistente para la cola y con un recorrido completo del README en un clon limpio. Revisé también los commits, el orden en que compilaban y que no se hubiera borrado ninguna aserción existente.

## 2. Contexto y criterios antes de delegar

Antes de delegar, Claude propuso 11 decisiones de diseño y yo las acepté:

1. El evento del negocio sería crear una asignación: notifica al docente.
2. Crear una solicitud notifica a todos los Administradores activos.
3. Resolverla notifica al solicitante con el motivo.
4. Los permisos se guardan en una sola operación: primero la asignación y después la notificación; documenté esta limitación.
5. La preferencia de correo es global, está activada por defecto y solo aplica a notificaciones.
6. El máximo es 3 intentos y existe el estado Fallido.
7. La consulta de la cola es solo para el Administrador y no muestra el cuerpo.
8. El reporte del Core es por mes y estado.
9. El reporte del negocio es el porcentaje de asistencia por estudiante y materia, calculado como (Presente + Tardanza) / total × 100.
10. «Desde» y «hasta» son obligatorios y ambos se incluyen.
11. Las fechas nuevas se devuelven en UTC con Z.

También antes de delegar, Claude redactó los borradores de AGENTS.md y de `docs/criterios-notificaciones-reportes.md`, con 27 criterios (C-01 a C-27) en formato dado/cuando/entonces y con su requisito. Yo los revisé, los aprobé, los creé en VS Code, verifiqué la codificación e hice el commit `a8df6a7` (PR #52, merge `909429b`). Ese commit es anterior a `da4398f`, el primer commit de notificaciones.

## 3. Bloques

### B0 — Auditoría inicial (17 nov)

**Qué se pidió.** Una auditoría de solo lectura del agente sobre `a94f524`, que tenía 147 pruebas.

**Qué entregó.** Los hallazgos fueron: la cola no tenía intentos, último error ni estado Fallido, y un fallo cortaba la pasada con un `throw`; no había código de notificaciones; no había consulta de Administradores activos; el evento del negocio no podía guardarse en la misma transacción que el Core porque había dos DbContext; no había ningún `GROUP BY` sobre `IQueryable`; y las fechas se leían de `datetime2` sin Z (RD-11). El agente dijo también que `src/Core/Notificaciones` no existía; dato que resultó inexacto.

**Qué falló y cómo se corrigió.** Verifiqué el dato con `Test-Path`: la carpeta existía, vacía, sin archivos ni seguimiento en Git. Lo dejé constatado antes de seguir.

**Cómo lo verifiqué.** Comprobé por mi cuenta la ruta con `Test-Path` y contrasté los hallazgos con las decisiones que acepté ese mismo día.

### B1 — AGENTS.md y criterios (17 nov)

**Qué se pidió.** Dejar por escrito el contexto del agente y los criterios de aceptación antes de empezar las entregas.

**Qué entregó.** Claude redactó los borradores de AGENTS.md y de `docs/criterios-notificaciones-reportes.md`: 27 criterios, C-01 a C-27, en formato dado/cuando/entonces con su requisito.

**Qué falló y cómo se corrigió.** No hubo fallos en este bloque.

**Cómo lo verifiqué.** Yo revisé y aprobé ambos documentos, los creé en VS Code, verifiqué la codificación e hice el commit `a8df6a7` (PR #52, merge `909429b`), anterior a `da4398f`.

### B2 — Notificaciones (17-18 nov)

**Qué se pidió.** Implementar las notificaciones según las decisiones aceptadas y los criterios C-01 a C-27.

**Qué entregó.** El agente se detuvo a mitad del B2-A; comprobé el estado con Git y lo retomé en una conversación nueva. El commit del B2-A fue `da4398f` (entidades y repositorios). Yo creé la migración `AgregaNotificaciones` (`eb53ac0`) y la revisé. En el B2-B el agente entregó `f7019a4`, `4b18324`, `332a0f5`, `0511dd8` y `8c9bc6c`, con 12 pruebas nuevas (159 en total).

**Qué falló y cómo se corrigió.** Claude detectó que el agente había confundido los números de criterio (C-xx) con los de requisito (RF-NOT-xx) en tres nombres de pruebas y un comentario. El agente lo corrigió en `282f6b2` y encontró por su cuenta tres referencias más del mismo tipo. Yo renombré dos archivos de prueba con `git mv` (`3d31ec5`). El mensaje de `332a0f5` cita RF-NOT-06 por error y no se reescribió porque ya estaba subido. Comprobé que no se borró ninguna aserción existente.

**Cómo lo verifiqué.** El 18 nov verifiqué a mano con SQL Server real los criterios C-01, C-03, C-05 a C-11, además de 400 y 401. PR #53, merge `10b66c0`.

### B3 — Cola de correos (18 nov)

**Qué se pidió.** Implementar los reintentos, el estado Fallido y la consulta restringida de la cola.

**Qué entregó.** El agente entregó B3-A en `fd2a75a`. Yo creé la migración `AgregaReintentosCorreo` (`e6fca21`), con solo tres `AddColumn`. En B3-B el agente puso el commit de pruebas antes del commit del servicio de consulta que esas pruebas usaban.

**Qué falló y cómo se corrigió.** Ese commit (`8bb5b57`) no compilaba solo (CS0246), aunque el informe decía 165 en verde. Claude lo sospechó por el orden de los commits; yo lo confirmé compilando ese commit y lo corregí reordenando con cherry-pick (`c209ec1`, `eaff3c9`, `6f063f3`). Comprobé que el contenido final era idéntico, que cada commit compilaba y subí con `--force-with-lease`.

**Cómo lo verifiqué.** Verifiqué a mano con un SMTP inexistente (`smtp.no-existe.invalid`): el correo pasó por 1 y 2 intentos y quedó Fallido con 3 y «SocketException: Name or service not known», sin la credencial. Los Fallido no se reintentaron. La consulta de la cola no muestra el cuerpo y da 400, 403 y 401. PR #54, merge `af7623d`.

### B4 — Reportes (18 nov)

**Qué se pidió.** Implementar el reporte del Core por mes y estado y el reporte de asistencia del negocio, con «desde» y «hasta» obligatorios e incluidos.

**Qué entregó.** El agente entregó `090ff71`, `aabae00`, `4c76b21` y `4bb1c6f`, con 19 pruebas nuevas (184 en total). Su informe no incluyó el código ni la lista de pruebas que yo había pedido.

**Qué falló y cómo se corrigió.** Extraje el código con mis comandos y Claude lo revisó: la agrupación se hace en la base antes de traer los datos, el último día está incluido, hay filtrado por rol y no se muestra la cédula.

**Cómo lo verifiqué.** Con SQL Server real comprobé que la agrupación funcionó en SQL Server, que una solicitud de las 23:59:30 del último día cuenta, que cada docente ve solo lo suyo, y 400 y 401. En asistencia obtuve 33.33 y 100, y 50 y 100 sin el último día. PR #55, merge `25f69f9`.

### B5 — README (18-19 nov)

**Qué se pidió.** Documentar la práctica en el README, de forma que otra persona pudiera ejecutarlo siguiendo el orden del documento.

**Qué entregó.** El agente se detuvo al empezar porque reutilicé la conversación del bloque anterior; lo retomé en una nueva. La versión inicial estuvo en `11d6952`, `4efeb75`, `eb0a8f0` y `99232c5`.

**Qué falló y cómo se corrigió.** Claude revisó el diff y encontró que el reporte de solicitudes usaba fechas fijas (las solicitudes del profesor tendrán la fecha del día en que las cree y los dos rangos saldrían vacíos) y que no decía dónde mirar si la asignación ya existía (409). Se corrigió en `b298bcd`, `68d3c5e`, `80bda5d`, `6aa24ec` y `c9c9f03`. Después Claude revisó el README completo contra la lista «Qué se revisa exactamente» del enunciado y encontró que faltaban comandos (cola como Estándar, procesador dos veces, errores de los reportes), que la contraseña del docente en Notificaciones era la anterior al frente 4 y que AGENTS.md y los criterios no figuraban en la estructura. Se corrigió en `e0183fd`, `ed0c9a0`, `4737a83`, `f82cf86` y `53b7176`.

**Cómo lo verifiqué.** El 19 nov hice el recorrido completo del README en un clon limpio, como lo haría el profesor: arranque, primer Administrador, datos de demostración, los seis frentes, notificaciones, cola con SMTP falso y reportes; todo funcionó. Encontré dos detalles: Datos de demostración antes del frente 1 sin los comandos del docente, y las líneas de log del procesador manual. Los corregí en `cabc4e1` y `0b0717c`. PR #56, merge `6d77ca2`.

## 4. Errores del agente detectados y corregidos

| Bloque | Error | Quién lo detectó | Corrección | Commit |
| --- | --- | --- | --- | --- |
| B0 | Dijo que `src/Core/Notificaciones` no existía; existía vacía, sin archivos ni seguimiento en Git | Yo, con `Test-Path` | Constatar la existencia real de la carpeta antes de seguir | Sin commit (auditoría de solo lectura) |
| B2 | Confundió números de criterio (C-xx) con números de requisito (RF-NOT-xx) en tres nombres de pruebas y un comentario | Claude | Corregir las referencias; el agente encontró tres más por su cuenta | `282f6b2` |
| B2 | Dos archivos de prueba con nombre incorrecto | El agente localizó las referencias; yo renombré los archivos | `git mv` de dos archivos de prueba | `3d31ec5` |
| B2 | El mensaje de `332a0f5` cita RF-NOT-06 por error | Claude | No se reescribió porque ya estaba subido | Sin corrección (`332a0f5`) |
| B3 | El commit de pruebas (`8bb5b57`) iba antes del servicio de consulta que usaban y no compilaba solo (CS0246), aunque el informe decía 165 en verde | Claude lo sospechó por el orden de los commits; yo lo confirmé compilando ese commit | Reordenar con cherry-pick, comprobar que el contenido final era idéntico, que cada commit compilaba y subir con `--force-with-lease` | `c209ec1`, `eaff3c9`, `6f063f3` |
| B4 | El informe no incluyó el código ni la lista de pruebas pedidos | Yo | Extraer el código con mis comandos; Claude lo revisó | `090ff71`, `aabae00`, `4c76b21`, `4bb1c6f` |
| B5 | El reporte de solicitudes usaba fechas fijas: las solicitudes del profesor tendrán la fecha del día en que las cree y los dos rangos saldrían vacíos | Claude | Corregir el README | `b298bcd`, `68d3c5e`, `80bda5d`, `6aa24ec`, `c9c9f03` |
| B5 | No decía dónde mirar si la asignación ya existía (409) | Claude | Añadir la indicación al README | `b298bcd`, `68d3c5e`, `80bda5d`, `6aa24ec`, `c9c9f03` |
| B5 | Faltaban comandos (cola como Estándar, procesador dos veces, errores de los reportes) | Claude, al revisar el README completo contra «Qué se revisa exactamente» | Añadir los comandos | `e0183fd`, `ed0c9a0`, `4737a83`, `f82cf86`, `53b7176` |
| B5 | La contraseña del docente en Notificaciones era la anterior al frente 4 | Claude | Actualizar la contraseña en el README | `e0183fd`, `ed0c9a0`, `4737a83`, `f82cf86`, `53b7176` |
| B5 | AGENTS.md y los criterios no figuraban en la estructura del README | Claude | Añadirlos a la estructura | `e0183fd`, `ed0c9a0`, `4737a83`, `f82cf86`, `53b7176` |
| B5 | Datos de demostración antes del frente 1 sin los comandos del docente | Yo, en el recorrido en clon limpio | Corregir el README | `cabc4e1` |
| B5 | Líneas de log del procesador manual | Yo, en el recorrido en clon limpio | Corregir el README | `0b0717c` |

## 5. Errores del asistente

Claude dio `@(Invoke-RestMethod ...).Count`, que en PowerShell 5.1 siempre da 1, y un `Where-Object` directo sobre `Invoke-RestMethod` que no filtraba; se corrigió guardando antes en una variable.

Claude dio `Move-Item` y `Remove-Item` encadenados sin comprobar la ruta, y se borró la carpeta donde guardé por error dos informes. Los recuperé con «Guardar como» desde las pestañas de VS Code.

En la primera revisión del README Claude solo miró el diff y no vio los huecos frente a la rúbrica que encontró en la segunda.

## 6. Incidentes míos y del entorno

Guardé informes dentro del repositorio. Docker Desktop y WSL dejaron de responder y tuve que reiniciar. Una vez me salté el paso de ascender al Administrador. Di por mezclado un PR antes de pulsar «Merge». Reutilicé una conversación de OpenCode. En el recorrido ejecuté dos veces los frentes 5 y 6; el 409 de la calificación repetida es el esperado.

## 7. Observaciones para la Asignación 3

No corregidas aquí:

- En la misma respuesta de permisos, `fechaCreacion` sale sin Z y `fechaResolucion` con Z.
- `fechaSubida` sale con Z al subir un documento y sin Z al listarlo (RD-11, anterior a esta práctica).
- Un correo que quede en Enviando si el proceso se cae no se recupera (limitación documentada).

## 8. Qué cambio para la próxima vez

- Usar una conversación nueva de OpenCode por bloque.
- Pedir que cada commit compile por sí solo y comprobarlo.
- Exigir el código en el informe y verificarlo con mis comandos.
- No mezclar las numeraciones C-xx y RF en los criterios.
- Revisar el README con la lista de lo que el profesor revisa, pensando en el día en que él lo ejecutará y siguiendo el orden del documento.
- Guardar los informes fuera del repositorio.

## 9. Fuentes

- El enunciado de la Práctica 5.
- `Requerimientos_Core_PIII.docx`.
- `docs/criterios-notificaciones-reportes.md`.
- `AGENTS.md`.

No se consultaron fuentes externas en esta práctica.

## 10. Tabla final de PR

| Número | Rama | Merge | Contenido |
| --- | --- | --- | --- |
| #52 | Bloque B1: AGENTS.md y criterios | `909429b` | AGENTS.md y `docs/criterios-notificaciones-reportes.md` con 27 criterios (C-01 a C-27), commit `a8df6a7` |
| #53 | Bloque B2: notificaciones | `10b66c0` | Entidades, repositorios, migración `AgregaNotificaciones`, bandeja, preferencia y avisos por evento; 12 pruebas nuevas (159 en total) |
| #54 | Bloque B3: cola de correos | `af7623d` | Reintentos, estado Fallido y consulta restringida de la cola; migración `AgregaReintentosCorreo` |
| #55 | Bloque B4: reportes | `25f69f9` | Reporte del Core por mes y estado y reporte de asistencia por estudiante y materia; 19 pruebas nuevas (184 en total) |
| #56 | `docs/practica5-readme` | `6d77ca2` | README de la Práctica 5, corregido y recorrido en clon limpio |
