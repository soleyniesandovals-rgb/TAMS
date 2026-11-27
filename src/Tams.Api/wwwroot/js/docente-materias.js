import { pedir } from "./api.js";
import { usuarioActual } from "./marco.js";
import {
  limpiar,
  mostrarMensaje,
  quitarEjemplo,
  tomarMolde,
} from "./ui.js";

const usuario = await usuarioActual;
if (usuario) {
  await iniciarMaterias();
}

async function iniciarMaterias() {
  const principal = document.querySelector("main.contenido-app");
  const lista = document.querySelector('[data-js="lista-asignaciones"]');
  const mensajePagina = document.querySelector('[data-js="mensaje-pagina"]');
  const mensajeVacio = document.querySelector('[data-js="mensaje-sin-datos"]');

  if (!lista) {
    return;
  }

  const moldeTarjeta = tomarMolde(lista, "a[data-js='ver-asignacion']");
  const moldeVentana = tomarMolde(
    principal,
    ".ventana-modal[id^='ventana-asignacion-']",
  );

  limpiar(mensajePagina);

  try {
    const asignaciones = await pedir("GET", "/api/docente/asignaciones");

    if (!Array.isArray(asignaciones) || asignaciones.length === 0) {
      if (mensajeVacio) {
        mensajeVacio.hidden = false;
      }
      return;
    }

    if (mensajeVacio) {
      mensajeVacio.hidden = true;
    }

    const ventanas = [];
    for (const asignacion of asignaciones) {
      lista.append(crearTarjeta(asignacion, moldeTarjeta));
      ventanas.push(crearVentana(asignacion, moldeVentana));
    }
    lista.after(...ventanas);
  } catch (error) {
    mostrarMensaje(mensajePagina, error.message, "error");
  }
}

function cursoDe(asignacion) {
  return `${asignacion?.grado ?? ""} ${asignacion?.seccion ?? ""}`;
}

function crearTarjeta(asignacion, molde) {
  const tarjeta = molde ? molde.cloneNode(true) : document.createElement("a");
  quitarEjemplo(tarjeta);

  const id = asignacion?.id;
  tarjeta.id = `ver-asignacion-${id}`;
  tarjeta.href = `#ventana-asignacion-${id}`;

  const pestana = tarjeta.querySelector(".tarjeta-carpeta-pestana");
  if (pestana) {
    pestana.textContent = asignacion?.materiaNombre ?? "";
  }

  const anio = tarjeta.querySelector(".tarjeta-carpeta-anio");
  if (anio) {
    anio.textContent = asignacion?.anioEscolarNombre ?? "";
  }

  const curso = tarjeta.querySelector(".tarjeta-carpeta-curso");
  if (curso) {
    curso.textContent = cursoDe(asignacion);
  }

  return tarjeta;
}

function crearVentana(asignacion, molde) {
  const ventana = molde ? molde.cloneNode(true) : document.createElement("div");
  quitarEjemplo(ventana);

  const id = asignacion?.id;
  const idVentana = `ventana-asignacion-${id}`;
  const idTitulo = `titulo-ventana-asignacion-${id}`;

  ventana.id = idVentana;
  const caja = ventana.querySelector(".ventana-modal-caja");
  if (caja) {
    caja.setAttribute("aria-labelledby", idTitulo);
  }

  const titulo = ventana.querySelector(".ventana-modal-titulo");
  if (titulo) {
    titulo.id = idTitulo;
    titulo.textContent = `${asignacion?.materiaNombre ?? ""} · ${cursoDe(asignacion)}`;
  }

  const datos = [
    asignacion?.esTecnica === true ? "Técnica" : "Básica",
    cursoDe(asignacion),
    `${asignacion?.horasSemanales ?? ""}`,
    asignacion?.anioEscolarNombre ?? "",
  ];
  const casillas = ventana.querySelectorAll(".lista-detalles dd");
  datos.forEach((texto, indice) => {
    if (casillas[indice]) {
      casillas[indice].textContent = texto;
    }
  });

  const pasarLista = ventana.querySelector('[data-js="pasar-lista"]');
  if (pasarLista) {
    pasarLista.id = `pasar-lista-${id}`;
    pasarLista.href = `asistencia-lista.html?asignacionId=${id}`;
  }

  const calificar = ventana.querySelector('[data-js="calificar"]');
  if (calificar) {
    calificar.id = `calificar-${id}`;
    calificar.href = `calificar.html?asignacionId=${id}`;
  }

  return ventana;
}
