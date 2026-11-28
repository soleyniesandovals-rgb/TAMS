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
  await iniciarAsignaciones();
}

async function iniciarAsignaciones() {
  const tbody = document.querySelector('[data-js="tabla-asignaciones"] tbody');
  const mensajePagina = document.querySelector('[data-js="mensaje-pagina"]');
  const errorAsignacion = document.querySelector("#error-asignacion");
  const formularioFiltros = document.querySelector("#filtros-asignaciones");
  const filtroDocente = document.querySelector("#filtro-docente");
  const filtroCurso = document.querySelector("#filtro-curso");
  const formulario = document.querySelector("#form-asignar-docente");
  const selectorDocente = document.querySelector("#asignar-docente-campo");
  const selectorMateria = document.querySelector("#asignar-materia");
  const selectorCurso = document.querySelector("#asignar-curso");

  if (!tbody || !formulario) {
    return;
  }

  const moldeFila = tomarMolde(tbody, "tr");
  let asignaciones = [];

  function pintar() {
    tbody.replaceChildren();
    const docenteFiltro = String(filtroDocente?.value ?? "").trim().toLowerCase();
    const cursoFiltro = String(filtroCurso?.value ?? "").trim().toLowerCase();

    for (const asignacion of asignaciones) {
      const docente = `${asignacion?.docenteNombre ?? ""} ${asignacion?.docenteCorreo ?? ""}`.toLowerCase();
      const curso = `${asignacion?.grado ?? ""} ${asignacion?.seccion ?? ""}`.toLowerCase();
      if (!docente.includes(docenteFiltro) || !curso.includes(cursoFiltro)) {
        continue;
      }

      const fila = moldeFila ? moldeFila.cloneNode(true) : document.createElement("tr");
      quitarEjemplo(fila);
      const datos = [
        asignacion?.docenteNombre || asignacion?.docenteCorreo || "",
        asignacion?.materiaNombre ?? "",
        `${asignacion?.grado ?? ""} ${asignacion?.seccion ?? ""}`,
        asignacion?.anioEscolarNombre ?? "",
        String(asignacion?.horasSemanales ?? ""),
      ];
      const celdas = fila.querySelectorAll("td");
      datos.forEach((texto, indice) => {
        if (celdas[indice]) {
          celdas[indice].textContent = texto;
        }
      });
      tbody.append(fila);
    }
  }

  async function cargar() {
    try {
      asignaciones = await pedir("GET", "/api/asignaciones");
      pintar();
    } catch (error) {
      tbody.replaceChildren();
      mostrarMensaje(mensajePagina, error.message, "error");
    }
  }

  function llenarSelect(selector, elementos, texto, valor) {
    if (!selector) {
      return;
    }
    selector.replaceChildren();
    const opcionInicial = document.createElement("option");
    opcionInicial.value = "";
    opcionInicial.textContent = "Elige";
    selector.append(opcionInicial);

    for (const elemento of elementos) {
      const opcion = document.createElement("option");
      opcion.value = String(valor(elemento) ?? "");
      opcion.textContent = texto(elemento);
      selector.append(opcion);
    }
  }

  async function llenarFormulario() {
    const anios = await pedir("GET", "/api/anios-escolares");
    const activo = anios.find((anio) => anio?.esActivo === true);
    const cuentas = await pedir("GET", "/api/cuentas");
    const materias = await pedir("GET", "/api/materias");
    const cursos = activo
      ? await pedir(
          "GET",
          `/api/cursos?anioEscolarId=${encodeURIComponent(activo.id)}`,
        )
      : [];

    llenarSelect(
      selectorDocente,
      cuentas.filter((cuenta) => cuenta?.activo === true),
      (cuenta) => `${cuenta?.nombre ?? ""} (${cuenta?.correo ?? ""})`,
      (cuenta) => cuenta?.id,
    );
    llenarSelect(
      selectorMateria,
      materias,
      (materia) => materia?.nombre ?? "",
      (materia) => materia?.id,
    );
    llenarSelect(
      selectorCurso,
      cursos,
      (curso) => `${curso?.grado ?? ""} ${curso?.seccion ?? ""}`,
      (curso) => curso?.id,
    );
  }

  if (formularioFiltros) {
    formularioFiltros.noValidate = true;
    formularioFiltros.addEventListener("submit", (evento) => {
      evento.preventDefault();
      pintar();
    });
  }

  try {
    await llenarFormulario();
  } catch (error) {
    mostrarMensaje(mensajePagina, error.message, "error");
  }

  formulario.noValidate = true;
  formulario.addEventListener("submit", async (evento) => {
    evento.preventDefault();
    limpiar(errorAsignacion);

    const cuerpo = {
      usuarioId: Number(selectorDocente?.value ?? 0),
      materiaId: Number(selectorMateria?.value ?? 0),
      cursoId: Number(selectorCurso?.value ?? 0),
      horasSemanales: Number(formulario.elements.horasSemanales?.value ?? 0),
    };

    try {
      await pedir("POST", "/api/asignaciones", cuerpo);
      formulario.reset();
      location.hash = "";
      await cargar();
      mostrarMensaje(mensajePagina, "Asignación creada.", "exito");
    } catch (error) {
      mostrarMensaje(errorAsignacion, error.message, "error");
    }
  });

  await cargar();
}
