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
  await iniciarEstudiantes();
}

async function iniciarEstudiantes() {
  const titulo = document.querySelector("h1.titulo-pagina");
  const tbody = document.querySelector('[data-js="lista-estudiantes"] tbody');
  const mensajePagina = document.querySelector('[data-js="mensaje-pagina"]');
  const mensajeVacio = document.querySelector(
    '[data-js="mensaje-sin-estudiantes"]',
  );
  const cursoId = new URLSearchParams(location.search).get("cursoId");
  const moldeFila = tomarMolde(tbody, "tr");

  if (mensajeVacio) {
    mensajeVacio.hidden = true;
  }
  limpiar(mensajePagina);

  if (!cursoId) {
    if (titulo) {
      titulo.textContent = "Estudiantes";
    }
    mostrarMensaje(mensajePagina, "Elige un curso desde Cursos.", "error");
    return;
  }

  try {
    const asignaciones = await pedir("GET", "/api/docente/asignaciones");
    const asignacion = Array.isArray(asignaciones)
      ? asignaciones.find((item) => String(item?.cursoId) === cursoId)
      : undefined;

    if (asignacion && titulo) {
      titulo.textContent = `Estudiantes · ${asignacion.grado ?? ""} ${asignacion.seccion ?? ""}`;
    } else if (titulo) {
      titulo.textContent = "Estudiantes";
    }

    const estudiantes = await pedir(
      "GET",
      `/api/docente/cursos/${encodeURIComponent(cursoId)}/estudiantes`,
    );

    if (!Array.isArray(estudiantes) || estudiantes.length === 0) {
      if (mensajeVacio) {
        mensajeVacio.hidden = false;
      }
      return;
    }

    for (const [indice, estudiante] of estudiantes.entries()) {
      const fila = moldeFila
        ? moldeFila.cloneNode(true)
        : document.createElement("tr");
      quitarEjemplo(fila);

      const celdas = fila.querySelectorAll("td");
      const datos = [
        `${indice + 1}`,
        estudiante?.nombre ?? "",
        estudiante?.apellido ?? "",
      ];
      datos.forEach((texto, posicion) => {
        if (celdas[posicion]) {
          celdas[posicion].textContent = texto;
        }
      });

      tbody.append(fila);
    }
  } catch (error) {
    mostrarMensaje(mensajePagina, error.message, "error");
  }
}
