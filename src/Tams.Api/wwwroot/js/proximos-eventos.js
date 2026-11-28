import { pedir } from "./api.js";
import { limpiar, mostrarMensaje, quitarEjemplo, tomarMolde } from "./ui.js";

const MESES = {
  "01": "ENE",
  "02": "FEB",
  "03": "MAR",
  "04": "ABR",
  "05": "MAY",
  "06": "JUN",
  "07": "JUL",
  "08": "AGO",
  "09": "SEP",
  "10": "OCT",
  "11": "NOV",
  "12": "DIC",
};

/**
 * Pinta hasta cinco eventos próximos usando el li de ejemplo como molde.
 * La fecha llega como texto aaaa-mm-dd: se parte con split para no cambiar
 * de día por la zona horaria del navegador.
 */
export async function cargarProximosEventos(lista, mensajeVacio) {
  if (!lista) {
    return;
  }

  const molde = tomarMolde(lista, "li.evento");
  limpiar(mensajeVacio);

  try {
    const respuesta = await pedir("GET", "/api/eventos/proximos");
    const elementos = Array.isArray(respuesta?.elementos)
      ? respuesta.elementos
      : [];

    if (elementos.length === 0) {
      if (mensajeVacio) {
        mensajeVacio.hidden = false;
      }
      return;
    }

    for (const elemento of elementos.slice(0, 5)) {
      const fila = molde ? molde.cloneNode(true) : document.createElement("li");
      quitarEjemplo(fila);
      fila.classList.add("evento");

      const partes = String(elemento?.fecha ?? "").split("-");
      const dia = fila.querySelector(".evento-dia");
      if (dia) {
        dia.textContent = String(Number(partes[2]));
      }

      const mes = fila.querySelector(".evento-mes");
      if (mes) {
        mes.textContent = MESES[partes[1]] ?? "";
      }

      const titulo = fila.querySelector(".evento-titulo");
      if (titulo) {
        titulo.textContent = elemento?.descripcion ?? "";
      }

      const detalle = fila.querySelector(".evento-detalle");
      if (detalle) {
        detalle.textContent =
          elemento?.tipo === "Feriado"
            ? "Feriado nacional"
            : "Actividad registrada";
      }

      lista.append(fila);
    }
  } catch (error) {
    mostrarMensaje(mensajeVacio, error.message, "error");
  }
}
