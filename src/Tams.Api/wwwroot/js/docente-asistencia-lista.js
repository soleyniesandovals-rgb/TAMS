import { pedir } from "./api.js";
import { usuarioActual } from "./marco.js";
import {
  limpiar,
  mostrarMensaje,
  quitarEjemplo,
  tomarMolde,
} from "./ui.js";

const ESTADOS = ["Presente", "Ausente", "Tardanza", "Excusa"];

const usuario = await usuarioActual;
if (usuario) {
  await iniciarLista();
}

async function iniciarLista() {
  const principal = document.querySelector("main.contenido-app");
  const fechaInput = principal?.querySelector("#asistencia-fecha");
  const buscar = principal?.querySelector('[data-js="asistencia-buscar"]');
  const mensajePagina = principal?.querySelector('[data-js="mensaje-pagina"]');
  const mensajeEnviada = principal?.querySelector(
    '[data-js="mensaje-asistencia-enviada"]',
  );
  const aviso = principal?.querySelector('[data-js="aviso-edicion"]');
  const resumen = principal?.querySelector('[data-js="resumen-asistencia"]');
  const tbody = principal?.querySelector("#tabla-asistencia tbody");
  const editar = principal?.querySelector('[data-js="asistencia-editar"]');
  const enviar = principal?.querySelector('[data-js="asistencia-enviar"]');
  const titulos = principal?.querySelectorAll("h1.titulo-pagina span");

  if (!principal || !tbody || !enviar || !editar || !fechaInput) {
    return;
  }

  const moldeFila = tomarMolde(tbody, "tr");
  if (resumen) {
    quitarEjemplo(resumen);
    resumen.textContent = "";
  }
  for (const titulo of titulos ?? []) {
    titulo.textContent = "";
    titulo.removeAttribute("data-ejemplo");
  }

  const crudo = new URLSearchParams(location.search).get("asignacionId");
  const asignacionId = Number(crudo);
  const idValido =
    crudo !== null &&
    /^\d+$/.test(crudo) &&
    Number.isSafeInteger(asignacionId) &&
    asignacionId > 0;

  if (!idValido) {
    mostrarMensaje(mensajePagina, "La asignación no es válida.", "error");
    editar.disabled = true;
    enviar.disabled = true;
    return;
  }

  const hoy = fechaLocal(new Date());
  fechaInput.value = hoy;
  fechaInput.max = hoy;

  let listaActual = null;

  function actualizarResumen() {
    if (!resumen) {
      return;
    }

    const conteos = Object.fromEntries(ESTADOS.map((estado) => [estado, 0]));
    for (const radio of tbody.querySelectorAll('input[type="radio"]:checked')) {
      if (Object.hasOwn(conteos, radio.value)) {
        conteos[radio.value] += 1;
      }
    }

    resumen.textContent =
      `${conteos.Presente} presentes · ${conteos.Ausente} ausentes · ` +
      `${conteos.Tardanza} tardanzas · ${conteos.Excusa} excusas`;
  }

  function mostrarAviso(editadaEn) {
    if (!aviso) {
      return;
    }

    if (!editadaEn || Number.isNaN(new Date(editadaEn).getTime())) {
      aviso.hidden = true;
      return;
    }

    const fecha = new Date(editadaEn);
    const dos = (valor) => String(valor).padStart(2, "0");
    const texto = aviso.querySelector("span");
    if (texto) {
      texto.textContent =
        `${dos(fecha.getDate())}/${dos(fecha.getMonth() + 1)}/` +
        `${fecha.getFullYear()} a las ${dos(fecha.getHours())}:` +
        dos(fecha.getMinutes());
      texto.removeAttribute("data-ejemplo");
    }
    aviso.hidden = false;
  }

  function pintarLista(respuesta) {
    listaActual = respuesta;
    limpiar(mensajePagina);
    limpiar(mensajeEnviada);

    if (titulos?.[0]) {
      titulos[0].textContent = respuesta?.materiaNombre ?? "";
      titulos[0].removeAttribute("data-ejemplo");
    }
    if (titulos?.[1]) {
      titulos[1].textContent =
        `${respuesta?.grado ?? ""} ${respuesta?.seccion ?? ""}`;
      titulos[1].removeAttribute("data-ejemplo");
    }

    mostrarAviso(respuesta?.editadaEn ?? null);

    const registros = Array.isArray(respuesta?.registros)
      ? respuesta.registros
      : [];
    tbody.replaceChildren();

    for (const [indice, registro] of registros.entries()) {
      const fila = moldeFila
        ? moldeFila.cloneNode(true)
        : document.createElement("tr");
      quitarEjemplo(fila);
      fila.dataset.estudianteId = String(registro?.estudianteId ?? "");

      const celdas = fila.querySelectorAll("td");
      if (celdas[0]) {
        celdas[0].textContent = String(indice + 1);
      }
      if (celdas[1]) {
        celdas[1].textContent = registro?.nombre ?? "";
      }
      if (celdas[2]) {
        celdas[2].textContent = registro?.apellido ?? "";
      }

      const nombreCompleto =
        `${registro?.nombre ?? ""} ${registro?.apellido ?? ""}`;
      const estado = registro?.estado ?? null;
      for (const radio of fila.querySelectorAll('input[type="radio"]')) {
        radio.name = `estado-${registro?.estudianteId}`;
        radio.checked =
          estado === null ? radio.value === "Presente" : radio.value === estado;
        const texto = radio.parentElement?.querySelector(".texto-oculto");
        if (texto) {
          texto.textContent = `${radio.value} — ${nombreCompleto}`;
        }
      }

      const observacion = fila.querySelector('input[type="text"]');
      if (observacion) {
        const id = `observacion-${registro?.estudianteId}`;
        observacion.id = id;
        observacion.name = id;
        observacion.value = registro?.observacion ?? "";

        const etiqueta = observacion
          .closest("td")
          ?.querySelector("label.texto-oculto");
        if (etiqueta) {
          etiqueta.htmlFor = id;
          etiqueta.textContent = `Observación de ${registro?.nombre ?? ""}`;
        }
      }

      tbody.append(fila);
    }

    actualizarResumen();

    if (registros.length === 0) {
      mostrarMensaje(mensajePagina, "Este curso no tiene estudiantes.");
      editar.hidden = true;
      editar.disabled = true;
      enviar.disabled = true;
      return;
    }

    const guardada = respuesta?.guardada === true;
    for (const entrada of tbody.querySelectorAll("input")) {
      entrada.disabled = guardada;
    }
    editar.hidden = !guardada;
    editar.disabled = false;
    enviar.disabled = guardada;
  }

  async function cargarLista() {
    const valor = fechaInput.value;
    if (!valor) {
      mostrarMensaje(mensajePagina, "Elige una fecha.", "error");
      return;
    }

    limpiar(mensajePagina);
    limpiar(mensajeEnviada);
    enviar.disabled = true;
    editar.disabled = true;

    try {
      const respuesta = await pedir(
        "GET",
        `/api/docente/asistencias?asignacionId=${asignacionId}` +
          `&fecha=${encodeURIComponent(valor)}`,
      );
      pintarLista(respuesta);
    } catch (error) {
      listaActual = null;
      tbody.replaceChildren();
      if (resumen) {
        resumen.textContent = "";
      }
      if (aviso) {
        aviso.hidden = true;
      }
      editar.hidden = true;
      editar.disabled = true;
      enviar.disabled = true;
      mostrarMensaje(mensajePagina, error.message, "error");
    }
  }

  async function guardarLista() {
    if (!listaActual || enviar.disabled) {
      return;
    }

    const registros = Array.from(tbody.querySelectorAll("tr"), (fila) => {
      const texto = fila.querySelector('input[type="text"]')?.value.trim() ?? "";
      return {
        estudianteId: Number(fila.dataset.estudianteId),
        estado:
          fila.querySelector('input[type="radio"]:checked')?.value ??
          "Presente",
        observacion: texto.length > 0 ? texto : null,
      };
    });

    enviar.disabled = true;
    editar.disabled = true;

    try {
      const respuesta = await pedir("PUT", "/api/docente/asistencias", {
        asignacionId,
        fecha: listaActual?.fecha ?? "",
        registros,
      });
      pintarLista(respuesta);
      mostrarMensaje(mensajeEnviada, "Lista enviada.", "exito");
    } catch (error) {
      mostrarMensaje(mensajePagina, error.message, "error");
      enviar.disabled = false;
    } finally {
      editar.disabled = false;
    }
  }

  tbody.addEventListener("change", actualizarResumen);
  buscar?.addEventListener("click", cargarLista);
  editar.addEventListener("click", () => {
    if (!listaActual) {
      return;
    }

    for (const entrada of tbody.querySelectorAll("input")) {
      entrada.disabled = false;
    }
    editar.hidden = true;
    enviar.disabled = false;
    limpiar(mensajeEnviada);
  });
  enviar.addEventListener("click", guardarLista);

  await cargarLista();
}

function fechaLocal(fecha) {
  const anio = fecha.getFullYear();
  const mes = String(fecha.getMonth() + 1).padStart(2, "0");
  const dia = String(fecha.getDate()).padStart(2, "0");
  return `${anio}-${mes}-${dia}`;
}
