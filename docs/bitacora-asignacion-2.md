# Bitácora — Asignación 2 (Gestión de Permisos)

- **Agente:** OpenCode, en VS Code.
- **Sesión:** octubre de 2026.
- **Alcance delegado:** Bloque 1 (dominio, persistencia, DbSets en CoreDbContext), Bloque 2 (servicio, DI, controlador) y Bloque 3 (pruebas unitarias), además de documentación (README y bitácora).

## El caso más importante: sesión en carpeta incorrecta (OpenCode)

### Qué se le pidió (Bloque 1)
Construir el Bloque 1 de Permisos (entidades `SolicitudPermiso` y `PermisoActivo`, enum de estados, máquina de transiciones `TransicionesPermiso`, configuraciones de Entity Framework, y agregar los DbSet correspondientes a `CoreDbContext`) en una rama nueva `feat/asignacion2-permisos-dominio`.

### Qué devolvió el agente (reporte incorrecto)
Un reporte completo y convincente afirmando que había:
- creado la rama `feat/asignacion2-permisos-dominio`,
- escrito los 6 archivos nuevos,
- hecho 3 commits con los mensajes correctos,
- que `dotnet build` había pasado en **0 advertencias y 0 errores**, y
- que intentó hacer push pero falló por falta de credenciales de Git en ese entorno.

### El error
**Nada de eso era real.** 

- En VS Code, la pestaña de Control de Código Fuente no mostraba la rama nueva.
- Al comprobar en la terminal real del proyecto, `git branch --show-current` mostraba `main`.
- Al hacer una búsqueda recursiva del archivo `TransicionesPermiso.cs` en todo el disco de la usuaria (`Get-ChildItem -Recurse -Filter TransicionesPermiso.cs -ErrorAction SilentlyContinue`), **no apareció en ningún lado**.
- La causa fue que OpenCode estaba trabajando dentro de una conversación anterior (de la época de Fase 0, sobre las entidades de `Tams.Negocio`) que tenía registrada **otra carpeta de proyecto distinta a la real**. Todo el trabajo reportado —código, commits, build— ocurrió contra un estado virtual de esa conversación, no contra el repositorio real ubicado en `C:\Users\soley\OneDrive\Desktop\ITLA\PIII\TAMS`.

### Cómo se detectó
- Revisión de la pestaña de Control de Código Fuente de VS Code (la rama nueva no aparecía).
- Confirmación con `git branch --show-current` en la terminal real (mostraba `main`).
- Búsqueda recursiva en todo el disco (`Get-ChildItem -Recurse -Filter TransicionesPermiso.cs`) que no encontró ninguna coincidencia con el archivo que el agente afirmaba haber creado.

### Cómo se corrigió
- Se abrió una **conversación nueva** en OpenCode, seleccionando explícitamente la carpeta correcta del proyecto (`C:\Users\soley\OneDrive\Desktop\ITLA\PIII\TAMS`).
- Antes de repetir el prompt grande, se verificó con una prueba simple (pedirle que leyera el `README.md` real) para confirmar que estaba operando sobre la carpeta correcta.
- Con esa verificación hecha, se repitió el prompt del Bloque 1 y, esa segunda vez, el agente creó, commiteó y empujó correctamente la rama, dando lugar al **PR #20**.

## Resultado de los 3 bloques

- Bloque 1 (dominio/persistencia): `src/Core/Permisos` + DbSet en `CoreDbContext` → **PR #20** (mezclado).
- Bloque 2 (servicio/endpoints): `GestionPermisosServicio`, extensiones de unidad de trabajo, `AddCorePermisos` + `PermisosController` → **PR #23** (mezclado).
- Bloque 3 (pruebas unitarias): `tests/Tests.Permisos` con 8 pruebas usando dobles en memoria → **PR #24** (mezclado). `dotnet test TAMS.slnx` ejecuta **9/9** pruebas en verde.

## Limitaciones conocidas

- **Permiso genérico.** El permiso que se demuestra en esta asignación es genérico (una lista de "permisos activos" por `(UsuarioId, TipoPermiso)` y trazabilidad a `SolicitudPermiso`). No se liga a una acción específica del negocio de TAMS; es suficiente para demostrar RF-GP-01 a RF-GP-07.
- **RD-09 (sobrevivir a reiniciar la aplicación).** RD-09 se verifica por estar respaldado en SQL Server (persistencia EF, migraciones aplicadas). No hay una prueba automatizada que reinicie el proceso; la verificación fue por diseño (datos persisten en la base).
- **Condiciones de carrera (Tokens/Códigos).** Como se documentó en la bitácora de la Práctica 1, los códigos/tokens de un solo uso se verifican con lectura+escritura (no actualización condicional atómica) — punto a mejorar en una iteración futura.

## Lecciones aprendidas

- **Nunca asumir que el agente "está en el proyecto correcto".** Verificar explícitamente la ruta de trabajo antes de ejecutar tareas grandes.
- **Probar con una acción mínima** (leer `README.md`, `git branch --show-current`) es más barato y mucho más eficaz que detectar "código fantasma" después de un reporte exagerado.
- **Doble verificación**: pestaña SCM + terminal + búsqueda recursiva fueron decisivas para detectar la discrepancia.
- **Principio de mínima sorpresa:** una nueva conversación con la carpeta correcta elimina la ambigüedad heredada de conversaciones pasadas.
