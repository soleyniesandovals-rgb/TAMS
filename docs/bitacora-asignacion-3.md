# Bitácora de la Asignación 3

Soy Soleyni Sandoval.

## 1. Cómo hice la auditoría

El 20 de noviembre, el agente (OpenCode) hizo una auditoría de solo lectura sobre `9f7d5d2` (tag `practica-5`) contra los criterios C-01 a C-25, los RF-NOT y RF-REP y los RD-02, RD-03, RD-06, RD-07, RD-08, RD-10 y RD-11. La auditoría arrojó 9 candidatos: 4 confirmados en el código y 5 hipótesis. Claude seleccionó los 4 que se pueden reproducir con comandos.

Yo los reproduje el 20 de noviembre en un clon limpio con Docker y SQL Server real sobre `9f7d5d2`, y después los repetí sobre `79a6d1d` (la rama corregida): ya no se reproducían. Guardé las salidas en mi carpeta de respaldos, fuera del repositorio.

La autoría quedó así: OpenCode hizo la auditoría de solo lectura y escribió las correcciones y las pruebas; Claude seleccionó los hallazgos, escribió los prompts, revisó cada entrega y redactó los borradores de los cambios a `AGENTS.md` y a los criterios; yo reproduje cada hallazgo en el contenedor antes y después, aprobé y guardé los cambios de `AGENTS.md` y de los criterios, y mezclé los PR.

## 2. Hallazgos

### H1

**Hallazgo:** H1 — «Una fecha extrema válida provoca un error 500 en el reporte de solicitudes» — gravedad media.

**Qué se pidió:** En la Práctica 5 (prompt del bloque B4), «desde y hasta obligatorios con formato AAAA-MM-DD, ambos incluidos», con el filtro «< (hasta + 1 día)». Los criterios C-24 y C-25 pedían 400 si faltaba un extremo, si el formato era inválido o si `desde` era posterior a `hasta`, y 200 con lista vacía para un rango sin datos.

**Qué entregó el agente (con la evidencia):** `RangoFechasReporte` calculaba `Hasta.AddDays(1)` sin protegerse (commit `090ff71`, PR #55). Evidencia en `9f7d5d2`: `GET /api/reportes/solicitudes-permiso?desde=9999-12-31&hasta=9999-12-31` → HTTP 500 y, en el log de `app`, «System.ArgumentOutOfRangeException: Value to add was out of range»; el reporte de asistencia con las mismas fechas → HTTP 200 con `[]`.

**Por qué pasó:** Criterio ausente: C-25 cubría el formato y el orden, pero no los valores límite. El prompt de Claude indicaba «< hasta + 1 día» sin advertir del último día representable, y la revisión del código (de Claude y mía) no pensó en ese caso: revisión insuficiente.

**Corrección:** La hizo el agente con el prompt del bloque A3-B1: el rango sin límite superior cuando `hasta` es el último día representable, sin dejar de incluirlo, corregido donde se calcula y donde se usa en la consulta. Commit `6c547d0` (PR #58, merge `05291dc`). 4 pruebas nuevas `A3_H1`: sobre el código anterior fallaron 3 de 4 (la cuarta confirma que C-24 sigue igual); sobre el corregido pasaron las 4. Después: HTTP 200 con `[]`.

**Qué cambias:** C-25 ahora incluye las fechas extremas (`0001-01-01`, `9999-12-31`) y `AGENTS.md` pide que una entrada con formato válido nunca provoque una excepción y que las pruebas cubran los valores límite.

### H2

**Hallazgo:** H2 — «El filtro de estado de la cola acepta números y combinaciones» — gravedad baja.

**Qué se pidió:** En la Práctica 5 (prompt del bloque B3-B), «?estado= (Pendiente, Enviando, Enviado o Fallido, sin distinguir mayúsculas; cualquier otro valor → 400)»; C-17 decía «un estado inválido responde 400».

**Qué entregó el agente (con la evidencia):** `Enum.TryParse` con `Enum.IsDefined` (commit `eaff3c9`, PR #54), que acepta números («0») y combinaciones separadas por comas («Pendiente,Enviado»). Evidencia en `9f7d5d2`: `?estado=0` → HTTP 200 y `?estado=Pendiente,Enviado` → HTTP 200; `?estado=Perdido` → HTTP 400. La misma causa estaba en la lectura del rol del token en `ReportesController` (Práctica 5, bloque B4) y en `DocumentosController`.

**Por qué pasó:** Criterio ambiguo: «estado inválido» sin ejemplos de números ni de combinaciones; la prueba de la Práctica 5 solo probó «desconocido»; revisión insuficiente.

**Corrección:** La hizo el agente con el prompt del bloque A3-B1: solo se aceptan los nombres; la causa repetida se corrigió en el mismo commit con la regla `ReglasRolUsuario` de Control de acceso (también usada por `AdministracionCuentasServicio`). Commit `4599bf6` (PR #58). 19 pruebas nuevas `A3_H2`: sobre el código anterior no compilan, porque `ReglasRolUsuario` no existía; conservando solo esa clase, fallaron 3 de 19; sobre el corregido pasaron las 19. Después: `?estado=0` y `?estado=Pendiente,Enviado` → HTTP 400 con el mensaje de estados válidos, `?estado=fallido` → HTTP 200.

**Qué cambias:** C-17 ahora dice que solo se aceptan los nombres y que los números y las combinaciones con comas responden 400; `AGENTS.md` pide aceptar solo los nombres exactos al convertir texto en un enum.

### H3

**Hallazgo:** H3 — «El Administrador recibe la notificación de su propia solicitud» — gravedad baja.

**Qué se pidió:** En la Práctica 5, la decisión «crear una solicitud notifica a todos los Administradores activos» (propuesta por Claude y aceptada por mí) y el criterio C-01, que decía «cada Administrador activo» sin excluir al solicitante.

**Qué entregó el agente (con la evidencia):** Lo pedido (commit `4b18324`, PR #53): notificaba a todos los Administradores activos, incluido el solicitante. Evidencia en `9f7d5d2`: el Administrador crea su propia solicitud y en su bandeja aparece «Admin (admin@ejemplo.com) solicitó el permiso acceso-reportes.»; al aprobarla, HTTP 409 «No puedes resolver tu propia solicitud.».

**Por qué pasó:** Criterio incompleto: C-01 (redactado por Claude y aprobado por mí) no consideró que un Administrador también puede crear una solicitud. El agente cumplió lo que pedía el criterio.

**Corrección:** La hizo el agente con el prompt del bloque A3-B1: se excluye al solicitante; si es el único Administrador activo, no se notifica a nadie. Commit `fc26527` (PR #58). 3 pruebas nuevas `A3_H3`: sobre el código anterior fallaron 2 de 3 (la tercera confirma que C-01 sigue cumpliéndose con un solicitante Estándar); sobre el corregido pasaron las 3. Después: una nueva solicitud propia se crea y la bandeja del Administrador sigue con 1 notificación.

**Qué cambias:** C-01 ahora excluye al Administrador solicitante.

### H4

**Hallazgo:** H4 — «Un error de enlazado expone detalles internos y no usa el formato {mensaje}» — gravedad media.

**Qué se pidió:** En `AGENTS.md` y en los criterios, errores como `{"mensaje": "..."}` sin trazas (RD-08); en el prompt del bloque B2-B, `PUT /api/notificaciones/preferencias` con 400 `{"mensaje"}` si faltaba `correoActivo`.

**Qué entregó el agente (con la evidencia):** El controlador solo validaba el caso nulo (commit `0511dd8`, PR #53); un tipo incorrecto lo rechaza antes la respuesta automática de `[ApiController]`. Evidencia en `9f7d5d2`: `PUT` con `{"correoActivo":"true"}` → HTTP 400 con `ValidationProblemDetails` («One or more validation errors occurred.», «$.correoActivo», `traceId`) que expone el nombre interno «Tams.Api.Controladores.NotificacionesController+ActualizarPreferenciaRequest».

**Por qué pasó:** Instrucción incompleta: se pidió `{"mensaje"}` para el campo ausente, no para un tipo incorrecto; y revisión insuficiente: mi verificación manual de la Práctica 5 solo probó el cuerpo sin el campo.

**Corrección:** La hizo el agente con el prompt del bloque A3-B1: una única respuesta para los errores de enlazado de todos los controladores, que no copia nada del error original (`RespuestaErrorEnlazado`, registrada en `Program.cs`). Commit `79a6d1d` (PR #58). 2 pruebas nuevas `A3_H4`: sobre el código anterior no compilan, porque esa clase no existía; sobre el corregido pasan las 2. La prueba de comportamiento es la reproducción por HTTP: después, HTTP 400 con `{"mensaje":"La solicitud contiene datos inválidos."}`, sin «Tams.», «$.» ni `traceId`.

**Qué cambias:** El primer párrafo de los criterios incluye los errores de enlazado; `AGENTS.md` lo exige como convención.

**Qué cambias, general:** En el commit `3564516` (PR #59, merge `a6f18bf`), además, `AGENTS.md` exige que cada commit compile por sí solo, que el informe traiga el código o el diff completo, y que los nombres de las pruebas citen el requisito y no el criterio, por lo aprendido en la Práctica 5. Los criterios tienen una sección «Cambios tras la auditoría (Asignación 3)».

## 3. Cómo verificar cada corrección (comandos)

Para cada commit repetí este procedimiento, donde `Hn` es `H1`, `H2`, `H3` o `H4` según el hallazgo:

```powershell
git switch --detach <commit>
git checkout <commit>^ -- src
dotnet test TAMS.slnx --filter "FullyQualifiedName~A3_Hn"   # falla o no compila
git checkout <commit> -- src
dotnet test TAMS.slnx --filter "FullyQualifiedName~A3_Hn"   # pasa
git switch main
```

Los commits son `6c547d0` (H1), `4599bf6` (H2), `fc26527` (H3) y `79a6d1d` (H4).

Además, repetí por HTTP los comandos de cada hallazgo sobre `9f7d5d2` y sobre el tag `practica-5`:

```powershell
# H1
curl.exe -s -w "`nHTTP %{http_code}`n" "$base/api/reportes/solicitudes-permiso?desde=9999-12-31&hasta=9999-12-31" -H "Authorization: Bearer $tokAdmin"
curl.exe -s -w "`nHTTP %{http_code}`n" "$base/api/reportes/asistencia?desde=9999-12-31&hasta=9999-12-31" -H "Authorization: Bearer $tokAdmin"

# H2
curl.exe -s -w "`nHTTP %{http_code}`n" "$base/api/correos?estado=0" -H "Authorization: Bearer $tokAdmin"
curl.exe -s -w "`nHTTP %{http_code}`n" "$base/api/correos?estado=Pendiente,Enviado" -H "Authorization: Bearer $tokAdmin"
curl.exe -s -w "`nHTTP %{http_code}`n" "$base/api/correos?estado=Perdido" -H "Authorization: Bearer $tokAdmin"
curl.exe -s -w "`nHTTP %{http_code}`n" "$base/api/correos?estado=fallido" -H "Authorization: Bearer $tokAdmin"

# H3
Invoke-RestMethod -Method Post -Uri "$base/api/permisos" -Headers $admin -ContentType "application/json" -Body (@{ tipoPermiso = "acceso-reportes" } | ConvertTo-Json)
Invoke-RestMethod -Method Get -Uri "$base/api/notificaciones" -Headers $admin

# H4
$archivoPref = Join-Path $env:TEMP "preferencia.json"
[IO.File]::WriteAllText($archivoPref, '{"correoActivo":"true"}')
curl.exe -s -w "`nHTTP %{http_code}`n" -X PUT "$base/api/notificaciones/preferencias" -H "Authorization: Bearer $tokAdmin" -H "Content-Type: application/json" --data-binary "@$archivoPref"
```

Después de la corrección, H1 responde 200 con `[]`; `?estado=0` y `?estado=Pendiente,Enviado` responden 400 con el mensaje de estados válidos y `?estado=fallido` responde 200; la bandeja del Administrador sigue con 1 notificación; y el `PUT` responde 400 con `{"mensaje":"La solicitud contiene datos inválidos."}`.

## 4. Candidatos que no declaré como hallazgos, y por qué

- Fechas sin `Z` en las respuestas de permisos y documentos (RD-11): es código anterior a la Práctica 5; queda como deuda técnica.
- Posible carrera al aprobar dos solicitudes del mismo permiso y posible pérdida del conteo de intentos con dos procesadores a la vez: son hipótesis de concurrencia que no se pueden reproducir de forma fiable.
- Posible información sensible en el último error del SMTP: hipótesis sin reproducir.
- Rol numérico en el token: quedó incluido como parte de la causa de H2.

## 5. Tabla resumen

| Hallazgo | Pieza | Gravedad | Commit de la corrección | Pruebas nuevas | Cambio en el contexto o en los criterios |
|---|---|---|---|---|---|
| H1 | Reportes | media | `6c547d0` | 4 (`A3_H1`) | C-25 y `AGENTS.md` |
| H2 | Correos; Reportes y Documentos (rol del token) | baja | `4599bf6` | 19 (`A3_H2`) | C-17 y `AGENTS.md` |
| H3 | Permisos / Notificaciones | baja | `fc26527` | 3 (`A3_H3`) | C-01 |
| H4 | Notificaciones / API | media | `79a6d1d` | 2 (`A3_H4`) | Criterios (primer párrafo) y `AGENTS.md` |
