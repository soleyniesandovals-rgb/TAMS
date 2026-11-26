import { pedir } from "./api.js";
import { borrarSesion, obtenerToken } from "./sesion.js";

/**
 * Marco común de las pantallas privadas.
 * RD-06: el rol solo se usa para navegar; la autorización real la decide la API.
 */
async function iniciarMarco() {
  if (!obtenerToken()) {
    location.assign("/index.html");
    return null;
  }

  let usuario;
  try {
    usuario = await pedir("GET", "/api/cuentas/yo");
  } catch {
    // api.js ya borró la sesión y redirigió cuando la respuesta fue 401.
    return null;
  }

  const ruta = location.pathname;
  const inicioDelRol =
    usuario?.rol === "Administrador"
      ? "/admin/inicio.html"
      : "/docente/inicio.html";

  if (
    (ruta.includes("/admin/") && usuario?.rol !== "Administrador") ||
    (ruta.includes("/docente/") && usuario?.rol !== "Estandar")
  ) {
    location.assign(inicioDelRol);
    return null;
  }

  aplicarUsuario(usuario);

  try {
    await actualizarContador();
  } catch {
    // Un fallo al contar no debe impedir el resto del marco.
  }

  conectarCierreSesion();
  return usuario;
}

function aplicarUsuario(usuario) {
  const contenedor = document.querySelector('[data-js="usuario-actual"]');
  if (!contenedor) {
    return;
  }

  const inicial = contenedor.querySelector(".inicial-usuario");
  if (inicial) {
    inicial.textContent = (usuario?.nombre ?? "").trim().charAt(0);
    inicial.removeAttribute("data-ejemplo");
  }

  const correo = contenedor.querySelector(".correo-usuario");
  if (correo) {
    correo.textContent = usuario?.correo ?? "";
    correo.removeAttribute("data-ejemplo");
  }
}

async function actualizarContador() {
  const notificaciones = await pedir("GET", "/api/notificaciones");
  const sinLeer = Array.isArray(notificaciones)
    ? notificaciones.filter(
        (notificacion) => notificacion?.leida === false,
      ).length
    : 0;

  const enlace = document.querySelector('[data-js="contador-notificaciones"]');
  if (!enlace) {
    return;
  }

  const span = enlace.querySelector(".contador-notificaciones");
  if (span) {
    span.textContent = String(sinLeer);
    span.hidden = sinLeer === 0;
  }

  enlace.setAttribute("aria-label", `Notificaciones (${sinLeer} sin leer)`);
}

function conectarCierreSesion() {
  const enlace = document.querySelector('[data-js="cerrar-sesion"]');
  if (!enlace) {
    return;
  }

  enlace.addEventListener("click", async (evento) => {
    evento.preventDefault();

    try {
      await pedir("POST", "/api/cuentas/logout");
    } catch {
      // Aun si la API falla, la sesión local se cierra igual.
    }

    borrarSesion();
    location.assign("/index.html");
  });
}

/** Los bloques siguientes pueden reutilizar el usuario sin volver a pedirlo. */
export const usuarioActual = iniciarMarco();
