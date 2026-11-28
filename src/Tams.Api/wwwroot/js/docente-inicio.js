import { usuarioActual } from "./marco.js";
import { cargarProximosEventos } from "./proximos-eventos.js";

const usuario = await usuarioActual;
if (usuario) {
  await iniciarInicio(usuario);
}

async function iniciarInicio(usuario) {
  const saludo = document.querySelector("h1.titulo-pagina span[data-ejemplo]");
  if (saludo) {
    saludo.textContent = usuario.nombre ?? "";
    saludo.removeAttribute("data-ejemplo");
  }

  const lista = document.querySelector('[data-js="proximos-eventos"]');
  const mensaje = document.querySelector('[data-js="mensaje-sin-eventos"]');
  await cargarProximosEventos(lista, mensaje);
}
