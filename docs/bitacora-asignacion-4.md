# Bitácora de la Asignación 4 — Auditoría

Soy Soleyni Sandoval. En esta bitácora registro cómo trabajé en la Asignación 4, desde la auditoría de solo lectura hasta el recorrido final del README.

## 1. Cómo trabajé

Claude (el asistente de claude.ai) propuso las decisiones, redactó los borradores de los criterios y de `AGENTS.md`, escribió los prompts y revisó cada entrega. El agente (OpenCode) hizo la auditoría de solo lectura y escribió el código, las pruebas y el README. Yo acepté las decisiones, guardé y confirmé los criterios y `AGENTS.md`, ajusté CA-12, creé la migración, verifiqué cada bloque con SQL Server real y mezclé los PR.

## 2. Contexto y criterios antes del código

El 21 de noviembre, antes de escribir código, el agente hizo la auditoría de solo lectura sobre el commit `c3178c7`: `src/Core/Auditoria` estaba vacío; hizo el mapa de los diez eventos con archivo y línea; comprobó que en los diez el cambio se guarda en un solo guardado de la unidad de trabajo del Core; vio que reactivar y el restablecimiento forzado no recibían al Administrador; y vio que en la recuperación no hay sesión y el actor es el dueño del código.

Claude propuso diez decisiones y yo las acepté todas:

- Los 10 eventos del Core, incluido el cambio de contraseña con sesión, y nada del negocio.
- Valores en texto corto legible, nunca hashes ni códigos.
- Registro en el mismo guardado.
- Inmutabilidad en la aplicación, sin trigger.
- `GET /api/auditoria` con filtros opcionales `usuarioId`, `entidad`, `desde` y `hasta`, máximo 500, del más reciente al más antiguo.
- Validación propia de la pieza Auditoría.
- Diez acciones con nombre cerrado.
- Solo operaciones con éxito.
- El Administrador como actor desde el token.
- Fechas en UTC con `Z`.

Ese mismo 21 de noviembre, Claude redactó `docs/criterios-auditoria.md` (CA-01 a CA-23, con numeración propia para no mezclarla con los C-xx de la Práctica 5 ni con los RF-AUD-xx) y los cambios de `AGENTS.md`. Yo los revisé, los guardé y los confirmé (commit `6f4a3b7`, PR #61, merge `a5eef5e`), antes del primer commit de código.

## 3. Bloques

### B2 — Entidad, registrador y repositorio (21 nov)

**Qué se pidió:** la base del registro de auditoría: una entidad inmutable, los enums, la configuración, un repositorio de solo agregar en la unidad de trabajo y un registrador que no guardara por sí mismo, con sus pruebas.

**Qué entregó el agente:** la entidad `RegistroAuditoria` inmutable (sin setters públicos, solo se crea con `Crear`, recorta los valores a 500 caracteres), los enums, la configuración, el repositorio de solo agregar en la unidad de trabajo y el registrador que no guarda por sí mismo (commits `c900024`, `e14dcad`, `1ce5b0b`; 6 pruebas, 218 en total).

**Qué falló y cómo se corrigió:** el B2 no necesitó correcciones. Yo creé la migración `AgregaAuditoria` y la revisé: solo un `CreateTable` en el esquema core y tres índices (commit `15773eb`). PR #62, merge `09e3589`.

**Cómo lo verifiqué:** Revisé la migración (solo un CreateTable en el esquema core y tres índices); Claude revisó el código y las pruebas pasaron (218). La tabla recibió datos reales en la verificación del B3, con SQL Server real.

### B3 — Los diez eventos (21 nov)

**Qué se pidió:** registrar los diez eventos del Core, después de las validaciones y antes del guardado, solo si la operación tiene éxito, con el Administrador como actor desde el token.

**Qué entregó el agente:** los diez eventos (commits `e1fa629`, `d82940d`, `d9f52de`, `54214ad`; 26 pruebas, 244 en total), registrados después de las validaciones y antes del guardado. Reactivar y el restablecimiento forzado reciben ahora al Administrador desde el token. En la subida de documentos el id lo asigna la base al guardar, así que el documento y su registro se guardan en una misma transacción; por eso ajusté después CA-12 (commit `311ce5b`, PR #68, merge `25d9f83`). PR #63, merge `1dd446e`.

**Qué falló y cómo se corrigió:** el B3 no necesitó correcciones. Para la subida de documentos, Claude redactó el nuevo texto de CA-12 y yo lo revisé, lo guardé y lo confirmé (commit 311ce5b).

**Cómo lo verifiqué:** con SQL Server real: exactamente 10 registros con los valores de los criterios; el rol repetido, aprobar la propia solicitud (409), cambiarse el propio rol (409) y la contraseña actual incorrecta (400) no dejaron registro.

### B4 — La consulta (21 nov)

**Qué se pidió:** `GET /api/auditoria` con filtros opcionales `usuarioId`, `entidad`, `desde` y `hasta`, máximo 500, del más reciente al más antiguo, con validación propia de la pieza Auditoría y acceso solo del Administrador.

**Qué entregó el agente:** los filtros aplicados en la base, `usuarioId` entero positivo, `entidad` solo por nombre exacto, fechas inclusivas sin error con `9999-12-31`, `fechaHora` marcada como UTC y `AuditoriaController` solo con GET para el Administrador (commits `bc49447`, `d6b0d2d`, `b4d5911`).

**Qué falló y cómo se corrigió:** 13 nombres de prueba usaban números de criterio como si fueran requisitos (`RF_AUD_14`, `RF_AUD_15`, `RF_AUD_16` y `RF_AUD_18`; los requisitos de auditoría solo van de RF-AUD-01 a RF-AUD-07). Claude lo detectó al revisar los nombres de las pruebas; el agente corrigió solo los nombres, sin tocar ninguna aserción ni el código (commit `a0461c9`).

**Cómo lo verifiqué:** por HTTP: filtros, seis 400, 403, 401, DELETE y PUT 405, y los registros siguen tras reiniciar. PR #64, merge `e2d6dd4` (con la corrección `a0461c9`; 275 pruebas).

### README (26 nov)

**Qué se pidió:** escribir la sección «Auditoría» del README.

**Qué entregó el agente:** la sección «Auditoría» (commits `e9b6317`, `543b3ff`, `ef4e9a8`).

**Qué falló y cómo se corrigió:** Claude encontró en la revisión dos detalles: la estructura de las pruebas sin la auditoría y una flecha del diagrama del negocio hacia Auditoría que no era real. El agente los corrigió (`447c30c`). PR #69, merge `733baf0`.

**Cómo lo verifiqué:** ese día hice el recorrido completo en un clon limpio: los seis frentes funcionaron igual que antes; la sección «Auditoría» funcionó tal como está escrita: los diez eventos, las dos operaciones rechazadas sin registro, 15 registros en total, cada filtro, seis 400, 403, 401, dos 405 y 15 registros antes y después de reiniciar. El id de Otro fue 1002 y no 3, porque el reinicio del frente 7 hizo saltar los ids; el README funcionó porque saca los ids de la lista en vez de suponerlos.

## 4. El error del agente

> **El error del agente (B4):** 13 nombres de prueba usaban números de criterio como si fueran requisitos: `RF_AUD_14`, `RF_AUD_15`, `RF_AUD_16` y `RF_AUD_18`. Los requisitos de auditoría solo van de RF-AUD-01 a RF-AUD-07. Es la misma confusión de numeraciones de la Práctica 5, y `AGENTS.md` la prohíbe expresamente desde la Asignación 3 (commit `3564516`). Claude lo detectó al revisar los nombres de las pruebas; el agente corrigió solo los nombres, sin tocar ninguna aserción ni el código (commit `a0461c9`). Lección: una regla escrita en el contexto reduce el riesgo, pero no sustituye la revisión.

Otro error menor, sin consecuencias en el código: en el informe de la auditoría B0 se le coló texto en otro idioma en mitad de una frase y una errata.

## 5. Aciertos del agente

- En el B4 se detuvo al comprobar que main no tenía el merge del B3, como pedía el prompt.
- Detectó que el comando `Select-String -Recurse` que le dio Claude no existe en PowerShell y usó `Get-ChildItem -Recurse | Select-String`.
- El B2 y el B3 no necesitaron correcciones.

## 6. Errores del asistente

- Redactó CA-12 con «mismo guardado» sin prever la subida de documentos.
- Dio un comando `Select-String -Recurse` que no existe.
- En un resultado esperado supuso que Otro tendría el id 3.

## 7. Incidentes míos y del entorno

- Lancé el B4 antes de mezclar el PR del B3 (el agente se detuvo).
- Volví a ejecutar los comandos de commit y push de los criterios cuando ya estaban mezclados, sin daño.
- Docker Desktop estaba cerrado varias veces al empezar una verificación.

## 8. Registro de auditoría frente a commit de git

| | Registro de auditoría (TAMS) | Commit de git |
|---|---|---|
| Qué guarda | Una acción sobre un dato de la aplicación: quién, qué acción, sobre qué entidad, cuándo y el valor anterior y el nuevo | Un cambio en el código: autor, fecha, mensaje y las diferencias línea por línea |
| Quién lo crea | La aplicación, automáticamente, cuando una operación termina con éxito | Una persona (o el agente) a propósito, con git commit |
| Quién aparece como autor | El usuario del token (usuarioId); no se puede escribir a mano | El nombre y el correo configurados en git; se pueden poner a mano |
| Cuándo | fechaHora en UTC, tomada del reloj del servidor | La fecha del equipo donde se hizo el commit |
| Antes y después | Textos cortos (por ejemplo «Estandar» → «Administrador») | El diff completo del archivo |
| Identificador | Un número que asigna la base (en mi recorrido saltó de 4 a 1002 tras un reinicio) | Un hash calculado a partir del contenido (por ejemplo d82940d) |
| ¿Se puede cambiar? | No desde la aplicación: no hay PUT, PATCH ni DELETE (405) y la entidad no tiene setters; sí con acceso directo a la base, porque no hay trigger | Sí en local (amend, rebase, cherry-pick, como hice en la Práctica 5), pero el hash cambia y en GitHub hace falta un push forzado |
| Cómo se consulta | GET /api/auditoria con filtros, solo el Administrador | git log, git show, git blame |
| Para qué sirve | Responder quién hizo qué sobre los datos del sistema en funcionamiento | Responder quién cambió el código, cuándo y por qué, y poder revertirlo |
| Ejemplo real | Registro 1006 del 26 nov: el Administrador (1) cambió el rol del usuario 1002 de Estandar a Administrador | Commit d82940d del 21 nov: «feat(A4): audita la subida y la eliminación de documentos (RF-AUD-03)» |

Los dos son historiales que responden «quién hizo qué y cuándo», pero de cosas distintas: el registro de auditoría vigila los datos mientras el sistema funciona, y el commit vigila el código mientras se construye. Un commit de este proyecto (d82940d) es justo el que hace que la aplicación escriba los registros de auditoría de los documentos.

## 9. Qué cambio para la próxima vez

- Revisar siempre los nombres de las pruebas, aunque la regla esté en el contexto.
- En el README, sacar los ids de las respuestas y no suponerlos.
- Comprobar que main tiene el merge anterior antes de lanzar un bloque.
- Abrir Docker Desktop antes de verificar.

## 10. Fuentes

- El enunciado de la Asignación 4.
- `Requerimientos_Core_PIII.docx` (pieza 6, RF-AUD-01 a 07).
- `docs/criterios-auditoria.md`.
- `AGENTS.md`.

No se consultaron fuentes externas.

## 11. Tabla de PR

| PR | Rama | Merge | Contenido |
|---|---|---|---|
| #61 | docs/a4-contexto-criterios | a5eef5e | criterios y AGENTS.md |
| #62 | feat/auditoria | 09e3589 | entidad, registrador y migración |
| #63 | feat/auditoria-eventos | 1dd446e | los diez eventos |
| #64 | feat/auditoria-consulta | e2d6dd4 | la consulta y la corrección de los nombres de prueba |
| #68 | docs/a4-criterio-ca12 | 25d9f83 | ajuste de CA-12 |
| #69 | docs/a4-readme | 733baf0 | README «Auditoría» |
