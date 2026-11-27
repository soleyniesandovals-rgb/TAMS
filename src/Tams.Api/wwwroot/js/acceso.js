import { pedir } from "./api.js";
import { borrarSesion, guardarToken } from "./sesion.js";
import { limpiar, mostrarMensaje } from "./ui.js";

const contenedorLogin = document.querySelector("#mensaje-login");
const textoLogin = document.querySelector('[data-js="mensaje-login-texto"]');
const parrafoActivacion = document.querySelector('[data-js="mensaje-login-activacion"]');
const enlaceReenviar = document.querySelector('[data-js="reenviar-activacion"]');
const mensajeRegistro = document.querySelector("#mensaje-registro");
const formularioLogin = document.querySelector('[data-js="login"]');
const formularioRegistro = document.querySelector('[data-js="registro"]');
const botonLogin = formularioLogin.querySelector('button[type="submit"]');
const botonRegistro = formularioRegistro.querySelector('button[type="submit"]');

function ocultarMensajes() {
  limpiar(textoLogin);
  limpiar(mensajeRegistro);
  parrafoActivacion.hidden = true;
  contenedorLogin.hidden = true;
}

function mostrarEnLogin(texto, tipo, cuentaSinActivar = false) {
  ocultarMensajes();
  contenedorLogin.classList.remove("mensaje-exito", "mensaje-error");
  contenedorLogin.classList.add(tipo === "exito" ? "mensaje-exito" : "mensaje-error");
  mostrarMensaje(textoLogin, texto, tipo);
  parrafoActivacion.hidden = !cuentaSinActivar;
  contenedorLogin.hidden = false;
}

function mostrarEnRegistro(texto, tipo) {
  ocultarMensajes();
  mostrarMensaje(mensajeRegistro, texto, tipo);
}

async function iniciarSesion(evento) {
  evento.preventDefault();
  ocultarMensajes();

  const correo = document.querySelector("#login-correo").value;
  const contrasena = document.querySelector("#login-contrasena").value;

  botonLogin.disabled = true;
  try {
    const sesion = await pedir("POST", "/api/cuentas/login", { correo, contrasena });
    guardarToken(sesion.token);

    const usuario = await pedir("GET", "/api/cuentas/yo");
    const inicio = usuario.rol === "Administrador"
      ? "/admin/inicio.html"
      : "/docente/inicio.html";
    location.assign(inicio);
  } catch (error) {
    borrarSesion();
    mostrarEnLogin(error.message, "error", error.status === 403);
  } finally {
    botonLogin.disabled = false;
  }
}

async function reenviarActivacion(evento) {
  evento.preventDefault();
  ocultarMensajes();

  const correo = document.querySelector("#login-correo").value.trim();
  if (!correo) {
    mostrarEnLogin("Escribe tu correo para reenviar la activación.", "error");
    return;
  }

  try {
    const respuesta = await pedir("POST", "/api/cuentas/reenviar", { correo });
    mostrarEnLogin(respuesta.mensaje, "exito");
  } catch (error) {
    mostrarEnLogin(error.message, "error");
  }
}

async function registrar(evento) {
  evento.preventDefault();
  ocultarMensajes();

  const nombre = document.querySelector("#registro-nombre").value;
  const correo = document.querySelector("#registro-correo").value;
  const contrasena = document.querySelector("#registro-contrasena").value;
  const repeticion = document.querySelector("#registro-repetir").value;

  if (contrasena !== repeticion) {
    mostrarEnRegistro("Las contraseñas no coinciden.", "error");
    return;
  }

  botonRegistro.disabled = true;
  try {
    const respuesta = await pedir("POST", "/api/cuentas/registro", {
      nombre,
      correo,
      contrasena,
    });
    mostrarEnRegistro(respuesta.mensaje, "exito");
    formularioRegistro.reset();
  } catch (error) {
    mostrarEnRegistro(error.message, "error");
  } finally {
    botonRegistro.disabled = false;
  }
}

async function activarDesdeEnlace() {
  const codigo = new URLSearchParams(location.search).get("activacion");
  if (codigo === null) {
    return;
  }

  ocultarMensajes();

  try {
    const respuesta = await pedir(
      "GET",
      `/api/cuentas/activar?codigo=${encodeURIComponent(codigo)}`,
    );
    mostrarEnLogin(respuesta.mensaje, "exito");
  } catch (error) {
    mostrarEnLogin(error.message, "error");
  } finally {
    history.replaceState(null, "", location.pathname + location.hash);
  }
}

formularioLogin.addEventListener("submit", iniciarSesion);
formularioRegistro.addEventListener("submit", registrar);
enlaceReenviar.addEventListener("click", reenviarActivacion);
activarDesdeEnlace();
