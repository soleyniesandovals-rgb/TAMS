import { pedir } from "./api.js";
import { usuarioActual } from "./marco.js";
import { limpiar, mostrarMensaje } from "./ui.js";

const usuario = await usuarioActual;
if (usuario) {
  await iniciarMiCuenta(usuario);
}

async function iniciarMiCuenta(usuario) {
  const mensajeCuenta = document.querySelector('[data-js="mensaje-cuenta"]');
  const mensajeCambiar = document.querySelector(
    '[data-js="mensaje-cambiar-contrasena"]',
  );

  rellenarDatos(usuario);
  await cargarMaterias(mensajeCuenta);
  conectarCambioContrasena(mensajeCuenta, mensajeCambiar);
}

function rellenarDatos(usuario) {
  const nombre = (usuario.nombre ?? "").trim();
  const datos = [
    [".inicial-grande", nombre.charAt(0)],
    [".cuenta-rol", usuario.rol === "Administrador" ? "Administrador" : "Docente"],
    [".nombre-cuenta", nombre],
    [".cuenta-correo", usuario.correo ?? ""],
  ];

  for (const [selector, texto] of datos) {
    const elemento = document.querySelector(selector);
    if (!elemento) {
      continue;
    }

    elemento.textContent = texto;
    elemento.removeAttribute("data-ejemplo");
  }
}

async function cargarMaterias(mensajeCuenta) {
  const lista = document.querySelector('[data-js="materias-imparte"]');
  if (!lista) {
    return;
  }

  const molde = lista.querySelector("li[data-ejemplo]")?.cloneNode(true);
  lista.querySelectorAll("li[data-ejemplo]").forEach((fila) => fila.remove());

  try {
    const asignaciones = await pedir("GET", "/api/docente/asignaciones");
    const agrupadas = new Map();

    for (const asignacion of Array.isArray(asignaciones) ? asignaciones : []) {
      const materia = asignacion?.materiaNombre;
      if (typeof materia !== "string" || materia.length === 0) {
        continue;
      }

      if (!agrupadas.has(materia)) {
        agrupadas.set(materia, []);
      }

      const curso = `${asignacion.grado ?? ""} ${asignacion.seccion ?? ""}`.trim();
      const cursos = agrupadas.get(materia);
      if (curso && !cursos.includes(curso)) {
        cursos.push(curso);
      }
    }

    if (agrupadas.size === 0) {
      anadirFila(lista, molde, "Sin asignaciones todavía.");
      return;
    }

    for (const [materia, cursos] of agrupadas) {
      anadirFila(lista, molde, `${materia} · ${cursos.join(", ")}`);
    }
  } catch (error) {
    mostrarMensaje(mensajeCuenta, error.message, "error");
  }
}

function anadirFila(lista, molde, texto) {
  const fila = molde ? molde.cloneNode(true) : document.createElement("li");
  fila.removeAttribute("data-ejemplo");
  fila.querySelectorAll("[data-ejemplo]").forEach((elemento) => {
    elemento.removeAttribute("data-ejemplo");
  });
  fila.textContent = texto;
  lista.append(fila);
}

function conectarCambioContrasena(mensajeCuenta, mensajeCambiar) {
  const formulario = document.querySelector(
    '[data-js="form-cambiar-contrasena"]',
  );
  if (!formulario) {
    return;
  }

  const boton = document.querySelector('[data-js="guardar-contrasena"]');

  formulario.addEventListener("submit", async (evento) => {
    evento.preventDefault();
    limpiar(mensajeCuenta);
    limpiar(mensajeCambiar);

    const actual = formulario.elements.actual?.value ?? "";
    const nueva = formulario.elements.nueva?.value ?? "";
    const repetir = formulario.elements.repetir?.value ?? "";

    if (nueva !== repetir) {
      mostrarMensaje(mensajeCambiar, "Las contraseñas no coinciden.", "error");
      return;
    }

    if (boton) {
      boton.disabled = true;
    }

    try {
      await pedir("POST", "/api/cuentas/cambiar-contrasena", {
        contrasenaActual: actual,
        contrasenaNueva: nueva,
      });

      formulario.reset();
      location.hash = "";
      mostrarMensaje(mensajeCuenta, "Contraseña cambiada.", "exito");
    } catch (error) {
      mostrarMensaje(mensajeCambiar, error.message, "error");
    } finally {
      if (boton) {
        boton.disabled = false;
      }
    }
  });
}
