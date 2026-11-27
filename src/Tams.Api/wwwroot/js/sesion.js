const CLAVE_TOKEN = "tams.token";

/** Guarda el token de acceso solo en la sesión del navegador. */
export function guardarToken(token) {
  sessionStorage.setItem(CLAVE_TOKEN, token);
}

/** Devuelve el token vigente, o null si no hay sesión iniciada. */
export function obtenerToken() {
  return sessionStorage.getItem(CLAVE_TOKEN);
}

/** Elimina el token de la sesión del navegador. */
export function borrarSesion() {
  sessionStorage.removeItem(CLAVE_TOKEN);
}
