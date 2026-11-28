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
  await iniciarSolicitudes();
}

async function iniciarSolicitudes() {
  const principal = document.querySelector("main.contenido-app");
  const tbody = document.querySelector('[data-js="tabla-solicitudes"] tbody');
  const mensajePagina = document.querySelector('[data-js="mensaje-pagina"]');
  const mensajeVacio = document.querySelector(
    '[data-js="mensaje-sin-solicitudes"]',
  );
  const formularioFiltros = document.querySelector("#filtros-solicitudes");
  const desde = document.querySelector("#solicitudes-desde");
  const hasta = document.querySelector("#solicitudes-hasta");
  const formularioAprobar = document.querySelector("#form-aprobar");
  const formularioRechazar = document.querySelector("#form-rechazar");

  if (!tbody) {
    return;
  }

  const moldeFila = tomarMolde(tbody, "tr");
  const moldeVer = tomarMolde(
    principal,
    ".ventana-modal[id^='ventana-ver-solicitud-']",
  );

  let solicitudes = [];
  let cuentas = [];
  let idSeleccionada = null;

  function fechaLocal(fecha) {
    if (!fecha) {
      return "";
    }
    const valor = new Date(fecha);
    if (Number.isNaN(valor.getTime())) {
      return "";
    }
    const anio = valor.getFullYear();
    const mes = String(valor.getMonth() + 1).padStart(2, "0");
    const dia = String(valor.getDate()).padStart(2, "0");
    return `${anio}-${mes}-${dia}`;
  }

  function fechaLegible(fecha) {
    if (!fecha) {
      return "";
    }
    const valor = new Date(fecha);
    return Number.isNaN(valor.getTime())
      ? ""
      : valor.toLocaleDateString("es-DO");
  }

  function tipoLegible(tipo) {
    if (tipo === "acceso-reportes") {
      return "Acceso a reportes";
    }
    if (tipo === "corregir-calificacion") {
      return "Corregir calificación";
    }
    return String(tipo ?? "");
  }

  function nombreSolicitante(solicitud) {
    const id = solicitud?.solicitanteId;
    const cuenta = cuentas.find((item) => item?.id === id);
    return cuenta?.nombre || `Usuario ${id}`;
  }

  function pintar() {
    const abierta = location.hash.slice(1);
    principal
      .querySelectorAll(".ventana-modal[id^='ventana-ver-solicitud-']")
      .forEach((ventana) => ventana.remove());
    tbody.replaceChildren();

    const desdeValor = desde?.value ?? "";
    const hastaValor = hasta?.value ?? "";
    const visibles = solicitudes.filter((solicitud) => {
      const fecha = fechaLocal(solicitud?.fechaCreacion);
      if (desdeValor && (!fecha || fecha < desdeValor)) {
        return false;
      }
      if (hastaValor && (!fecha || fecha > hastaValor)) {
        return false;
      }
      return true;
    });

    if (mensajeVacio) {
      mensajeVacio.hidden = visibles.length > 0;
    }

    const ventanas = [];
    for (const solicitud of visibles) {
      const id = solicitud?.id;
      tbody.append(
        crearFila(solicitud, moldeFila, nombreSolicitante(solicitud), (valor) => {
          idSeleccionada = valor;
          limpiar(document.querySelector('[data-js="mensaje-aprobacion"]'));
          limpiar(document.querySelector('[data-js="mensaje-rechazo"]'));
        }),
      );
      ventanas.push(
        crearVentanaVer(
          solicitud,
          moldeVer,
          nombreSolicitante(solicitud),
        ),
      );
    }
    principal.append(...ventanas);

    if (abierta && principal.querySelector(`[id="${abierta}"]`)) {
      history.replaceState(null, "", location.pathname + location.search);
      location.replace(`#${abierta}`);
    }
  }

  async function cargar() {
    try {
      solicitudes = await pedir("GET", "/api/permisos/pendientes");
      pintar();
    } catch (error) {
      tbody.replaceChildren();
      mostrarMensaje(mensajePagina, error.message, "error");
    }
  }

  try {
    cuentas = await pedir("GET", "/api/cuentas");
  } catch {
    cuentas = [];
  }

  if (formularioFiltros) {
    formularioFiltros.noValidate = true;
    formularioFiltros.addEventListener("submit", (evento) => {
      evento.preventDefault();
      pintar();
    });
  }

  if (formularioAprobar) {
    formularioAprobar.noValidate = true;
    formularioAprobar.addEventListener("submit", async (evento) => {
      evento.preventDefault();
      const mensaje = document.querySelector('[data-js="mensaje-aprobacion"]');
      limpiar(mensaje);
      if (idSeleccionada === null) {
        return;
      }
      try {
        await pedir("POST", `/api/permisos/${idSeleccionada}/aprobar`);
        location.hash = "";
        await cargar();
        mostrarMensaje(mensajePagina, "Solicitud aprobada.", "exito");
      } catch (error) {
        mostrarMensaje(mensaje, error.message, "error");
      }
    });
  }

  if (formularioRechazar) {
    formularioRechazar.noValidate = true;
    formularioRechazar.addEventListener("submit", async (evento) => {
      evento.preventDefault();
      const mensaje = document.querySelector('[data-js="mensaje-rechazo"]');
      limpiar(mensaje);
      if (idSeleccionada === null) {
        return;
      }
      const motivo = String(formularioRechazar.elements.motivo?.value ?? "").trim();
      if (motivo.length === 0) {
        mostrarMensaje(mensaje, "Escribe el motivo del rechazo.", "error");
        return;
      }
      try {
        await pedir("POST", `/api/permisos/${idSeleccionada}/rechazar`, {
          motivo,
        });
        formularioRechazar.reset();
        location.hash = "";
        await cargar();
        mostrarMensaje(mensajePagina, "Solicitud rechazada.", "exito");
      } catch (error) {
        mostrarMensaje(mensaje, error.message, "error");
      }
    });
  }

  await cargar();
}

function crearFila(solicitud, molde, nombre, alElegir) {
  const fila = molde ? molde.cloneNode(true) : document.createElement("tr");
  quitarEjemplo(fila);

  const id = solicitud?.id;
  const celdas = fila.querySelectorAll("td");
  if (celdas[0]) {
    celdas[0].textContent = nombre;
  }
  if (celdas[1]) {
    celdas[1].textContent = tipoLegible(solicitud?.tipoPermiso);
  }
  if (celdas[2]) {
    celdas[2].textContent = fechaLegible(solicitud?.fechaCreacion);
  }

  const ver = fila.querySelector('[data-js="ver-solicitud"]');
  if (ver) {
    ver.id = `ver-solicitud-${id}`;
    ver.href = `#ventana-ver-solicitud-${id}`;
  }

  const aprobar = fila.querySelector('[data-js="aprobar-solicitud"]');
  if (aprobar) {
    aprobar.id = `aprobar-${id}`;
    aprobar.href = "#ventana-aprobar";
    aprobar.addEventListener("click", () => alElegir(id));
  }

  const rechazar = fila.querySelector('[data-js="rechazar-solicitud"]');
  if (rechazar) {
    rechazar.id = `rechazar-${id}`;
    rechazar.href = "#ventana-rechazar";
    rechazar.addEventListener("click", () => alElegir(id));
  }

  return fila;
}

function crearVentanaVer(solicitud, molde, nombre) {
  const ventana = molde ? molde.cloneNode(true) : document.createElement("div");
  quitarEjemplo(ventana);

  const id = solicitud?.id;
  const idVentana = `ventana-ver-solicitud-${id}`;
  ventana.id = idVentana;

  const idTitulo = `titulo-${idVentana}`;
  const caja = ventana.querySelector(".ventana-modal-caja");
  if (caja) {
    caja.setAttribute("aria-labelledby", idTitulo);
  }
  const titulo = ventana.querySelector(".ventana-modal-titulo");
  if (titulo) {
    titulo.id = idTitulo;
  }

  const datos = ventana.querySelectorAll(".ventana-modal-contenido span");
  const valores = [
    nombre,
    tipoLegible(solicitud?.tipoPermiso),
    fechaLegible(solicitud?.fechaCreacion),
  ];
  valores.forEach((texto, posicion) => {
    if (datos[posicion]) {
      datos[posicion].textContent = texto;
    }
  });

  return ventana;
}
