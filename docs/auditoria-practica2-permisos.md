# Auditoría — Práctica 2: Gestión de permisos (RF-GP-01 a RF-GP-07)

- **Rama:** `test/practica2-auditoria` (creada desde `main`, commit `5269857`).
- **Alcance:** auditoría de cobertura y de capacidad de detección de la suite actual.
- **Restricción:** no se modificó código de producción. Las mutaciones se aplicaron sobre
  una **copia temporal** del repositorio (`%TEMP%\opencode\tams-mutacion`), verificada por
  hash tras cada restauración. El árbol de trabajo de este repositorio quedó intacto.
- **Fecha:** 21 de octubre de 2026.

---

## 1. Ejecución de la suite

Comando del README (`README.md:85`):

```powershell
dotnet test TAMS.slnx
```

Resultado: **17/17 en verde, 0 fallos, 0 omitidos.**

| Proyecto | Sup./Total | Duración |
|---|---|---|
| `Tests.Permisos` | 11/11 | 420 ms |
| `Tests.Core` | 5/5 | 845 ms |
| `Tests.Arquitectura` | 1/1 | 1 s |

Observación: el build emite las advertencias **preexistentes** `NU1902`/`NU1903`
(`System.Security.Cryptography.Pkcs` 10.0.0) en los 6 proyectos. No son un efecto de esta
auditoría, pero conviene documentarlas o actualizar el paquete.

---

## 2. Matriz RF-GP-01 a RF-GP-07 contra la suite existente

Todas las pruebas viven en `tests/Tests.Permisos/GestionPermisosServicioTests.cs` (11 pruebas,
todas a nivel de **servicio**; ninguna a nivel HTTP).

| RF | Prueba | Líneas | Qué verifica | ID en el nombre | ID en comentario | Estado |
|---|---|---|---|---|---|---|
| RF-GP-01 | `CrearAsync_NaceEnEstadoPendiente` | L24-39 | La solicitud nace `Pendiente`, con solicitante y tipo correctos, y `ResueltaPorId`/`FechaResolucion`/`MotivoRechazo` en null | No | No (sin comentario) | Parcial |
| RF-GP-01/05 | `AprobarAsync_TransicionaAAplicadaYRegistraQuienYCuando` | L41-58 | Al aprobar, el estado final es `Aplicada` y se registra quién resolvió y cuándo | No | No | Parcial |
| RF-GP-05 | `AprobarAsync_CreaPermisoActivo` | L60-79 | La aprobación inserta el `PermisoActivo` con el usuario, tipo y solicitud correctos | No | No | Parcial |
| RF-GP-06 | `AprobarAsync_SegundaVezLanzaYEstadoNoCambia` | L81-102 | Aprobar dos veces lanza `TransicionNoPermitidaExcepcion` y no altera el estado | No | No | Parcial |
| RF-GP-04 | `RechazarAsync_SinMotivoLanzaReglaNegocioExcepcion` | L104-118 | Rechazar sin motivo lanza `ReglaNegocioExcepcion` | No | No | Parcial |
| RF-GP-04 | `RechazarAsync_ConMotivoTransicionaARechazada` | L120-137 | Con motivo válido, el estado pasa a `Rechazada` y se guarda `MotivoRechazo` | No | No | Parcial |
| RF-GP-07 | `AprobarAsync_PropiaSolicitudLanzaReglaNegocioExcepcion` | L139-152 | Un administrador no puede aprobar su propia solicitud | No | No | Parcial |
| RF-GP-02 | `ListarPropiasAsync_SoloDevuelveLasDelUsuario` | L154-173 | Cada usuario ve únicamente sus propias solicitudes | No | No | Parcial |
| **RF-GP-03** | `ListarPendientesAsync_SoloDevuelveLasPendientes` | L175-195 | De 3 solicitudes (1 pendiente, 1 aprobada, 1 rechazada), solo vuelve la pendiente | No | **Sí, pero dice "RF-GP-02"** → etiqueta **incorrecta** | Parcial (punto ciego, ver §3 D1) |
| RF-GP-04/06 | `RechazarAsync_SobreSolicitudYaAplicadaLanzaTransicionNoPermitida` | L197-215 | Una solicitud ya `Aplicada` no se puede rechazar | No | Sí (correcto) | Parcial |
| RF-GP-07 | `RechazarAsync_PropiaSolicitudLanzaReglaNegocioExcepcion` | L217-229 | Un administrador no puede rechazar su propia solicitud | No | Sí (correcto) | Parcial |

### 2.1 Trazabilidad de los IDs

- **8 de 11 pruebas no mencionan ningún ID de RF** ni en el nombre ni en un comentario.
- Solo 3 pruebas citan un RF, y **una de ellas está mal etiquetada**: el comentario de
  `ListarPendientesAsync_SoloDevuelveLasPendientes` dice `RF-GP-02`, pero la prueba exercise
  la bandeja de pendientes del administrador, que es **RF-GP-03**.
- Ninguna prueba cita `RF-GP-01` ni `RF-GP-05` de forma explícita, aunque las cubra de hecho.

### 2.2 Huecos por RF

| RF | Hueco detectado |
|---|---|
| RF-GP-01 | Sin pruebas de la validación de entrada: `tipoPermiso` vacío/en blanco y longitud > 100 (`GestionPermisosServicio.cs:30-40`). Sin verificación del 201 HTTP. |
| RF-GP-02 | No se prueba que un **Administrador** que consulta `GET /api/permisos` también ve solo las propias (aclaración del README L159). Sin prueba HTTP. |
| RF-GP-03 | **Punto ciego crítico**: la prueba usa el doble `SolicitudesPermisoRepositorioFalso`, que **ya filtra por `Pendiente`** (L38). El filtro del servicio y el del repositorio real nunca se ejercitan de forma independiente. Sin prueba del 403 a un Estandar (`[RequiereRol]`, `PermisosController.cs:66`). El query EF real (`SolicitudesPermisoRepositorio.cs:26`) no tiene ninguna prueba. |
| RF-GP-04 | No se comprueba que el motivo se guarde **recortado**, ni que el rechazo registre `ResueltaPorId`/`FechaResolucion`, ni el 400 HTTP. |
| RF-GP-05 | No se prueba el antes/después a través de `TienePermisoAsync` (solo se mira el repositorio). **La rama "ya existe el permiso, no se duplica" (`GestionPermisosServicio.cs:101`) nunca se ejecuta en verde.** |
| RF-GP-06 | **Falta el caso 3 del README (L175):** rechazar una solicitud ya `Rechazada` debe dar 409. No hay ninguna prueba de `RechazarAsync` sobre una solicitud rechazada. No hay pruebas unitarias de la clase `TransicionesPermiso` (`PermitidosDesde`, `EsValida`, `EsTerminal`). El mensaje del README (L174) no coincide con el real: la excepción dice `"No se permite pasar de Aplicada a Aprobada. Motivo: la transición no está habilitada en la máquina de estados."`, y ninguna prueba asserta el mensaje. |
| RF-GP-07 | Solo se asserta el **tipo** de excepción: ni el 409 ni el mensaje `"No puedes resolver tu propia solicitud."`, ni que el chequeo ocurra **antes** de validar la transición (README L179), que es justo lo que lo hace inequívoco. |

### 2.3 Hueco estructural transversal

**No existe ni una sola prueba a nivel HTTP.** Los tres proyectos de prueba usan únicamente
xUnit, sin `Microsoft.AspNetCore.Mvc.Testing` / `WebApplicationFactory`. En consecuencia,
**ningún código HTTP descrito en el README (201, 400, 403, 404, 409, 401) está automatizado**:
todo eso hoy solo se puede provocar a mano con `Invoke-WebRequest`, tal como documenta el
propio README. Los casos de rol (403 de `Estandar`) y el mapeo de excepciones a códigos HTTP
(`PermisosController.cs:97-105` y `132-139`) no tienen ninguna red de seguridad.

---

## 3. Los 6 defectos: dónde se plantan y si la suite los detecta

Método: se aplicó cada defecto sobre la copia temporal, se ejecutó `dotnet test` sobre
`Tests.Permisos` y se verificó por hash que el archivo original quedaba restaurado.

| # | Defecto | Punto de inyección exacto | ¿Lo detecta la suite hoy? | Evidencia |
|---|---|---|---|---|
| 1 | Filtro de pendientes: "estado es Pendiente" → "no es Rechazada" | `src/Core/Permisos/Application/Servicios/GestionPermisosServicio.cs:72` (`.Where(s => s.Estado == EstadoSolicitudPermiso.Pendiente)`) y, en la otra capa, `src/Core/Permisos/Infrastructura/Repositorios/SolicitudesPermisoRepositorio.cs:26` | **NO** (en ninguno de los dos puntos) | Suite en verde con el defectoplantado. Causa: el doble `SolicitudesPermisoRepositorioFalso.cs:38` ya devuelve solo `Pendiente`, así que el predicado del servicio se aplica sobre un conjunto ya filtrado → la aserción `Assert.Single` es tautológica. Prueba: haciendo el doble "fiel" (que devuelve todo) el defecto **sí** se detecta en `ListarPendientesAsync_SoloDevuelveLasPendientes`, y en verde sin defecto. |
| 2 | Agregar transición `Aplicada → Aprobada` | `src/Core/Permisos/Domain/Maquinas/TransicionesPermiso.cs:38` (`[EstadoSolicitudPermiso.Aplicada] = [],`) | **SÍ** | 1 fallo: `AprobarAsync_SegundaVezLanzaYEstadoNoCambia` (la segunda aprobación ya no lanza). |
| 3 | Quitar la llamada que aplica el permiso al aprobar | `src/Core/Permisos/Application/Servicios/GestionPermisosServicio.cs:101-111` (bloque `if (!await unidad.PermisosActivos.ExistePermisoAsync(...)) { ... AgregarAsync(new PermisoActivo {...}) }`) — variante alterna: L98-99 (`Exigir(..., Aplicada)` + `solicitud.Estado = Aplicada`) | **SÍ** en ambas variantes | 3a → 1 fallo: `AprobarAsync_CreaPermisoActivo` (colección vacía). 3b → 3 fallos: `AprobarAsync_TransicionaAAplicadaYRegistraQuienYCuando`, `AprobarAsync_SegundaVezLanzaYEstadoNoCambia`, `RechazarAsync_SobreSolicitudYaAplicadaLanzaTransicionNoPermitida`. |
| 4 | En rechazar, "estado es Pendiente" → "estado no es Aplicada" | `src/Core/Permisos/Application/Servicios/GestionPermisosServicio.cs:137` (`TransicionesPermiso.Exigir(solicitud.Estado, EstadoSolicitudPermiso.Rechazada);`) — equivalentes en la máquina: `TransicionesPermiso.cs:25-29` (`Pendiente → Rechazada`), `:32` (`Aprobada → Aplicada`) y `:35` (`Rechazada = []`) | **NO** | 3 variantes probadas y las 3 pasan inadvertidas: (a) precondición `if (Estado == Aplicada) throw`; (b) `Rechazada → Rechazada` habilitada; (c) `Aprobada → Rechazada` habilitada. Causa: no existe ninguna prueba que rechace una solicitud **ya rechazada**, que es justo el caso 3 de RF-GP-06 del README (L175). |
| 5 | Estado inicial `Pendiente → Aprobada` | `src/Core/Permisos/Application/Servicios/GestionPermisosServicio.cs:47` (`Estado = EstadoSolicitudPermiso.Pendiente,`) — también `src/Core/Permisos/Domain/Entidades/SolicitudPermiso.cs:23` (inicializador por defecto, sobreescrito por el servicio) | **SÍ** | 7 fallos, encabezados por `CrearAsync_NaceEnEstadoPendiente`, `AprobarAsync_CreaPermisoActivo`, `AprobarAsync_TransicionaAAplicadaYRegistraQuienYCuando`, `ListarPendientesAsync_SoloDevuelveLasPendientes`, `RechazarAsync_SobreSolicitudYaAplicadaLanzaTransicionNoPermitida`. Nota: plantar el defecto **solo** en `SolicitudPermiso.cs:23` tampoco lo detecta nadie, porque el servicio sobrescribe el estado. |
| 6 | Quitar la verificación de auto-resolución **solo en rechazar** | `src/Core/Permisos/Application/Servicios/GestionPermisosServicio.cs:128` (`ExigirNoAutoResolucion(solicitud, administradorId);` dentro de `RechazarAsync`) | **SÍ** | 1 fallo: `RechazarAsync_PropiaSolicitudLanzaReglaNegocioExcepcion` (el rechazo propio se resuelve sin error). |

### 3.1 Defecto espejo (comprobado por simetría)

| # | Defecto | Punto de inyección | ¿Detectado? | Evidencia |
|---|---|---|---|---|
| 7 | Quitar la verificación de auto-resolución **solo en aprobar** | `src/Core/Permisos/Application/Servicios/GestionPermisosServicio.cs:87` | **SÍ** | 1 fallo: `AprobarAsync_PropiaSolicitudLanzaReglaNegocioExcepcion`. Confirma que las dos ramas de la regla están cubiertas por pruebas distintas. |

### 3.2 Resumen de capacidad de detección

- Detectados: **4 de 6** (D2, D3, D5, D6) más el espejo D7.
- **No detectados: 2 de 6** — D1 (filtro de pendientes) y D4 (precondición de rechazo).
- La ceguera de D1 es **estructural**: no es que falte una aserción, es que la prueba existente
  no puede fallar por construcción (doble filtrado).
- La ceguera de D4 es una **laguna directa**: falta el caso de rechazo sobre una solicitud ya
  rechazada, que el README exige en RF-GP-06.

---

## 4. ¿La verificación de auto-resolución está compartida entre aprobar y rechazar?

**Sí, en el servicio: es una única función auxiliar privada compartida por las dos rutas.**

- Helper único: `src/Core/Permisos/Application/Servicios/GestionPermisosServicio.cs:160-166`
  ```csharp
  private static void ExigirNoAutoResolucion(SolicitudPermiso solicitud, int administradorId)
  {
      if (solicitud.SolicitanteId == administradorId)
      {
          throw new ReglaNegocioExcepcion("No puedes resolver tu propia solicitud.", 409);
      }
  }
  ```
- Llamadas: `AprobarAsync` en **L87** y `RechazarAsync` en **L128**. Ambos lanzan el mismo
  `ReglaNegocioExcepcion` con código HTTP 409.

Matices que importan:

1. **La lógica es compartida; la invocación no.** Son dos llamadas distintas en dos métodos
   distintos. Por eso el defecto 6 (borrar solo la de rechazar) rompe solo el rechazo, y el
   espejo (borrar solo la de aprobar) rompe solo la aprobación: comprobado empíricamente.
   Si se quisiera que el defecto fuera imposible, el chequeo debería vivir en un único
   punto de entrada (p. ej. un `ResolverAsync` único parametrizado por el estado destino).
2. **El orden importa y está implícito.** En `AprobarAsync` el chequeo (L87) va **antes** de
   `TransicionesPermiso.Exigir` (L91) a propósito, para que el mensaje sea inequívoco
   (README L179). En `RechazarAsync` el chequeo (L128) va **antes** de la validación del
   motivo (L131), lo que produce un 409 "No puedes resolver tu propia solicitud." en lugar
   del 400 "El rechazo requiere un motivo." cuando el administrador rechaza su propia
   solicitud sin motivo. Ninguna prueba fija ese orden.
3. **Lo que no está verificado.** Las pruebas solo comprueban el *tipo* de excepción. No
   verifican el código 409, ni el texto `"No puedes resolver tu propia solicitud."`, ni que
   el 409 se traduzca en la respuesta HTTP (`PermisosController.cs:97-100` y `132-135`).
4. **El alcance de la regla.** Compara `SolicitudPermiso.SolicitanteId` contra el id del
   administrador que resuelve, lo que solo tiene sentido porque el rol lo impone
   `[RequiereRol(RolUsuario.Administrador)]` en el controlador
   (`PermisosController.cs:78` y `:113`). Si el rol llegara a cambiar, el mensaje "409" por
   auto-resolución se solaparía con el de la transición no permitida.

---

## 5. Recomendaciones (pendientes de implementar; no forman parte de esta auditoría)

Ordenadas por relación entre hueco y riesgo:

1. **Cerrar la ceguera de D1** (crítica): hacer que `SolicitudesPermisoRepositorioFalso` sea
   un almacén fiel (devuelve lo que hay) y que el filtro viva en un solo sitio, o duplicar el
   filtro en el doble para poder distinguir qué capa se rompió. Añadir, además, una prueba del
   query real (`SolicitudesPermisoRepositorio.cs:23-28`) con SQL Server.
2. **Añadir el caso 3 de RF-GP-06**: rechazar una solicitud ya `Rechazada` → 409
   `TransicionNoPermitidaExcepcion` (cierra la ceguera de D4).
3. **Etiquetar las 11 pruebas** con su ID de RF (nombre o comentario) y corregir el
   `RF-GP-02` erróneo de `ListarPendientesAsync_SoloDevuelveLasPendientes` → `RF-GP-03`.
4. **Assertar códigos y mensajes**: 409 + `"No puedes resolver tu propia solicitud."`,
   400 + `"El rechazo requiere un motivo."`, y el texto real de `TransicionNoPermitidaExcepcion`
   (el README debe ajustarse al mensaje real, no al revés).
5. **Cubrir la rama "no duplicar permiso"** (`GestionPermisosServicio.cs:101`) aprobando dos
   solicitudes del mismo tipo para el mismo usuario.
6. **Pruebas a nivel HTTP** con `WebApplicationFactory` para 201/400/403/404/409 y para el
   403 del `Estandar` en `GET /api/permisos/pendientes`.
7. **Pruebas unitarias de `TransicionesPermiso`** (`PermitidosDesde`, `EsValida`, `EsTerminal`)
   para blindar el mapa de transiciones.
8. **Pruebas de validación de entrada** de `CrearAsync` (vacío, longitud > 100).
9. Corregir la estructura del README (L184-198): no menciona `tests/Tests.Core`, que sí existe
   y se ejecuta.

---

## 6. Reproducibilidad

```powershell
git switch -c test/practica2-auditoria main
dotnet test TAMS.slnx          # 17/17 en verde
```

Las mutaciones se aplicaron sobre una copia temporal del repositorio, con verificación por
hash SHA-256 de los tres archivos mutados tras cada restauración
(`GestionPermisosServicio.cs`, `TransicionesPermiso.cs`, `SolicitudesPermisoRepositorio.cs`).
El repositorio de trabajo no contiene ninguna mutación.
