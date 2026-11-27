const CLASES_TIPO = {
  exito: "mensaje-exito",
  error: "mensaje-error",
};

/** Muestra un aviso usando siempre textContent: nunca se interpreta HTML. */
export function mostrarMensaje(elemento, texto, tipo) {
  if (!elemento) {
    return;
  }

  elemento.textContent = texto;
  elemento.classList.remove("mensaje-exito", "mensaje-error");

  if (tipo) {
    elemento.dataset.tipoMensaje = String(tipo);
    const clase = CLASES_TIPO[String(tipo).toLowerCase()];
    if (clase) {
      elemento.classList.add(clase);
    }
  } else {
    delete elemento.dataset.tipoMensaje;
  }

  elemento.hidden = false;
}

/** Vacía un aviso y lo oculta. */
export function limpiar(elemento) {
  if (!elemento) {
    return;
  }

  elemento.textContent = "";
  elemento.classList.remove("mensaje-exito", "mensaje-error");
  delete elemento.dataset.tipoMensaje;
  elemento.hidden = true;
}

/** Quita data-ejemplo del elemento y de todos sus descendientes. */
export function quitarEjemplo(elemento) {
  if (!elemento) {
    return;
  }

  elemento.removeAttribute("data-ejemplo");
  elemento.querySelectorAll("[data-ejemplo]").forEach((hijo) => {
    hijo.removeAttribute("data-ejemplo");
  });
}

/** Clona el primer elemento que cumple el selector y borra todos los que cumplan. */
export function tomarMolde(contenedor, selector) {
  if (!contenedor) {
    return undefined;
  }

  const molde = contenedor.querySelector(selector)?.cloneNode(true);
  contenedor.querySelectorAll(selector).forEach((elemento) => {
    elemento.remove();
  });

  return molde;
}
