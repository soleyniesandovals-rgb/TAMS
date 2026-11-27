import { pedir } from "./api.js";
import { actualizarContador, usuarioActual } from "./marco.js";
import { limpiar, mostrarMensaje } from "./ui.js";

const usuario = await usuarioActual;
if (usuario) {
  await iniciarNotificaciones();
}

async function iniciarNotificaciones() {
  const lista = document.querySelector('[data-js="lista-notificaciones"]');
  const mensajeVacio = document.querySelector(
    '[data-js="mensaje-sin-notificaciones"]',
  );
  const formulario = document.querySelector(
    '[data-js="preferencias-notificaciones"]',
  );
  const mensajePreferencias = document.querySelector(
    '[data-js="mensaje-preferencias"]',
  );

  if (!lista) {
    return;
  }

  // El molde se toma antes de borrar las filas de ejemplo y se reutiliza al recargar.
  const molde = lista.querySelector("li.notificacion")?.cloneNode(true);
  const textoVacio = mensajeVacio?.textContent ?? "No tienes notificaciones.";

  await cargarLista(lista, molde, mensajeVacio, textoVacio);

  if (formulario) {
    await cargarPreferencias(formulario, mensajePreferencias);
    conectarPreferencias(formulario, mensajePreferencias);
  }
}

async function cargarLista(lista, molde, mensajeVacio, textoVacio) {
  try {
    const notificaciones = await pedir("GET", "/api/notificaciones");
    lista.querySelectorAll("li").forEach((fila) => fila.remove());

    if (!Array.isArray(notificaciones) || notificaciones.length === 0) {
      if (mensajeVacio) {
        mensajeVacio.textContent = textoVacio;
        mensajeVacio.classList.remove("mensaje-exito", "mensaje-error");
        mensajeVacio.hidden = false;
      }
      return;
    }

    if (mensajeVacio) {
      mensajeVacio.hidden = true;
    }

    for (const notificacion of notificaciones) {
      lista.append(crearFila(notificacion, molde, lista, mensajeVacio, textoVacio));
    }
  } catch (error) {
    if (mensajeVacio) {
      mostrarMensaje(mensajeVacio, error.message, "error");
    }
  }
}

function crearFila(notificacion, molde, lista, mensajeVacio, textoVacio) {
  const fila = molde ? molde.cloneNode(true) : document.createElement("li");
  fila.removeAttribute("data-ejemplo");
  fila.querySelectorAll("[data-ejemplo]").forEach((elemento) => {
    elemento.removeAttribute("data-ejemplo");
  });

  const sinLeer = notificacion?.leida === false;
  fila.classList.toggle("no-leida", sinLeer);

  const titulo = notificacion?.titulo ?? "";
  const mensaje = notificacion?.mensaje ?? "";
  const texto = fila.querySelector("span > span");
  if (texto) {
    texto.textContent = `${titulo}: ${mensaje}`;
  }

  const fecha = fila.querySelector(".notificacion-fecha");
  if (fecha) {
    const fechaCreacion = notificacion?.fechaCreacion;
    const fechaTexto = fechaCreacion
      ? new Date(fechaCreacion).toLocaleDateString("es-DO")
      : "";
    fecha.textContent = `${fechaTexto} · ${sinLeer ? "No leída" : "Leída"}`;
  }

  const enlace = fila.querySelector('[data-js="marcar-leida"]');
  if (!sinLeer) {
    enlace?.remove();
  } else if (enlace) {
    const id = notificacion?.id;
    enlace.id = `marcar-leida-${id}`;
    enlace.href = "#";
    enlace.addEventListener("click", async (evento) => {
      evento.preventDefault();

      try {
        await pedir("POST", `/api/notificaciones/${id}/leida`);
        await cargarLista(lista, molde, mensajeVacio, textoVacio);
        await actualizarContador();
      } catch (error) {
        if (mensajeVacio) {
          mostrarMensaje(mensajeVacio, error.message, "error");
        }
      }
    });
  }

  return fila;
}

async function cargarPreferencias(formulario, mensajePreferencias) {
  const casilla = formulario.elements.correoActivo;
  if (!casilla) {
    return;
  }

  try {
    const preferencia = await pedir("GET", "/api/notificaciones/preferencias");
    casilla.checked = preferencia?.correoActivo === true;
  } catch (error) {
    mostrarMensaje(mensajePreferencias, error.message, "error");
  }
}

function conectarPreferencias(formulario, mensajePreferencias) {
  formulario.addEventListener("submit", async (evento) => {
    evento.preventDefault();
    limpiar(mensajePreferencias);

    const casilla = formulario.elements.correoActivo;
    try {
      await pedir("PUT", "/api/notificaciones/preferencias", {
        correoActivo: casilla.checked,
      });
      mostrarMensaje(mensajePreferencias, "Preferencia guardada.", "exito");
    } catch (error) {
      mostrarMensaje(mensajePreferencias, error.message, "error");
    }
  });
}
