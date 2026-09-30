# Bitácora — Fase 0, crítica del diagrama de componentes

- **Agente:** OpenCode, en VS Code.
- **Tarea:** revisar el diagrama de componentes Mermaid contra las seis
  señales de diseño mal hecho de la diapositiva 11 y las reglas de
  notación C4 nivel 3, sin modificar archivos.

## Qué le pedí

Que evaluara el diagrama señal por señal (caja Utils/Común, flecha en dos
sentidos, nombre con dos verbos, flecha del Core hacia TAMS, flecha sin
etiqueta, base de datos como componente), más cualquier otro problema que
viera, indicando su nivel de confianza en cada uno.

## Qué me devolvió

Una tabla con el resultado de las seis señales, y una lista de 10 problemas
adicionales ordenados por prioridad. Acertó en detectar que 24 flechas no
tenían etiqueta, que la base de datos y el volumen estaban dibujados como
componentes, y encontró un ciclo real entre Control de acceso y Auditoría
que yo no había visto.

## Dónde se equivocó

Marcó con confianza "Alta" que el diagrama violaba las reglas de C4 por
mostrar el contenedor de TAMS y el del Core en un mismo diagrama. Eso
contradice el material del curso: la diapositiva 7 ("Siete piezas, un solo
contenedor") muestra exactamente ese patrón como el correcto para este
proyecto, porque el Core y el módulo de negocio son un solo contenedor con
distintas piezas, no dos contenedores separados.

## Cómo lo corregí

Comparé la observación del agente contra la diapositiva 7 del material de
la clase y descarté ese punto. Mantuve el resto de sus hallazgos válidos
(flechas sin etiqueta, base de datos como componente, el ciclo
Control de acceso↔Auditoría) y los apliqué al rehacer el diagrama.