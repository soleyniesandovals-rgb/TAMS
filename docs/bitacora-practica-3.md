# Bitácora de la Práctica 3

**Estudiante:** Soleyni Sandoval
**Práctica:** 3 — Capa HTTP, pruebas de la máquina de estados y pruebas de integración del módulo de negocio

## 1. Resumen de la práctica

La Práctica 3 consistió en llevar el módulo de negocio de TAMS desde el dominio hasta la API, y en demostrar su comportamiento con pruebas automatizadas. El trabajo se dividió en siete bloques: una auditoría de solo lectura del punto de partida (B0), el diseño de la máquina de estados en `docs/maquina-de-estados.md` (B1), la capa de aplicación `CalificacionesServicio` con sus siete operaciones (B2), la capa HTTP `CalificacionesController` con su registro en `Program.cs` y el script `docs/datos-demo.sql` (B3), el proyecto `Tests.Negocio` con 40 pruebas de la máquina de estados (B4), el proyecto `Tests.Integracion` con 4 pruebas sobre los servicios reales y persistencia EF InMemory (B5) y la documentación final en el `README.md` (B6).

Al cierre, la suite completa quedó en **70 pruebas repartidas en cinco proyectos**, todas correctas y sin errores.

## 2. Cronología por bloques con su PR

| Bloque | Qué se hizo | PR |
|---|---|---|
| B0 | Auditoría de solo lectura del punto de partida: revisar qué existía y qué faltaba antes de escribir código. | Sin PR |
| B1 | Diseño de la máquina de estados en `docs/maquina-de-estados.md`. | #33 |
| B2 | Capa de aplicación `CalificacionesServicio` con las siete operaciones. | #34 |
| B3 | `CalificacionesController`, registro en `Program.cs` y `docs/datos-demo.sql`. | #35 |
| B4 | Proyecto `Tests.Negocio` con 40 pruebas de la máquina de estados. | #36 |
| B5 | Proyecto `Tests.Integracion` con 4 pruebas que usan `GestionPermisosServicio` y `CalificacionesServicio` reales, con persistencia EF InMemory. | #38 |
| B6 | `README.md` con la sección de defectos de la Práctica 3 y la guía para provocar cada funcionalidad. | #39 |

Los bloques se hicieron en ese orden. B0 no produjo cambios en el repositorio: solo sirvió para saber con qué contaba. B1 fijó el contrato de estados y transiciones antes de implementarlo. B2 y B3 separaron la lógica de aplicación de la capa HTTP. B4 y B5 agregaron las pruebas. B6 documentó el resultado.

## 3. Qué le pedí al agente en cada bloque

- **B0:** que auditara el estado del repositorio en modo de solo lectura, sin modificar ni crear archivos, y que me dijera qué partes del módulo de negocio ya existían y cuáles había que construir.
- **B1:** que escribiera `docs/maquina-de-estados.md` con los estados, las transiciones permitidas, las prohibidas, las condiciones y los nombres de las pruebas que las cubrirían.
- **B2:** que implementara `CalificacionesServicio` con las siete operaciones del negocio, respetando la máquina de estados y las reglas de puntaje.
- **B3:** que expusiera las siete operaciones en `CalificacionesController`, que registrara lo necesario en `Program.cs` y que preparara `docs/datos-demo.sql` para tener datos de demostración.
- **B4:** que creara el proyecto `Tests.Negocio` con 40 pruebas unitarias de la máquina de estados, sin dependencias de infraestructura.
- **B5:** que creara el proyecto `Tests.Integracion` con 4 pruebas que usaran `GestionPermisosServicio` y `CalificacionesServicio` reales, y que solo la persistencia fuera EF InMemory.
- **B6:** que actualizara el `README.md` con la sección de defectos de la Práctica 3 y con la guía para provocar cada funcionalidad, sin tocar las secciones de la Práctica 2.

## 4. Errores del agente, hallazgos y cómo los resolví

- **(a) Hallazgo: el defecto 3 no se podía plantar todavía.** En la auditoría B0 el agente señaló correctamente que el defecto 3, sobre la primera transición con condición (`Borrador → Enviada`, rango de 0 a 100), no se podía plantar todavía porque esa condición no existía en el código. Decidí implementarla en una sola línea de `CumpleCondicion`, en el PR #34, para que el defecto se pudiera plantar y la suite pudiera detectarlo.
- **(b) Cerrar sin cuerpo daba 400.** En el controlador, el cuerpo de la operación cerrar era obligatorio, así que cerrar directo sin cuerpo devolvía 400. Claude, el asistente con el que revisé cada informe del agente antes de aceptarlo, señaló que ese cuerpo obligatorio era la causa del 400 al cerrar directo sin cuerpo; lo comprobé y mandé la corrección al agente, que aplicó `EmptyBodyBehavior.Allow` en el PR #35.
- **(c) Error de sintaxis en el script demo.** `docs/datos-demo.sql` falló en SSMS con `Incorrect syntax near 'THROW'` porque faltaba un punto y coma antes del `THROW`. Se corrigió con `END;` en el commit `cf08232`.
- **(d) Riesgo de credenciales en el historial.** El agente creó el PR #35 por la API de GitHub con las credenciales guardadas en git. Verifiqué con `git log --all -S"ghp_"` y `git log --all -S"github_pat_"` que ningún token entró al historial; la salida fue vacía en ambos casos.
- **(e) Ejemplos del README que no reproducían el código esperado.** En el README, el ejemplo de corregir puntaje reutilizaba una calificación ya cerrada, lo que daba 409 en vez del 403 que prometía, y mostraba un texto de correo que no existe en el script. Claude, el asistente con el que revisé cada informe del agente antes de aceptarlo, señaló ambos problemas; los comprobé y mandé las correcciones al agente, que las aplicó en los commits `fab81a6` y `c0e3f86`.
- **(f) Bloque detenido por una contradicción del prompt.** El agente se detuvo al empezar el bloque B4 porque estaba en `main`; era una contradicción del prompt, que decía detenerse en `main` y luego pedía cambiar a `main`. Se corrigió cambiando la regla a "no hagas commits en `main`".
- **(g) PR #37 abierto por error.** Abrí por error el PR #37 (un Revert del PR #36). Lo cerré sin mezclar y verifiqué con `Test-Path` y `dotnet test` que `main` conservaba las 40 pruebas.

Además de esos errores, hice estas verificaciones:

- Conté con `Select-String` que los 14 métodos de `Tests.Negocio` tienen las tres fases (14, 14, 14, 14).
- Busqué `Falso|Fake|Mock|Stub|Substitute|Moq` en `tests/Tests.Integracion` y la búsqueda salió vacía.
- Hice un clon limpio con una base nueva `TamsP3`, siguiendo el `README.md`, y probé los cinco frentes más las 7 operaciones del negocio.

## 5. Decisiones que tomé

- El servicio usa `TamsDbContext` directamente, sin repositorios, por falta de tiempo.
- La integración usa los servicios reales y solo la persistencia es EF InMemory.
- La operación que depende de un permiso es corregir el puntaje de una calificación publicada, con el permiso `corregir-calificacion`.
- El permiso es global y no se consume: una vez aprobado, sirve para volver a corregir otras calificaciones publicadas.

## 6. Fuentes consultadas

- Enunciado de la Práctica 3 (aula virtual).
- Clase del 28 de octubre.
- `docs/maquina-de-estados.md`.
- `README.md` del repositorio.
- Informes del agente.
