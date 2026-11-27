/** Muestra un aviso usando siempre textContent: nunca se interpreta HTML. */
export function mostrarMensaje(elemento, texto, tipo) {
  if (!elemento) {
    return;
  }

  elemento.textContent = texto;
  if (tipo) {
    elemento.dataset.tipoMensaje = String(tipo);
  }
  elemento.hidden = false;
}

/** Vacía un aviso y lo oculta. */
export function limpiar(elemento) {
  if (!elemento) {
    return;
  }

  elemento.textContent = "";
  delete elemento.dataset.tipoMensaje;
  elemento.hidden = true;
}
