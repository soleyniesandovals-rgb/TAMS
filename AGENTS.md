# Contexto para el agente — TAMS

## Propósito
TAMS es un sistema de gestión académica para docentes (Programación III, ITLA). Tiene dos partes:
- **Core** (`src/Core`): especificación fija del curso. Piezas: ControlAcceso, Permisos, Documentos, Notificaciones, Reportes y Auditoria.
- **Módulo de negocio** (`src/Tams.Negocio`): años escolares, cursos, materias con resultados de aprendizaje, estudiantes, asignaciones docentes, calificaciones (máquina de estados) y asistencia.
- **API** (`src/Tams.Api`): punto de entrada único; controladores delgados.

## Lenguaje y framework
C# con .NET 10, ASP.NET Core Web API, Entity Framework Core (SQL Server), xUnit, NetArchTest. Autenticación con JWT.

## Cómo correrlo
- Contenedor (forma recomendada): `Copy-Item .env.example .env` y `docker compose up -d --build`. API en http://localhost:5132.
- Local: `cd src\Tams.Api` y `dotnet run` (las variables de entorno las tiene la dueña del repositorio; tú no las necesitas).

## Cómo probarlo
`dotnet test TAMS.slnx` desde la raíz. Todas las pruebas deben pasar antes de entregar. Las pruebas no levantan la aplicación ni usan SQL Server (RD-12).

## Convenciones de código
- Capas Domain / Application / Infrastructura / API. La lógica va en servicios de Application (RD-02); los controladores solo reciben, llaman al servicio y traducen el resultado a HTTP.
- **El Core nunca depende de `Tams.Negocio` (RD-03).** El negocio usa al Core solo a través de interfaces del Core. Hay una prueba de arquitectura que lo comprueba.
- Cada pieza del Core guarda sus propios datos (RD-01): no se agregan campos de una pieza en la entidad de otra.
- Autorización en el servidor con `[Authorize]` y `[RequiereRol(...)]` (RD-06). `[RequiereRol]` admite un solo rol por atributo.
- Errores como `{ "mensaje": "..." }`, sin trazas, rutas ni consultas (RD-08). Toda entrada se valida y una entrada inválida da 400 controlado (RD-07).
- Fechas: «ahora» siempre con el `TimeProvider` inyectado, en UTC. Las fechas y horas de las respuestas nuevas se devuelven en UTC con `Z` (RD-11).
- Pruebas: nombre que dice el comportamiento e incluye el requisito (por ejemplo `RF_NOT_05_MarcarLeida_DeOtroUsuario_Rechaza`), tres fases comentadas (preparar, actuar, verificar), independientes entre sí, con dobles en memoria o EF Core InMemory con un nombre de base único por prueba.
- Control de versiones: nunca commits en `main`; una rama por bloque; commits atómicos con asunto en imperativo y el ID del requisito (RF-NOT-xx, RF-REP-xx, RD-xx); PR con las secciones Qué cambia / Por qué / Cómo probarlo / Qué NO incluye.

## Restricciones
- **Nunca leas el archivo `.env` ni escribas credenciales, claves o cadenas de conexión** en ningún archivo, commit, mensaje o informe (RD-10). Para saber si una variable existe, usa solo su longitud.
- **No crees migraciones.** Cuando un cambio de entidad necesite una migración, haz el commit de la entidad y la configuración y DETENTE: la migración la crea la dueña del repositorio.
- No modifiques migraciones existentes, pruebas existentes, `Dockerfile`, `docker-compose.yml` ni `.env.example`, salvo que el prompt lo pida expresamente.
- No cambies el comportamiento de lo que ya funciona (los seis frentes del README): registro con activación, sesión, rechazo por rol, recuperación de contraseña, Gestión de permisos, negocio (operación válida y transición prohibida) y documentos.
- **No escribas archivos de texto con PowerShell** (`Set-Content`, `Out-File`, `echo`, redirecciones `>`): usa tu herramienta de edición y guarda en UTF-8 sin BOM.
- Si algo es ambiguo o falla de forma que no puedas resolver, DETENTE y explica el problema; no improvises.
- En tu informe final entrega las salidas reales de los comandos. No digas que algo funciona contra la base real si no lo ejecutaste.

## Criterios de aceptación de la Práctica 5
Están en `docs/criterios-notificaciones-reportes.md`. Tu trabajo se acepta solo si cumple cada criterio.