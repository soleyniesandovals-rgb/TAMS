# Bitácora — Práctica 1 (Control de acceso)

- **Agente:** OpenCode, en VS Code.
- **Sesión:** 1 al 4 de octubre de 2026.
- **Alcance delegado:** registro y activación, sesión, roles y
  administración, contraseñas, estructura de la máquina de estados, y
  auditoría final de todos los bloques contra la rúbrica.

## El error más importante: RF-CA-13 (restablecimiento forzado)

### Qué le pedí
Implementar el restablecimiento de contraseña forzado por un
Administrador (RF-CA-13): el Administrador fuerza el restablecimiento
de otro usuario, que recibe por correo un código para definir una
contraseña nueva.

### Qué me devolvió
Un diseño que emite un código de recuperación de un solo uso y lo
encola por correo, sin tocar la contraseña actual. El agente mismo
documentó la decisión en una tabla comparando este enfoque contra
generar una contraseña temporal, explicando las ventajas de seguridad
de no exponerle la contraseña al Administrador.

### Dónde se equivocó
El criterio de aceptación exacto de la Práctica 1 dice: *"El usuario no
puede seguir usando su contraseña anterior"* al forzarse el
restablecimiento — de forma **inmediata**, no solo cuando el usuario
confirme el código. El diseño inicial del agente dejaba la contraseña
anterior funcionando con total normalidad hasta que alguien completara
el flujo de recuperación, lo cual contradecía literalmente el criterio
que se iba a calificar.

### Cómo lo detecté
Comparando la implementación, línea por línea, contra el texto exacto
del documento de la práctica, no contra mi recuerdo general del
requisito.

### Cómo lo corregí
Le pedí al agente agregar un campo `RestablecimientoPendiente` en
`Usuario`, que el endpoint de restablecimiento forzado activa de
inmediato, y que el login revisa **antes** de comparar la contraseña,
rechazando el acceso aunque la contraseña sea correcta. El campo se
limpia solo cuando el usuario confirma su nueva contraseña. Verificado
con pruebas reales: tras forzar el restablecimiento, el login con la
contraseña anterior se rechazó de inmediato (403), sin que nadie
confirmara ningún código.

## Auditoría final contra la rúbrica

Antes de etiquetar la entrega, le pedí al agente auditar, archivo por
archivo y línea por línea, cada ID de requisito de la práctica (RF-CA,
RF-NOT, RF-NEG, RD), sin apoyarse en lo conversado antes. Encontró y
reportó, sin que yo lo pidiera, que la validación del nombre de rol al
cambiar permisos vivía en el controlador en vez de en la capa de
aplicación (RD-02), que corregimos en un commit aparte.

## Limitaciones conocidas, documentadas y no corregidas por el plazo

- **Condición de carrera en códigos de un solo uso:** tanto el token de
  activación como el código de recuperación de contraseña se verifican
  con un patrón de "leer y luego escribir", no con una actualización
  condicional atómica. Dos confirmaciones simultáneas con el mismo
  código podrían, en teoría, pasar ambas. El proyecto ya usa el patrón
  correcto (actualización condicional) en el procesador de correos, y
  sería la solución a aplicar aquí en una iteración futura.
- **Un envío fallido detiene el resto del lote de correos pendientes:**
  el procesador no sigue con los siguientes correos si uno falla.
- **Correos que quedan en estado "Enviando" sin reintento automático**
  si el proceso se interrumpe a mitad de un envío.
- El JWT no valida emisor ni audiencia, solo firma y expiración.