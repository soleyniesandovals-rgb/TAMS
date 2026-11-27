import { pedir } from "./api.js";
import { limpiar, mostrarMensaje } from "./ui.js";

const CLAVE_CODIGO = "tams.recuperacion.codigo";

const formularioCorreo = document.querySelector('[data-js="recuperar-correo"]');
const formularioCodigo = document.querySelector('[data-js="recuperar-codigo"]');
const formularioNueva = document.querySelector('[data-js="recuperar-nueva"]');

if (formularioCorreo) {
  const mensaje = document.querySelector('[data-js="mensaje-recuperar-correo"]');
  const botonCorreo = formularioCorreo.querySelector('button[type="submit"]');

  formularioCorreo.addEventListener("submit", async (evento) => {
    evento.preventDefault();
    limpiar(mensaje);

    const correo = document.querySelector("#recuperar-correo").value;

    botonCorreo.disabled = true;
    try {
      await pedir("POST", "/api/cuentas/recuperacion/iniciar", { correo });
      location.assign("recuperar-codigo.html");
    } catch (error) {
      botonCorreo.disabled = false;
      mostrarMensaje(mensaje, error.message, "error");
    }
  });
}

if (formularioCodigo) {
  const campoCodigo = document.querySelector("#recuperar-codigo-campo");
  const codigoDelCorreo = new URLSearchParams(location.search).get("codigo");
  if (codigoDelCorreo !== null) {
    campoCodigo.value = codigoDelCorreo.trim();
  }

  formularioCodigo.addEventListener("submit", (evento) => {
    evento.preventDefault();
    sessionStorage.setItem(CLAVE_CODIGO, campoCodigo.value.trim());
    location.assign("recuperar-nueva.html");
  });
}

if (formularioNueva) {
  const mensaje = document.querySelector('[data-js="mensaje-recuperar-nueva"]');
  const botonNueva = formularioNueva.querySelector('button[type="submit"]');

  formularioNueva.addEventListener("submit", async (evento) => {
    evento.preventDefault();
    limpiar(mensaje);

    const contrasena = document.querySelector("#nueva-contrasena").value;
    const repeticion = document.querySelector("#nueva-repetir").value;

    if (contrasena !== repeticion) {
      mostrarMensaje(mensaje, "Las contraseñas no coinciden.", "error");
      return;
    }

    const codigo = sessionStorage.getItem(CLAVE_CODIGO);
    if (!codigo) {
      mostrarMensaje(mensaje, "Primero escribe el código que recibiste por correo.", "error");
      return;
    }

    botonNueva.disabled = true;
    try {
      await pedir("POST", "/api/cuentas/recuperacion/confirmar", {
        codigo,
        contrasena,
      });
      sessionStorage.removeItem(CLAVE_CODIGO);
      mostrarMensaje(mensaje, "Contraseña cambiada. Ya puedes iniciar sesión.", "exito");
      setTimeout(() => {
        location.assign("index.html");
      }, 2000);
    } catch (error) {
      botonNueva.disabled = false;
      mostrarMensaje(mensaje, error.message, "error");
    }
  });
}
