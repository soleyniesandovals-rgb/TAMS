import { pedir } from "./api.js";
import { usuarioActual } from "./marco.js";
import {
  limpiar,
  mostrarMensaje,
  quitarEjemplo,
  tomarMolde,
} from "./ui.js";

const usuario = await usuarioActual;
if (usuario) {
  await iniciarUsuarios();
}

async function iniciarUsuarios() {
  const principal = document.querySelector("main.contenido-app");
  const tbody = document.querySelector('[data-js="tabla-usuarios"] tbody');
  const mensajePagina = document.querySelector('[data-js="mensaje-pagina"]');
  const formulario = document.querySelector("#buscar-usuarios");
  const campoBusqueda = document.querySelector("#busqueda-usuario");

  if (!tbody) {
    return;
  }

  const moldes = {
    fila: tomarMolde(tbody, "tr"),
    rol: tomarMolde(principal, ".ventana-modal[id^='ventana-rol-']"),
    estado: tomarMolde(principal, ".ventana-modal[id^='ventana-estado-']"),
    recuperar: tomarMolde(principal, ".ventana-modal[id^='ventana-recuperar-']"),
    activacion: tomarMolde(principal, ".ventana-modal[id^='ventana-activacion-']"),
  };

  let usuarios = [];
  let textoBusqueda = "";

  async function cargar() {
    try {
      usuarios = await pedir("GET", "/api/cuentas");
      pintar();
    } catch (error) {
      mostrarMensaje(mensajePagina, error.message, "error");
    }
  }

  function pintar() {
    const abierta = location.hash.slice(1);
    quitarVentanas(principal);
    tbody.replaceChildren();

    const filtro = textoBusqueda.trim().toLowerCase();
    const visibles = usuarios.filter((item) => {
      const nombre = String(item?.nombre ?? "").toLowerCase();
      const correo = String(item?.correo ?? "").toLowerCase();
      return nombre.includes(filtro) || correo.includes(filtro);
    });

    const ventanas = [];
    for (const item of visibles) {
      const id = item?.id;
      tbody.append(crearFila(item, moldes.fila));
      ventanas.push(crearVentanaRol(item, moldes.rol, cargar, mensajePagina));
      ventanas.push(crearVentanaEstado(item, moldes.estado, cargar, mensajePagina));
      ventanas.push(
        crearVentanaRecuperar(item, moldes.recuperar, cargar, mensajePagina),
      );
      if (item?.activo === false) {
        ventanas.push(
          crearVentanaActivacion(item, moldes.activacion, cargar, mensajePagina),
        );
      }
    }
    principal.append(...ventanas);

    if (abierta && principal.querySelector(`[id="${abierta}"]`)) {
      history.replaceState(null, "", location.pathname + location.search);
      location.replace(`#${abierta}`);
    }
  }

  if (formulario) {
    formulario.noValidate = true;
    formulario.addEventListener("submit", (evento) => {
      evento.preventDefault();
      textoBusqueda = campoBusqueda?.value ?? "";
      pintar();
    });
  }

  await cargar();
}

function quitarVentanas(principal) {
  principal
    .querySelectorAll(
      ".ventana-modal[id^='ventana-rol-'], .ventana-modal[id^='ventana-estado-'], .ventana-modal[id^='ventana-recuperar-'], .ventana-modal[id^='ventana-activacion-']",
    )
    .forEach((ventana) => ventana.remove());
}

function rolLegible(rol) {
  return rol === "Estandar" ? "Docente" : "Administrador";
}

function claseRol(rol) {
  return rol === "Estandar" ? "rol-estandar" : "rol-administrador";
}

function pintarClase(elemento, prefijo, clase) {
  if (!elemento) {
    return;
  }
  for (const actual of Array.from(elemento.classList)) {
    if (actual.startsWith(`${prefijo}-`)) {
      elemento.classList.remove(actual);
    }
  }
  if (clase) {
    elemento.classList.add(clase);
  }
}

function crearFila(item, molde) {
  const fila = molde ? molde.cloneNode(true) : document.createElement("tr");
  quitarEjemplo(fila);

  const id = item?.id;
  const celdas = fila.querySelectorAll("td");
  if (celdas[0]) {
    celdas[0].textContent = String(id ?? "");
  }
  if (celdas[1]) {
    celdas[1].textContent = item?.nombre ?? "";
  }

  const correo = fila.querySelector('[data-js="enviar-codigo-recuperacion"]');
  if (correo) {
    correo.id = `recuperar-${id}`;
    correo.href = `#ventana-recuperar-${id}`;
    correo.textContent = item?.correo ?? "";
  }

  const rol = fila.querySelector('[data-js="cambiar-rol"]');
  if (rol) {
    rol.id = `rol-${id}`;
    rol.href = `#ventana-rol-${id}`;
    rol.textContent = rolLegible(item?.rol);
    pintarClase(rol, "rol", claseRol(item?.rol));
  }

  const estado = fila.querySelector('[data-js="cambiar-estado"]');
  if (estado) {
    estado.id = `estado-${id}`;
    estado.href = `#ventana-estado-${id}`;
    estado.textContent = item?.activo === true ? "Activo" : "Inactivo";
    pintarClase(
      estado,
      "estado",
      item?.activo === true ? "estado-activo" : "estado-inactivo",
    );
  }

  const activacion = fila.querySelector('[data-js="reenviar-activacion"]');
  if (activacion) {
    if (item?.activo === true) {
      activacion.remove();
    } else {
      activacion.id = `activar-${id}`;
      activacion.href = `#ventana-activacion-${id}`;
      activacion.textContent = "Reenviar activación";
    }
  }

  return fila;
}

function prepararVentana(ventana, id, prefijo) {
  quitarEjemplo(ventana);
  const idVentana = `ventana-${prefijo}-${id}`;
  ventana.id = idVentana;

  const idTitulo = `titulo-${idVentana}`;
  const caja = ventana.querySelector(".ventana-modal-caja");
  if (caja) {
    caja.setAttribute("aria-labelledby", idTitulo);
  }
  const titulo = ventana.querySelector(".ventana-modal-titulo");
  if (titulo) {
    titulo.id = idTitulo;
  }

  return ventana.querySelector('[data-js="mensaje-ventana"]');
}

function ponerNombre(ventana, nombre) {
  const huella = ventana.querySelector(".ventana-modal-contenido span");
  if (huella) {
    huella.textContent = nombre ?? "";
  }
}

function crearVentanaRol(item, molde, recargar, mensajePagina) {
  const ventana = molde ? molde.cloneNode(true) : document.createElement("div");
  const id = item?.id;
  const mensaje = prepararVentana(ventana, id, "rol");
  ponerNombre(ventana, item?.nombre);

  const selector = ventana.querySelector("select");
  if (selector) {
    selector.id = `nuevo-rol-${id}`;
    const etiqueta = ventana.querySelector('label[for^="nuevo-rol-"]');
    if (etiqueta) {
      etiqueta.setAttribute("for", selector.id);
    }
    selector.value = item?.rol ?? "Estandar";
  }

  const confirmar = ventana.querySelector('[data-js="confirmar-rol"]');
  if (confirmar) {
    confirmar.id = `confirmar-rol-${id}`;
    confirmar.addEventListener("click", async (evento) => {
      evento.preventDefault();
      limpiar(mensaje);
      try {
        await pedir("PUT", `/api/cuentas/${id}/rol`, { rol: selector?.value });
        location.hash = "";
        await recargar();
        mostrarMensaje(mensajePagina, "Rol actualizado.", "exito");
      } catch (error) {
        mostrarMensaje(mensaje, error.message, "error");
      }
    });
  }

  return ventana;
}

function crearVentanaEstado(item, molde, recargar, mensajePagina) {
  const ventana = molde ? molde.cloneNode(true) : document.createElement("div");
  const id = item?.id;
  const mensaje = prepararVentana(ventana, id, "estado");
  const activo = item?.activo === true;
  const parrafo = ventana.querySelector(".ventana-modal-contenido p");
  if (parrafo) {
    if (parrafo.firstChild?.nodeType === 3) {
      parrafo.firstChild.textContent = activo
        ? "¿Desactivar la cuenta de "
        : "¿Reactivar la cuenta de ";
    }
  }
  ponerNombre(ventana, item?.nombre);

  const confirmar = ventana.querySelector('[data-js="confirmar-estado"]');
  if (confirmar) {
    confirmar.id = `confirmar-estado-${id}`;
    confirmar.addEventListener("click", async (evento) => {
      evento.preventDefault();
      limpiar(mensaje);
      const accion = activo ? "desactivar" : "reactivar";
      try {
        await pedir("POST", `/api/cuentas/${id}/${accion}`);
        location.hash = "";
        await recargar();
        mostrarMensaje(
          mensajePagina,
          activo ? "Usuario desactivado." : "Usuario reactivado.",
          "exito",
        );
      } catch (error) {
        mostrarMensaje(mensaje, error.message, "error");
      }
    });
  }

  return ventana;
}

function crearVentanaRecuperar(item, molde, recargar, mensajePagina) {
  const ventana = molde ? molde.cloneNode(true) : document.createElement("div");
  const id = item?.id;
  const mensaje = prepararVentana(ventana, id, "recuperar");
  const datos = ventana.querySelectorAll(".ventana-modal-contenido span");
  if (datos[0]) {
    datos[0].textContent = item?.nombre ?? "";
  }
  if (datos[1]) {
    datos[1].textContent = item?.correo ?? "";
  }

  const confirmar = ventana.querySelector('[data-js="confirmar-codigo-recuperacion"]');
  if (confirmar) {
    confirmar.id = `confirmar-recuperacion-${id}`;
    confirmar.addEventListener("click", async (evento) => {
      evento.preventDefault();
      limpiar(mensaje);
      try {
        await pedir("POST", `/api/cuentas/${id}/restablecer`);
        location.hash = "";
        await recargar();
        mostrarMensaje(mensajePagina, "Código de recuperación enviado.", "exito");
      } catch (error) {
        mostrarMensaje(mensaje, error.message, "error");
      }
    });
  }

  return ventana;
}

function crearVentanaActivacion(item, molde, recargar, mensajePagina) {
  const ventana = molde ? molde.cloneNode(true) : document.createElement("div");
  const id = item?.id;
  const mensaje = prepararVentana(ventana, id, "activacion");
  ponerNombre(ventana, item?.nombre);

  const confirmar = ventana.querySelector('[data-js="confirmar-activacion"]');
  if (confirmar) {
    confirmar.id = `confirmar-activacion-${id}`;
    confirmar.addEventListener("click", async (evento) => {
      evento.preventDefault();
      limpiar(mensaje);
      try {
        await pedir("POST", "/api/cuentas/reenviar", { correo: item?.correo });
        location.hash = "";
        await recargar();
        mostrarMensaje(mensajePagina, "Enlace de activación reenviado.", "exito");
      } catch (error) {
        mostrarMensaje(mensaje, error.message, "error");
      }
    });
  }

  return ventana;
}
