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
