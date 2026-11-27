import { borrarSesion, obtenerToken } from "./sesion.js";

function textoUtil(candidato) {
  return typeof candidato === "string" && candidato.trim().length > 0
    ? candidato
    : null;
}

/** RF-DOC-03: lee el nombre de la cabecera Content-Disposition sin interpretar HTML. */
function nombreDescargado(disposicion) {
  if (!disposicion) {
    return "documento";
  }

  const partes = disposicion.split(";").map((parte) => parte.trim());
  const conCodificacion = partes.find((parte) => /^filename\*=/i.test(parte));
  if (conCodificacion) {
    const valor = conCodificacion
      .slice(conCodificacion.indexOf("=") + 1)
      .trim()
      .replace(/^"|"$/g, "");
    const codificado = valor.split("''").pop();
    try {
      return decodeURIComponent(codificado) || "documento";
    } catch {
      return codificado || "documento";
    }
  }

  const conNombre = partes.find((parte) => /^filename=/i.test(parte));
  if (conNombre) {
    const valor = conNombre
      .slice(conNombre.indexOf("=") + 1)
      .trim()
      .replace(/^"|"$/g, "");
    return valor || "documento";
  }

  return "documento";
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

/**
 * Descarga un archivo con el token de la sesión.
 * RD-08: un fallo se reduce al mensaje de la API; un 401 cierra la sesión.
 */
export async function descargar(ruta) {
  const cabeceras = {};
  const token = obtenerToken();

  if (token) {
    cabeceras.Authorization = `Bearer ${token}`;
  }

  const respuesta = await fetch(ruta, {
    method: "GET",
    headers: cabeceras,
  });

  if (!respuesta.ok) {
    const texto = await respuesta.text();
    let datos = null;

    if (texto) {
      try {
        datos = JSON.parse(texto);
      } catch {
        datos = null;
      }
    }

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

  const blob = await respuesta.blob();
  return {
    blob,
    nombre: nombreDescargado(respuesta.headers.get("Content-Disposition")),
  };
}
