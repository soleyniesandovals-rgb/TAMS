# Bitácora de la Práctica 4

## 1. Resumen

Yo, Soleyni, trabajé la Práctica 4 con dos asistentes y una regla clara: el
agente (OpenCode) escribió el código y la documentación; Claude (asistente)
revisó sus informes, planificó los bloques, redactó los prompts y buscó la API
externa; yo ejecuté las verificaciones con mis propios comandos antes de
mezclar cada PR. No mezclé ningún cambio sin haberlo probado antes.

La práctica agregó el manejador de documentos, el contenedor con migraciones
automáticas y correos, y la integración con una API externa de feriados para
calcular los días lectivos. Al final quedó un README con el inicio por Docker,
un ADR con las decisiones y una verificación manual completa desde un clon
limpio.

## 2. Cronología con PR

- **B0 (4 nov):** auditoría de solo lectura del commit `4b565e4`. No cambié
  código en este bloque.
- **B1/B2 — Manejador de documentos:** PR #41, mezclado el 7 nov (merge
  `64d9aec`).
- **B5 — Contenedor:** PR #42, 4 commits, mezclado el 8 nov (merge `a537523`).
- **B4 — API externa:** PR #43, 5 commits, mezclado el 8 nov (merge `ba0b9fa`),
  con 75 pruebas en verde.
- **B6 — README y ADR:** PR #44, mezclado el 9 nov (merge `bc81d6d`).
- **Revisión final:** durante mi verificación manual se detectaron tres
  detalles del README (el asistente los detectó al revisarlo conmigo) y el
  agente los corrigió en `2e4c854`; en la revisión final del asistente se
  detectaron siete más, corregidos en este PR.

## 3. Qué le pedí al agente en cada bloque

- **B0:** le pedí una auditoría de solo lectura del commit `4b565e4` para
  detectar problemas antes de empezar. No debía modificar nada.
- **B1/B2:** le pedí implementar el manejador de documentos. El agente hizo
  tres preguntas de diseño: qué devuelve la descarga, cómo se define
  `IAlmacenamientoArchivos` y qué se guarda en `RutaAlmacenamiento`. Elegí
  las opciones recomendadas. Lo probé de punta a punta con tres usuarios:
  archivo vacío 400, sin sesión 401, otro usuario 403, eliminación lógica que
  conserva el registro y el archivo.
- **B5:** le pedí containerizar la aplicación con migraciones automáticas con
  reintentos (RD-09), modo sin SMTP que escribe los correos en el log (RD-10),
  un Dockerfile de dos etapas sin root, un compose con `db`, `app` y
  `correos`, volúmenes con nombre y `.env.example`.
- **B4:** le pedí integrar una API externa de feriados. El contrato incluía
  detenerse y avisarme si algo no se podía verificar. Se detuvo dos veces
  porque la app de OpenCode no veía la variable `TAMS_API_FERIADOS_CLAVE`;
  acordamos que él escribiera y probara el código con dobles y que yo hiciera
  la verificación con la API real.
- **B6:** le pedí reescribir el README con inicio por Docker, la sección de la
  API externa y la guía de los seis frentes. También le pedí crear el ADR y
  verificarlo en un clon limpio.

## 4. Errores, hallazgos y cómo se resolvieron

- **Hallazgos del B0:** no había migraciones automáticas; `appsettings.json`
  tenía enlaces fijos a `http://localhost:5132`; `procesar-correos` es de una
  sola pasada y con SMTP inválido termina con código 1; con `.env.example` no
  se podía activar una cuenta sin SMTP real; `src/Core/Documentos` estaba
  vacía; no existían archivos de contenedor. El README también tenía
  imprecisiones: decía que los correos usaban un `BackgroundService` con
  reintentos y `Tams.Api.http` todavía incluía `/weatherforecast`.
- **Hallazgo del B5:** las variables de entorno de mi terminal tenían
  prioridad sobre el `.env`. El agente lo detectó y las quitó solo en su
  proceso para probar como el profesor. La verificación se hizo en un clon
  limpio: solo `app` publica puerto, las migraciones corrieron en el intento
  1, la activación se hizo leyendo el enlace del log, el documento y el
  usuario se conservaron tras `docker compose down` sin `-v`, y no apareció
  ningún secreto real en el historial (`git log --all -S`).
- **Dos paradas del B4:** la app de OpenCode no veía la variable
  `TAMS_API_FERIADOS_CLAVE`. El agente se detuvo las dos veces, como pedía el
  contrato. Acordamos separar el trabajo: él escribió y probó con dobles; yo
  verifiqué con la API real. También informó que `[RequiereRol]` admite un
  solo rol, así que los endpoints de días lectivos quedaron solo para
  Administrador.
- **Error con la clave:** al guardar la clave usé mal `Read-Host` y la clave
  quedó escrita en el chat con el asistente. La regeneré de inmediato y todo
  lo verificado después usa la clave nueva.
- **Errores de SQL:** los primeros comandos `sqlcmd` que me dio el asistente
  fallaron por faltar `-I` (`QUOTED_IDENTIFIER`, `Msg 1934`) y el `INSERT` del
  año escolar, también del asistente, por faltar `FechaCreacion` (`Msg 515`);
  se corrigieron y quedaron documentados en el README.
- **Corte de internet en el B6:** el agente se detuvo a mitad del bloque, pero
  no dejó cambios a medias.
- **Detalles del README:** durante mi verificación manual el asistente detectó
  tres detalles (el procedimiento del 502 usaba el token del docente, faltaba
  el login después de reiniciar y una frase mal escrita) y el agente los
  corrigió en `2e4c854`. En la revisión final el asistente detectó siete más
  (el período 1 chocaba con el frente 6; el 403 de corregir puntaje no aparece
  después del frente 5 porque el permiso es global; la salida real de
  `dotnet test`; la estructura del proyecto desactualizada; una frase del año
  escolar de prueba; las fechas del año de `datos-demo.sql`; y `tipoPermiso`
  con mayúscula), corregidos en este PR.

## 5. Decisiones

- **API de feriados:** comparé cuatro APIs y elegí Calendarific. Su plan
  gratuito permite 500 llamadas al mes con atribución; Abstract API exige año,
  mes y día en el plan gratuito; HolidayAPI.com solo permite años pasados; API
  Ninjas solo permite el año actual. Probé mi clave con República Dominicana
  en 2026 y 2027 y obtuve código 200.
- **Diseño del manejador de documentos:** elegí las opciones recomendadas por
  el agente para la descarga, la interfaz de almacenamiento y el dato que se
  guarda en `RutaAlmacenamiento`.
- **Variables de entorno:** quedó claro que las variables de mi terminal
  tienen prioridad sobre el `.env`. Para probar como el profesor hay que
  quitarlas en el mismo proceso.
- **Roles:** como `[RequiereRol]` admite un solo rol, los endpoints de días
  lectivos quedaron restringidos a Administrador.
- **Verificación con la API real:** después de las dos paradas del B4, decidí
  que el agente escribiera y probara con dobles y que yo hiciera la
  verificación con la clave real.

## 6. Verificación manual

El 9 de noviembre seguí el README al pie de la letra en un clon limpio, sin
mis variables reales, y levanté el contenedor con un solo comando. Verifiqué
los seis frentes:

1. **Registro y activación:** 403 antes de activar, 400 al reusar el enlace y
   409 al repetir el correo.
2. **Sesión:** el mismo 401 y el mismo mensaje para clave mala y correo
   inexistente; el logout invalida el token.
3. **Roles:** rechazo por rol con 403.
4. **Recuperación:** respuesta idéntica exista o no el correo, código de un
   solo uso y sesiones anteriores invalidadas.
5. **Permisos:** 400 sin motivo, rechazo, 403 antes y acceso concedido después
   de aprobar, y 409 al aprobar dos veces.
6. **Negocio:** Borrador → Enviada → Publicada, 409 al reenviar una publicada
   y la prueba unitaria de Publicada → Borrador.

También comprobé la persistencia tras `docker compose down` y `docker compose
up`, y la API externa con 503, 502 y 404. En el B5 ya había comprobado que
solo `app` publica puerto, que las migraciones corren en el intento 1, que la
activación funciona leyendo el enlace del log, que el documento y el usuario
se conservan tras `docker compose down` sin `-v`, y que no hay secretos
reales en el historial.

En la verificación del B4 comprobé que 220 días hábiles menos 11 feriados dan
209 días lectivos (coincidió con el cálculo independiente que hizo el
asistente), que la clave no aparece en los logs, que con clave inválida
responde 502, que sin clave responde 503 y el resto del sistema sigue
funcionando, que un año inexistente responde 404 y que un Estándar recibe 403.
El 9 de noviembre ejecuté yo misma `dotnet test TAMS.slnx` en esta rama: 75
pruebas, 0 errores. El 9 de noviembre busqué en todo el historial
(`git log --all -S`) los valores de mis tres claves reales (SMTP, JWT y
Calendarific) y no aparecieron en ningún commit.

Durante mi verificación manual se detectaron tres detalles del README (el
asistente los detectó al revisarlo conmigo) y el agente los corrigió en
`2e4c854`; en la revisión final del asistente se detectaron siete más,
corregidos en este PR.

## 7. Fuentes

- Material del curso sobre contenedores (Docker Docs, de la lista de fuentes del profesor).
- Documentación y precios de Calendarific.
- Páginas de precios y documentación de Abstract API, HolidayAPI.com y API Ninjas, usadas por el asistente para comparar las APIs de feriados.
- Código fuente de dotnet/runtime, revisado por el agente para comprobar qué registra IHttpClientFactory.
- ADR de la práctica: [`docs/adr/0001-decisiones-practica-4.md`](adr/0001-decisiones-practica-4.md).
