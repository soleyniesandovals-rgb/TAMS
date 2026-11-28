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
  const tbody = document.querySelector('[data-js="tabla-estudiantes-curso"] tbody');
  const mensajePagina = document.querySelector('[data-js="mensaje-pagina"]');
  const mensajeVacio = document.querySelector(
    '[data-js="mensaje-sin-estudiantes"]',
  );
  const mensajeVentana = document.querySelector('[data-js="mensaje-inscribir"]');
  const formulario = document.querySelector("#form-inscribir-estudiante");

  if (!tbody) {
    return;
  }

  const moldeFila = tomarMolde(tbody, "tr");
  const crudo = new URLSearchParams(location.search).get("cursoId");
  const cursoId = Number(crudo);
  const idValido =
    crudo !== null &&
    /^\d+$/.test(crudo) &&
    Number.isSafeInteger(cursoId) &&
    cursoId > 0;

  limpiar(mensajePagina);
  if (mensajeVacio) {
    mensajeVacio.hidden = true;
  }

  if (!idValido) {
    if (titulo) {
      titulo.textContent = "Estudiantes";
    }
    mostrarMensaje(mensajePagina, "El curso no es válido.", "error");
    return;
  }

  async function cargar() {
    try {
      const cursos = await pedir("GET", "/api/cursos");
      const curso = Array.isArray(cursos)
        ? cursos.find((item) => item?.id === cursoId)
        : undefined;

      if (!curso) {
        if (titulo) {
          titulo.textContent = "Estudiantes";
        }
        mostrarMensaje(mensajePagina, "El curso no es válido.", "error");
        return;
      }

      if (titulo) {
        titulo.textContent = `Estudiantes · ${curso.grado ?? ""} ${curso.seccion ?? ""}`;
      }

      const estudiantes = await pedir(
        "GET",
        `/api/cursos/${encodeURIComponent(cursoId)}/estudiantes`,
      );
      tbody.replaceChildren();

      if (!Array.isArray(estudiantes) || estudiantes.length === 0) {
        if (mensajeVacio) {
          mensajeVacio.hidden = false;
        }
        return;
      }

      if (mensajeVacio) {
        mensajeVacio.hidden = true;
      }
      for (const [indice, estudiante] of estudiantes.entries()) {
        const fila = moldeFila ? moldeFila.cloneNode(true) : document.createElement("tr");
        quitarEjemplo(fila);
        const celdas = fila.querySelectorAll("td");
        const datos = [
          String(indice + 1),
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

  if (formulario) {
    formulario.noValidate = true;
    formulario.addEventListener("submit", async (evento) => {
      evento.preventDefault();
      limpiar(mensajeVentana);
      const cuerpo = {
        nombre: String(formulario.elements.nombre?.value ?? ""),
        apellido: String(formulario.elements.apellido?.value ?? ""),
        cedula: String(formulario.elements.cedula?.value ?? "").trim(),
      };

      try {
        await pedir(
          "POST",
          `/api/cursos/${encodeURIComponent(cursoId)}/estudiantes`,
          cuerpo,
        );
        formulario.reset();
        location.hash = "";
        await cargar();
        mostrarMensaje(mensajePagina, "Estudiante inscrito.", "exito");
      } catch (error) {
        mostrarMensaje(mensajeVentana, error.message, "error");
      }
    });
  }

  await cargar();
}
