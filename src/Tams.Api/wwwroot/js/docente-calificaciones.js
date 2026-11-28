import { pedir } from "./api.js";
import { usuarioActual } from "./marco.js";
import { limpiar, mostrarMensaje, quitarEjemplo } from "./ui.js";

const usuario = await usuarioActual;
if (usuario) {
  await iniciar();
}

async function iniciar() {
  const principal = document.querySelector("main.contenido-app");
  if (!principal) {
    return;
  }

  const mensajePagina = principal.querySelector('[data-js="mensaje-pagina"]');
  const mensajeVacio = principal.querySelector('[data-js="mensaje-sin-datos"]');
  const form = principal.querySelector('[data-js="filtros-calificaciones"]');
  const curso = principal.querySelector("#filtro-curso");
  const seccion = principal.querySelector("#filtro-seccion");
  const materia = principal.querySelector("#filtro-materia");
  const titulos = principal.querySelectorAll("h2.subtitulo-tabla");

  if (!form || !curso || !seccion || !materia || titulos.length < 2) {
    return;
  }

  // La primera sección de la maqueta es el molde; la segunda se descarta.
  const moldeTitulo = titulos[0].cloneNode(true);
  const moldeTabla = titulos[0].nextElementSibling?.cloneNode(true);
  titulos[0].nextElementSibling?.remove();
  titulos[1].nextElementSibling?.remove();
  titulos[0].remove();
  titulos[1].remove();

  limpiar(mensajePagina);
  if (mensajeVacio) {
    mensajeVacio.hidden = true;
  }

  let asignaciones = [];
  let resultado = [];

  function opcionVacia(texto) {
    const opcion = document.createElement("option");
    opcion.value = "";
    opcion.textContent = texto;
    return opcion;
  }

  function llenarSelect(select, valores, textoVacio) {
    select.replaceChildren(opcionVacia(textoVacio));
    for (const valor of [...new Set(valores)]) {
      if (esVacio(valor)) {
        continue;
      }
      const opcion = document.createElement("option");
      opcion.value = String(valor);
      opcion.textContent = String(valor);
      select.append(opcion);
    }
  }

  function llenarSecciones() {
    llenarSelect(
      seccion,
      asignaciones
        .filter((asignacion) => asignacion?.grado === curso.value)
        .map((asignacion) => asignacion.seccion),
      "Sección",
    );
    llenarMaterias();
  }

  function llenarMaterias() {
    materia.replaceChildren(opcionVacia("Materia"));
    for (const asignacion of asignaciones) {
      if (
        asignacion?.grado !== curso.value ||
        asignacion?.seccion !== seccion.value
      ) {
        continue;
      }
      const opcion = document.createElement("option");
      opcion.value = String(asignacion.id);
      opcion.textContent = asignacion.materiaNombre ?? "";
      materia.append(opcion);
    }
  }

  function pintar(tabla) {
    for (const elemento of resultado) {
      elemento.remove();
    }
    resultado = [];

    const titulo = moldeTitulo ? moldeTitulo.cloneNode(true) : document.createElement("h2");
    const contenedor = moldeTabla
      ? moldeTabla.cloneNode(true)
      : document.createElement("div");
    quitarEjemplo(titulo);
    quitarEjemplo(contenedor);

    const modalidad =
      tabla?.esTecnica === true
        ? "técnica: resultados de aprendizaje"
        : "básica: P1 a P4";
    titulo.textContent =
      `${tabla?.materiaNombre ?? ""} · ${tabla?.grado ?? ""} ${tabla?.seccion ?? ""} (${modalidad})`;

    const columnas = Array.isArray(tabla?.columnas) ? tabla.columnas : [];
    const estudiantes = Array.isArray(tabla?.estudiantes) ? tabla.estudiantes : [];
    const tablaElemento = contenedor.querySelector("table");
    const cabecera = tablaElemento?.querySelector("thead tr");
    const cuerpo = tablaElemento?.querySelector("tbody");

    if (cabecera) {
      cabecera.replaceChildren();
      for (const texto of ["##", "Estudiante"]) {
        cabecera.append(crearCelda("th", texto, "col"));
      }
      for (const columna of columnas) {
        let texto = columna.etiqueta ?? "";
        if (tabla?.esTecnica === true && !esVacio(columna.puntos)) {
          texto += ` (${columna.puntos})`;
        }
        cabecera.append(crearCelda("th", texto, "col"));
      }
      cabecera.append(crearCelda("th", "Acumulado", "col"));
    }

    if (cuerpo) {
      cuerpo.replaceChildren();
      for (const [indice, estudiante] of estudiantes.entries()) {
        const fila = document.createElement("tr");
        fila.append(crearCelda("td", String(indice + 1)));
        fila.append(
          crearCelda(
            "td",
            `${estudiante?.nombre ?? ""} ${estudiante?.apellido ?? ""}`,
          ),
        );
        for (const columna of columnas) {
          const nota = estudiante?.notas?.find(
            (nota) => nota.numero === columna.numero,
          );
          const celda = crearCelda(
            "td",
            esVacio(nota?.puntajeEfectivo) ? "" : formatear(nota.puntajeEfectivo),
          );
          celda.className = "celda-centro";
          fila.append(celda);
        }
        const acumulado = crearCelda(
          "td",
          esVacio(estudiante?.acumulado) ? "" : formatear(estudiante.acumulado),
        );
        acumulado.className = "celda-centro";
        fila.append(acumulado);
        cuerpo.append(fila);
      }
    }

    const ancla = form.closest(".titulo-pagina-fila") ?? form;
    ancla.after(titulo, contenedor);
    resultado = [titulo, contenedor];

    if (estudiantes.length === 0) {
      mostrarMensaje(mensajePagina, "Este curso no tiene estudiantes.");
    }
  }

  async function consultar(asignacionId) {
    limpiar(mensajePagina);
    try {
      const tabla = await pedir(
        "GET",
        `/api/docente/asignaciones/${asignacionId}/calificaciones`,
      );
      pintar(tabla);
    } catch (error) {
      for (const elemento of resultado) {
        elemento.remove();
      }
      resultado = [];
      mostrarMensaje(mensajePagina, error.message, "error");
    }
  }

  curso.addEventListener("change", llenarSecciones);
  seccion.addEventListener("change", llenarMaterias);
  form.addEventListener("submit", async (evento) => {
    evento.preventDefault();
    if (!curso.value || !seccion.value || !materia.value) {
      mostrarMensaje(mensajePagina, "Elige curso, sección y materia.", "error");
      return;
    }
    await consultar(materia.value);
  });

  try {
    asignaciones = await pedir("GET", "/api/docente/asignaciones");
  } catch (error) {
    mostrarMensaje(mensajePagina, error.message, "error");
    return;
  }

  if (!Array.isArray(asignaciones) || asignaciones.length === 0) {
    if (mensajeVacio) {
      mensajeVacio.hidden = false;
    }
    return;
  }

  llenarSecciones();

  const crudo = new URLSearchParams(location.search).get("asignacionId");
  if (crudo !== null) {
    const asignacionId = Number(crudo);
    const idValido =
      /^\d+$/.test(crudo) &&
      Number.isSafeInteger(asignacionId) &&
      asignacionId > 0;
    const asignacion = idValido
      ? asignaciones.find((entrada) => entrada.id === asignacionId)
      : undefined;

    if (asignacion) {
      curso.value = asignacion.grado ?? "";
      llenarSecciones();
      seccion.value = asignacion.seccion ?? "";
      llenarMaterias();
      materia.value = String(asignacion.id);
      await consultar(asignacion.id);
      return;
    }

    mostrarMensaje(mensajePagina, "La asignación no es válida.", "error");
  }
}

function crearCelda(etiqueta, texto, ambito) {
  const celda = document.createElement(etiqueta);
  celda.textContent = texto;
  if (ambito) {
    celda.scope = ambito;
  }
  return celda;
}

function esVacio(valor) {
  return valor === null || valor === undefined || valor === "";
}

function formatear(valor) {
  if (esVacio(valor)) {
    return "";
  }
  const numero = Number(valor);
  if (Number.isNaN(numero)) {
    return String(valor);
  }
  return numero.toLocaleString("es-DO", { maximumFractionDigits: 2 });
}
