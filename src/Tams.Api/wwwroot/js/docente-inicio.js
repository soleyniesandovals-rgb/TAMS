import { usuarioActual } from "./marco.js";
import { tomarMolde } from "./ui.js";

const usuario = await usuarioActual;
if (usuario) {
  iniciarInicio(usuario);
}

function iniciarInicio(usuario) {
  const saludo = document.querySelector("h1.titulo-pagina span[data-ejemplo]");
  if (saludo) {
    saludo.textContent = usuario.nombre ?? "";
    saludo.removeAttribute("data-ejemplo");
  }

  const lista = document.querySelector('[data-js="proximos-eventos"]');
  const mensaje = document.querySelector('[data-js="mensaje-sin-eventos"]');

  // B8: los eventos propios y los días lectivos todavía no llegan a esta pantalla.
  tomarMolde(lista, "li.evento");

  if (mensaje) {
    mensaje.hidden = false;
  }
}
