import { borrarSesion, obtenerToken } from "./sesion.js";

function textoUtil(candidato) {
  return typeof candidato === "string" && candidato.trim().length > 0
    ? candidato
    : null;
}

/**
 * Llama a un endpoint relativo al mismo origen y devuelve su JSON.
 * RD-08: los errores se reducen al mensaje que envió la API, sin detalles internos.
 */
export async function pedir(metodo, ruta, cuerpo) {
  const cabeceras = {};
  const token = obtenerToken();

  if (token) {
    cabeceras.Authorization = `Bearer ${token}`;
  }

  let cuerpoFetch;
  if (cuerpo instanceof FormData) {
    cuerpoFetch = cuerpo;
  } else if (cuerpo !== undefined && cuerpo !== null) {
    cabeceras["Content-Type"] = "application/json";
    cuerpoFetch = JSON.stringify(cuerpo);
  }

  const respuesta = await fetch(ruta, {
    method: metodo,
    headers: cabeceras,
    body: cuerpoFetch,
  });

  const texto = await respuesta.text();
  let datos = null;

  if (texto) {
    try {
      datos = JSON.parse(texto);
    } catch {
      datos = null;
    }
  }

  if (!respuesta.ok) {
    const mensaje =
      textoUtil(datos?.mensaje) ??
      textoUtil(datos?.title) ??
      "No se pudo completar la operación.";
    const error = new Error(mensaje);
    error.status = respuesta.status;

    if (respuesta.status === 401 && !ruta.startsWith("/api/cuentas/login")) {
      borrarSesion();
      location.assign("/index.html");
    }

    throw error;
  }

  return datos;
}
