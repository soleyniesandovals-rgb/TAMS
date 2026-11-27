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
  const lista = document.querySelector('[data-js="lista-solicitudes"]');
  const mensajePagina = document.querySelector('[data-js="mensaje-pagina"]');
  const mensajeVacio = document.querySelector(
    '[data-js="mensaje-sin-solicitudes"]',
  );
  const mensajeNueva = document.querySelector(
    '[data-js="mensaje-nueva-solicitud"]',
  );
  const formulario = document.querySelector('[data-js="crear-solicitud"]');
  const boton = document.querySelector('[data-js="enviar-solicitud"]');

  if (!lista) {
    return;
  }

  const moldeTarjeta = tomarMolde(lista, "a[data-js='ver-solicitud']");
  const moldeVentana = tomarMolde(
    principal,
    ".ventana-modal[id^='ventana-solicitud-']",
  );

  await cargarLista(lista, moldeTarjeta, moldeVentana, mensajePagina, mensajeVacio);

  if (formulario) {
    // Sin esto, el atributo required del select bloquearía el mensaje controlado.
    formulario.noValidate = true;
    formulario.addEventListener("submit", async (evento) => {
      evento.preventDefault();
      limpiar(mensajeNueva);
      limpiar(mensajePagina);

      const tipo = formulario.elements.tipo?.value ?? "";
      if (!tipo) {
        mostrarMensaje(mensajeNueva, "Elige un tipo de permiso.", "error");
        return;
      }

      if (boton) {
        boton.disabled = true;
      }

      try {
        await pedir("POST", "/api/permisos", { tipoPermiso: tipo });
        formulario.reset();
        location.hash = "";
        await cargarLista(
          lista,
          moldeTarjeta,
          moldeVentana,
          mensajePagina,
          mensajeVacio,
        );
        mostrarMensaje(mensajePagina, "Solicitud enviada.", "exito");
      } catch (error) {
        mostrarMensaje(mensajeNueva, error.message, "error");
      } finally {
        if (boton) {
          boton.disabled = false;
        }
      }
    });
  }
}

async function cargarLista(lista, moldeTarjeta, moldeVentana, mensajePagina, mensajeVacio) {
  limpiar(mensajePagina);
  lista.replaceChildren();
  document
    .querySelectorAll(".ventana-modal[id^='ventana-solicitud-']")
    .forEach((ventana) => ventana.remove());

  if (mensajeVacio) {
    mensajeVacio.hidden = true;
  }

  try {
    const solicitudes = await pedir("GET", "/api/permisos");

    if (!Array.isArray(solicitudes) || solicitudes.length === 0) {
      if (mensajeVacio) {
        mensajeVacio.hidden = false;
      }
      return;
    }

    const ventanas = [];
    for (const solicitud of solicitudes) {
      lista.append(crearTarjeta(solicitud, moldeTarjeta));
      ventanas.push(crearVentana(solicitud, moldeVentana));
    }
    lista.after(...ventanas);
  } catch (error) {
    mostrarMensaje(mensajePagina, error.message, "error");
  }
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

function claseEstado(estado) {
  if (estado === "Pendiente") {
    return "estado-pendiente";
  }
  if (estado === "Rechazada") {
    return "estado-rechazado";
  }
  if (estado === "Aprobada" || estado === "Aplicada") {
    return "estado-activo";
  }
  return "";
}

function pintarEtiqueta(etiqueta, estado) {
  if (!etiqueta) {
    return;
  }

  etiqueta.textContent = String(estado ?? "");
  for (const clase of Array.from(etiqueta.classList)) {
    if (clase.startsWith("estado-")) {
      etiqueta.classList.remove(clase);
    }
  }

  const clase = claseEstado(estado);
  if (clase) {
    etiqueta.classList.add(clase);
  }
}

function motivoRechazoDe(solicitud) {
  const motivo = solicitud?.motivoRechazo;
  if (typeof motivo !== "string" || motivo.trim().length === 0) {
    return "";
  }
  return motivo;
}

function formatearFecha(fecha) {
  return fecha ? new Date(fecha).toLocaleDateString("es-DO") : "";
}

function crearTarjeta(solicitud, molde) {
  const tarjeta = molde ? molde.cloneNode(true) : document.createElement("a");
  quitarEjemplo(tarjeta);

  const id = solicitud?.id;
  const estado = solicitud?.estado ?? "";
  const motivo = motivoRechazoDe(solicitud);

  tarjeta.id = `ver-solicitud-${id}`;
  tarjeta.href = `#ventana-solicitud-${id}`;

  const pestana = tarjeta.querySelector(".tarjeta-carpeta-pestana");
  if (pestana) {
    pestana.textContent = tipoLegible(solicitud?.tipoPermiso);
  }

  pintarEtiqueta(tarjeta.querySelector(".etiqueta-estado"), estado);

  const datos = tarjeta.querySelectorAll(".tarjeta-solicitud-datos > span");
  if (datos[1]) {
    datos[1].textContent = `Enviada el ${formatearFecha(solicitud?.fechaCreacion)}`;
  }

  const motivoSpan = tarjeta.querySelector(".tarjeta-solicitud-motivo");
  if (motivoSpan) {
    if (estado === "Rechazada" && motivo) {
      motivoSpan.textContent = `Motivo del rechazo: ${motivo}`;
    } else {
      motivoSpan.remove();
    }
  }

  return tarjeta;
}

function crearVentana(solicitud, molde) {
  const ventana = molde ? molde.cloneNode(true) : document.createElement("div");
  quitarEjemplo(ventana);

  const id = solicitud?.id;
  const estado = solicitud?.estado ?? "";
  const motivo = motivoRechazoDe(solicitud);
  const idVentana = `ventana-solicitud-${id}`;
  const idTitulo = `titulo-ventana-solicitud-${id}`;

  ventana.id = idVentana;
  const caja = ventana.querySelector(".ventana-modal-caja");
  if (caja) {
    caja.setAttribute("aria-labelledby", idTitulo);
  }

  const titulo = ventana.querySelector(".ventana-modal-titulo");
  if (titulo) {
    titulo.id = idTitulo;
  }

  const detalles = ventana.querySelector(".lista-detalles");
  const casillas = detalles?.querySelectorAll("dd");
  const etiquetas = detalles?.querySelectorAll("dt");

  if (casillas?.[0]) {
    casillas[0].textContent = tipoLegible(solicitud?.tipoPermiso);
  }
  pintarEtiqueta(casillas?.[1]?.querySelector(".etiqueta-estado"), estado);
  if (casillas?.[2]) {
    casillas[2].textContent = formatearFecha(solicitud?.fechaCreacion);
  }

  if (etiquetas?.[3] && casillas?.[3]) {
    if (estado === "Rechazada" && motivo) {
      etiquetas[3].textContent = "Motivo del rechazo";
      casillas[3].textContent = motivo;
    } else {
      etiquetas[3].remove();
      casillas[3].remove();
    }
  }

  return ventana;
}
