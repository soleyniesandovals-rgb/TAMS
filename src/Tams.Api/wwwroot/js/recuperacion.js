import { pedir } from "./api.js";
import { limpiar, mostrarMensaje } from "./ui.js";

const CLAVE_CODIGO = "tams.recuperacion.codigo";

const formularioCorreo = document.querySelector('[data-js="recuperar-correo"]');
const formularioCodigo = document.querySelector('[data-js="recuperar-codigo"]');
const formularioNueva = document.querySelector('[data-js="recuperar-nueva"]');

if (formularioCorreo) {
  const mensaje = document.querySelector('[data-js="mensaje-recuperar-correo"]');

  formularioCorreo.addEventListener("submit", async (evento) => {
    evento.preventDefault();
    limpiar(mensaje);

    const correo = document.querySelector("#recuperar-correo").value;

    try {
      await pedir("POST", "/api/cuentas/recuperacion/iniciar", { correo });
      location.assign("recuperar-codigo.html");
    } catch (error) {
      mostrarMensaje(mensaje, error.message, "error");
    }
  });
}

if (formularioCodigo) {
  const campoCodigo = document.querySelector("#recuperar-codigo-campo");
  const codigoDelCorreo = new URLSearchParams(location.search).get("codigo");
  if (codigoDelCorreo !== null) {
    campoCodigo.value = codigoDelCorreo;
  }

  formularioCodigo.addEventListener("submit", (evento) => {
    evento.preventDefault();
    sessionStorage.setItem(CLAVE_CODIGO, campoCodigo.value);
    location.assign("recuperar-nueva.html");
  });
}

if (formularioNueva) {
  const mensaje = document.querySelector('[data-js="mensaje-recuperar-nueva"]');

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

    try {
      await pedir("POST", "/api/cuentas/recuperacion/confirmar", {
        codigo,
        contrasena,
      });
      sessionStorage.removeItem(CLAVE_CODIGO);
      location.assign("index.html");
    } catch (error) {
      mostrarMensaje(mensaje, error.message, "error");
    }
  });
}
