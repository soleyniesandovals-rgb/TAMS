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
  await iniciarAsistencia();
}

async function iniciarAsistencia() {
  const principal = document.querySelector("main.contenido-app");
  const lista = document.querySelector('[data-js="lista-asistencia"]');
  const mensajePagina = document.querySelector('[data-js="mensaje-pagina"]');
  const mensajeVacio = document.querySelector('[data-js="mensaje-sin-datos"]');

  if (!lista) {
    return;
  }

  const moldeTarjeta = tomarMolde(
    lista,
    "a[data-js='ver-materias-asistencia']",
  );
  const moldeVentana = tomarMolde(
    principal,
    ".ventana-modal[id^='ventana-ver-materias-asistencia-']",
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

    const cursos = new Map();
    for (const asignacion of asignaciones) {
      const cursoId = asignacion?.cursoId;
      if (!cursos.has(cursoId)) {
        cursos.set(cursoId, []);
      }
      cursos.get(cursoId).push(asignacion);
    }

    const ventanas = [];
    for (const [cursoId, asignacionesCurso] of cursos) {
      lista.append(
        crearTarjeta(cursoId, asignacionesCurso[0], moldeTarjeta),
      );
      ventanas.push(
        crearVentana(
          cursoId,
          asignacionesCurso[0],
          asignacionesCurso,
          moldeVentana,
        ),
      );
    }
    lista.after(...ventanas);
  } catch (error) {
    mostrarMensaje(mensajePagina, error.message, "error");
  }
}

function cursoDe(asignacion) {
  return `${asignacion?.grado ?? ""} ${asignacion?.seccion ?? ""}`;
}

function crearTarjeta(cursoId, asignacion, molde) {
  const tarjeta = molde ? molde.cloneNode(true) : document.createElement("a");
  quitarEjemplo(tarjeta);

  tarjeta.id = `ver-materias-asistencia-${cursoId}`;
  tarjeta.href = `#ventana-materias-asistencia-${cursoId}`;

  const pestana = tarjeta.querySelector(".tarjeta-carpeta-pestana");
  if (pestana) {
    pestana.textContent = cursoDe(asignacion);
  }

  const anio = tarjeta.querySelector(".tarjeta-carpeta-anio");
  if (anio) {
    anio.textContent = asignacion?.anioEscolarNombre ?? "";
  }

  return tarjeta;
}

function crearVentana(cursoId, asignacion, asignacionesCurso, molde) {
  const ventana = molde ? molde.cloneNode(true) : document.createElement("div");
  quitarEjemplo(ventana);

  const idVentana = `ventana-materias-asistencia-${cursoId}`;
  const idTitulo = `titulo-ventana-materias-asistencia-${cursoId}`;

  ventana.id = idVentana;
  const caja = ventana.querySelector(".ventana-modal-caja");
  if (caja) {
    caja.setAttribute("aria-labelledby", idTitulo);
  }

  const titulo = ventana.querySelector(".ventana-modal-titulo");
  const anio = titulo?.querySelector("span");
  if (titulo && anio) {
    titulo.id = idTitulo;
    anio.textContent = asignacion?.anioEscolarNombre ?? "";
    if (titulo.firstChild?.nodeType === 3) {
      titulo.firstChild.textContent = `${cursoDe(asignacion)} · `;
    } else {
      titulo.insertBefore(
        document.createTextNode(`${cursoDe(asignacion)} · `),
        anio,
      );
    }
  }

  const listaEnlaces = ventana.querySelector(".lista-enlaces");
  const moldeEnlace = tomarMolde(listaEnlaces, "li");
  if (listaEnlaces) {
    for (const asignacionCurso of asignacionesCurso) {
      const fila = moldeEnlace
        ? moldeEnlace.cloneNode(true)
        : document.createElement("li");
      quitarEjemplo(fila);

      const celdas = fila.querySelectorAll("span");
      if (celdas[0]) {
        celdas[0].textContent = asignacionCurso?.materiaNombre ?? "";
      }
      if (celdas[1]) {
        celdas[1].textContent =
          asignacionCurso?.esTecnica === true ? "Técnica" : "Básica";
      }

      const enlace = fila.querySelector("a");
      if (enlace) {
        enlace.href = `asistencia-lista.html?asignacionId=${asignacionCurso?.id}`;
      }

      listaEnlaces.append(fila);
    }
  }

  return ventana;
}
