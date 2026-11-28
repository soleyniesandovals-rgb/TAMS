import { pedir } from "./api.js";
import { usuarioActual } from "./marco.js";
import {
  limpiar,
  mostrarMensaje,
  quitarEjemplo,
  tomarMolde,
} from "./ui.js";

const ESTADOS = {
  Borrador: { texto: "Borrador", clase: "estado-pendiente" },
  Enviada: { texto: "Enviada", clase: "estado-activo" },
  Publicada: { texto: "Publicada", clase: "estado-activo" },
  EnRecuperacion: { texto: "En recuperación", clase: "estado-rechazado" },
  Cerrada: { texto: "Cerrada", clase: "estado-inactivo" },
};
const SIN_NOTA = { texto: "Sin nota", clase: "estado-inactivo" };

const usuario = await usuarioActual;
if (usuario) {
  await iniciar();
}

async function iniciar() {
  const principal = document.querySelector("main.contenido-app");
  if (!principal) {
    return;
  }

  const mensajePagina = principal.querySelector('[data-js="mensaje-pagina"]');
  const tbody = principal.querySelector("#tabla-calificar tbody");
  const titulos = principal.querySelectorAll("h1.titulo-pagina span");
  const enviarTodas = principal.querySelector('[data-js="enviar-todas"]');
  const publicarTodas = principal.querySelector('[data-js="publicar-todas"]');
  const confirmarEnvio = principal.querySelector(
    '[data-js="confirmar-envio-todas"]',
  );
  const confirmarPublicacion = principal.querySelector(
    '[data-js="confirmar-publicacion-todas"]',
  );
  const hrefEnvio = enviarTodas?.getAttribute("href");
  const hrefPublicacion = publicarTodas?.getAttribute("href");
  const moldeFila = tomarMolde(tbody, "tr");
  const moldeVentana = tomarMolde(
    principal,
    '.ventana-modal[id^="ventana-calificar-"]',
  );

  function fijarEnlace(enlace, habilitado, href) {
    if (!enlace) {
      return;
    }
    if (habilitado) {
      if (href) {
        enlace.setAttribute("href", href);
      }
      enlace.removeAttribute("aria-disabled");
      enlace.removeAttribute("tabindex");
    } else {
      enlace.removeAttribute("href");
      enlace.setAttribute("aria-disabled", "true");
      enlace.setAttribute("tabindex", "-1");
    }
  }

  for (const [enlace, href] of [
    [enviarTodas, hrefEnvio],
    [publicarTodas, hrefPublicacion],
  ]) {
    enlace?.addEventListener("click", (evento) => {
      if (enlace.getAttribute("aria-disabled") === "true") {
        evento.preventDefault();
      }
    });
  }

  limpiar(mensajePagina);
  for (const titulo of titulos) {
    titulo.textContent = "";
    titulo.removeAttribute("data-ejemplo");
  }

  const crudo = new URLSearchParams(location.search).get("asignacionId");
  const asignacionId = Number(crudo);
  const idValido =
    crudo !== null &&
    /^\d+$/.test(crudo) &&
    Number.isSafeInteger(asignacionId) &&
    asignacionId > 0;

  if (!idValido) {
    fijarEnlace(enviarTodas, false, hrefEnvio);
    fijarEnlace(publicarTodas, false, hrefPublicacion);
    mostrarMensaje(mensajePagina, "La asignación no es válida.", "error");
    return;
  }

  let tablaActual = null;

  async function recargar(seguimiento) {
    try {
      const tabla = await pedir(
        "GET",
        `/api/docente/asignaciones/${asignacionId}/calificaciones`,
      );
      pintar(tabla);

      if (seguimiento) {
        location.hash = `ventana-calificar-${seguimiento.estudianteId}`;
        const ventana = principal.querySelector(
          `#ventana-calificar-${seguimiento.estudianteId}`,
        );
        const selector = ventana?.querySelector('[data-js="periodo"]');
        if (
          selector &&
          tabla?.columnas?.some((columna) => columna.numero === seguimiento.numero)
        ) {
          selector.value = String(seguimiento.numero);
          alCambiarColumna(ventana);
        }
        return ventana;
      }

      return null;
    } catch (error) {
      tablaActual = null;
      tbody.replaceChildren();
      for (const titulo of titulos) {
        titulo.textContent = "";
      }
      mostrarMensaje(mensajePagina, error.message, "error");
      return null;
    }
  }

  function pintar(tabla) {
    tablaActual = tabla;
    const columnas = Array.isArray(tabla?.columnas) ? tabla.columnas : [];
    const estudiantes = Array.isArray(tabla?.estudiantes) ? tabla.estudiantes : [];

    if (titulos[0]) {
      titulos[0].textContent = tabla?.materiaNombre ?? "";
    }
    if (titulos[1]) {
      titulos[1].textContent = `${tabla?.grado ?? ""} ${tabla?.seccion ?? ""}`;
    }

    tbody.replaceChildren();
    const ventanas = [];
    for (const [indice, estudiante] of estudiantes.entries()) {
      tbody.append(crearFila(estudiante, columnas, indice));
      ventanas.push(crearVentana(estudiante, columnas, tabla));
    }

    const primeraConfirmacion = principal.querySelector(
      '[data-js="confirmar-envio-todas"]',
    )?.closest(".ventana-modal");
    if (primeraConfirmacion) {
      primeraConfirmacion.before(...ventanas);
    } else {
      principal.append(...ventanas);
    }

    fijarEnlace(enviarTodas, notasEnEstado("Borrador").length > 0, hrefEnvio);
    fijarEnlace(
      publicarTodas,
      notasEnEstado("Enviada").length > 0,
      hrefPublicacion,
    );
  }

  function notasEnEstado(estado) {
    const lista = [];
    for (const estudiante of tablaActual?.estudiantes ?? []) {
      for (const columna of tablaActual?.columnas ?? []) {
        const nota = notaDe(estudiante, columna.numero);
        if (nota?.estado === estado) {
          lista.push({ estudiante, columna, nota });
        }
      }
    }
    return lista;
  }

  function crearFila(estudiante, columnas, indice) {
    const fila = moldeFila ? moldeFila.cloneNode(true) : document.createElement("tr");
    quitarEjemplo(fila);

    const id = estudiante?.estudianteId ?? "";
    const celdas = fila.querySelectorAll("td");
    if (celdas[0]) {
      celdas[0].textContent = String(indice + 1);
    }
    if (celdas[1]) {
      celdas[1].textContent = estudiante?.nombre ?? "";
    }
    if (celdas[2]) {
      celdas[2].textContent = estudiante?.apellido ?? "";
    }

    const actual = columnaActual(estudiante, columnas);
    const info = actual?.nota
      ? (ESTADOS[actual.nota.estado] ?? SIN_NOTA)
      : SIN_NOTA;
    const etiqueta = celdas[3]?.querySelector(".etiqueta-estado");
    if (etiqueta) {
      etiqueta.textContent = info.texto;
      etiqueta.className = `etiqueta-estado ${info.clase}`;
    }

    const enlace =
      celdas[4]?.querySelector('a[data-js="calificar-estudiante"]') ??
      celdas[4]?.querySelector("a");
    if (enlace) {
      enlace.id = `calificar-${id}`;
      enlace.href = `#ventana-calificar-${id}`;
    }

    return fila;
  }

  function crearVentana(estudiante, columnas, tabla) {
    const ventana = moldeVentana
      ? moldeVentana.cloneNode(true)
      : document.createElement("div");
    quitarEjemplo(ventana);

    const id = estudiante?.estudianteId ?? "";
    const idVentana = `ventana-calificar-${id}`;
    const idTitulo = `titulo-ventana-calificar-${id}`;
    ventana.id = idVentana;
    ventana.dataset.estudianteId = String(id);

    const caja = ventana.querySelector(".ventana-modal-caja");
    if (caja) {
      caja.setAttribute("aria-labelledby", idTitulo);
    }

    const titulo = ventana.querySelector(".ventana-modal-titulo");
    if (titulo) {
      titulo.id = idTitulo;
      titulo.textContent =
        `Estudiante: ${estudiante?.nombre ?? ""} ${estudiante?.apellido ?? ""}`;
    }

    const esquina = ventana.querySelector(".ventana-modal-esquina");
    if (esquina) {
      esquina.textContent = `${tabla?.grado ?? ""} ${tabla?.seccion ?? ""}`;
    }

    const detalles = [
      tabla?.materiaNombre ?? "",
      tabla?.esTecnica === true ? "Técnica" : "Básica",
    ];
    ventana.querySelectorAll(".lista-detalles dd").forEach((casilla, indice) => {
      if (detalles[indice] !== undefined) {
        casilla.textContent = detalles[indice];
      }
    });

    const form = ventana.querySelector('form[data-js="form-calificar"]');
    if (form) {
      form.id = `form-calificar-${id}`;
    }

    const selector = ventana.querySelector('[data-js="periodo"]');
    if (selector) {
      selector.id = `periodo-${id}`;
      selector.replaceChildren();
      const etiqueta = selector.closest(".fila-campo")?.querySelector("label");
      if (etiqueta) {
        etiqueta.htmlFor = selector.id;
      }
      columnas.forEach((columna, indice) => {
        selector.append(
          crearOpcion(columna, indice, estudiante, columnas, tabla),
        );
      });
      const actual = columnaActual(estudiante, columnas);
      if (actual?.columna) {
        selector.value = String(actual.columna.numero);
      }
    }

    const entrada = ventana.querySelector('[data-js="nota"]');
    if (entrada) {
      entrada.id = `nota-${id}`;
      entrada.step = "0.01";
      const etiqueta = entrada.closest(".fila-campo")?.querySelector("label");
      if (etiqueta) {
        etiqueta.htmlFor = entrada.id;
      }
    }

    const acumulada = ventana.querySelector('[data-js="acumulada"]');
    if (acumulada) {
      acumulada.id = `acumulada-${id}`;
      acumulada.value = esVacio(estudiante?.acumulado)
        ? "—"
        : formatear(estudiante.acumulado);
      acumulada.removeAttribute("data-ejemplo");
      const etiqueta = acumulada.closest(".fila-campo")?.querySelector("label");
      if (etiqueta) {
        etiqueta.htmlFor = acumulada.id;
      }
    }

    const entradaRecuperacion = ventana.querySelector(
      '[data-js="recuperacion-nota"]',
    );
    if (entradaRecuperacion) {
      entradaRecuperacion.id = `recuperacion-nota-${id}`;
      const etiqueta = entradaRecuperacion
        .closest(".fila-campo")
        ?.querySelector("label");
      if (etiqueta) {
        etiqueta.htmlFor = entradaRecuperacion.id;
      }
    }

    selector?.addEventListener("change", () => alCambiarColumna(ventana));
    for (const boton of ventana.querySelectorAll(
      ".ventana-modal-acciones button[data-js]",
    )) {
      boton.addEventListener("click", () => ejecutarAccion(ventana, boton.dataset.js));
    }
    alCambiarColumna(ventana);

    return ventana;
  }

  function crearOpcion(columna, indice, estudiante, columnas, tabla) {
    const nota = notaDe(estudiante, columna?.numero);
    let texto = columna?.etiqueta ?? "";
    if (
      tabla?.esTecnica === true &&
      !esVacio(columna?.puntajeMinimo)
    ) {
      texto += ` (mín. ${formatear(columna.puntajeMinimo)})`;
    }
    if (nota) {
      texto += ` · ${textoEstado(nota.estado)} (${formatear(nota.puntajeEfectivo ?? nota.puntajeObtenido)})`;
    }

    const opcion = document.createElement("option");
    opcion.value = String(columna?.numero ?? "");
    opcion.textContent = texto;

    if (!nota && indice > 0 && columna?.numero !== 1) {
      const anterior = notaDe(estudiante, columnas[indice - 1]?.numero);
      if (
        !anterior ||
        anterior.estado === "Borrador" ||
        anterior.estado === "Enviada"
      ) {
        opcion.disabled = true;
      }
    }

    return opcion;
  }

  function contextoDe(ventana) {
    const estudianteId = Number(ventana?.dataset.estudianteId);
    const numero = Number(ventana?.querySelector('[data-js="periodo"]')?.value);
    return {
      estudiante: tablaActual?.estudiantes?.find(
        (estudiante) => estudiante.estudianteId === estudianteId,
      ),
      columna: tablaActual?.columnas?.find(
        (columna) => columna.numero === numero,
      ),
      nota: notaDe(tablaActual?.estudiantes?.find(
        (estudiante) => estudiante.estudianteId === estudianteId,
      ), numero),
    };
  }

  function alCambiarColumna(ventana) {
    const { nota } = contextoDe(ventana);
    const entrada = ventana.querySelector('[data-js="nota"]');
    if (entrada) {
      entrada.value = nota ? formatear(nota.puntajeObtenido) : "";
    }

    const ayuda = ventana.querySelector('[data-js="ayuda"]');
    if (ayuda) {
      ayuda.textContent = `Estado: ${nota ? textoEstado(nota.estado) : SIN_NOTA.texto}`;
    }

    aplicarEstado(ventana, nota);
  }

  function aplicarEstado(ventana, nota) {
    const visibles = new Set();
    const estado = nota?.estado;
    const entrada = ventana.querySelector('[data-js="nota"]');
    const campoRecuperacion = ventana.querySelector(
      '[data-js="campo-recuperacion"]',
    );
    const entradaRecuperacion = ventana.querySelector(
      '[data-js="recuperacion-nota"]',
    );

    if (entrada) {
      entrada.readOnly = true;
    }
    if (campoRecuperacion) {
      campoRecuperacion.hidden = true;
    }
    if (entradaRecuperacion) {
      entradaRecuperacion.value = esVacio(nota?.puntajeRecuperacion)
        ? ""
        : formatear(nota.puntajeRecuperacion);
    }

    if (estado === undefined || estado === null || estado === "Borrador") {
      if (entrada) {
        entrada.readOnly = false;
      }
      visibles.add("borrador");
      visibles.add("enviar");
    } else if (estado === "Enviada") {
      visibles.add("publicar");
    } else if (estado === "Publicada") {
      if (entrada) {
        entrada.readOnly = false;
      }
      visibles.add("recuperacion");
      visibles.add("cerrar");
      visibles.add("corregir");
    } else if (estado === "EnRecuperacion") {
      if (campoRecuperacion) {
        campoRecuperacion.hidden = false;
      }
      visibles.add("cerrar");
    }

    for (const boton of ventana.querySelectorAll(
      ".ventana-modal-acciones button[data-js]",
    )) {
      boton.hidden = !visibles.has(boton.dataset.js);
      boton.disabled = false;
    }
  }

  function cuerpoNota(estudiante, columna, puntaje) {
    return {
      estudianteId: estudiante.estudianteId,
      materiaId: tablaActual?.materiaId,
      anioEscolarId: tablaActual?.anioEscolarId,
      tipoEvaluacion: columna.tipoEvaluacion,
      periodo: columna.tipoEvaluacion === "Periodo" ? columna.numero : null,
      numeroRa: columna.tipoEvaluacion === "RA" ? columna.numero : null,
      puntajeObtenido: puntaje,
    };
  }

  async function ejecutarAccion(ventana, accion) {
    const mensaje = ventana.querySelector('[data-js="mensaje-calificar"]');
    const { estudiante, columna, nota } = contextoDe(ventana);
    if (!estudiante || !columna) {
      return;
    }

    limpiar(mensaje);
    const entrada = ventana.querySelector('[data-js="nota"]');
    const textoNota = entrada?.value.trim() ?? "";
    const necesitaNota =
      accion === "borrador" || accion === "enviar" || accion === "corregir";
    if (necesitaNota && (textoNota === "" || Number.isNaN(Number(textoNota)))) {
      mostrarMensaje(mensaje, "Escribe la nota.", "error");
      return;
    }

    const campoRecuperacion = ventana.querySelector(
      '[data-js="campo-recuperacion"]',
    );
    const textoRecuperacion =
      ventana.querySelector('[data-js="recuperacion-nota"]')?.value.trim() ?? "";
    if (
      accion === "cerrar" &&
      campoRecuperacion &&
      !campoRecuperacion.hidden &&
      (textoRecuperacion === "" || Number.isNaN(Number(textoRecuperacion)))
    ) {
      mostrarMensaje(mensaje, "Escribe la nota de recuperación.", "error");
      return;
    }

    const puntaje = Number(textoNota);
    const botones = [
      ...ventana.querySelectorAll(".ventana-modal-acciones button[data-js]"),
    ];
    botones.forEach((boton) => {
      boton.disabled = true;
    });

    const avisos = [];
    let textoExito = "";

    try {
      if (accion === "borrador") {
        const respuesta = nota
          ? await pedir(
              "PATCH",
              `/api/calificaciones/${nota.calificacionId}/borrador`,
              { puntaje },
            )
          : await pedir(
              "POST",
              "/api/calificaciones",
              cuerpoNota(estudiante, columna, puntaje),
            );
        textoExito = "Nota guardada.";
        if (respuesta?.advertencia) {
          avisos.push(respuesta.advertencia);
        }
      } else if (accion === "enviar") {
        if (!nota) {
          const creada = await pedir(
            "POST",
            "/api/calificaciones",
            cuerpoNota(estudiante, columna, puntaje),
          );
          if (creada?.advertencia) {
            avisos.push(creada.advertencia);
          }
          await pedir("POST", `/api/calificaciones/${creada.id}/enviar`);
        } else if (nota.estado === "Borrador") {
          if (puntaje !== Number(nota.puntajeObtenido)) {
            const cambiada = await pedir(
              "PATCH",
              `/api/calificaciones/${nota.calificacionId}/borrador`,
              { puntaje },
            );
            if (cambiada?.advertencia) {
              avisos.push(cambiada.advertencia);
            }
          }
          await pedir("POST", `/api/calificaciones/${nota.calificacionId}/enviar`);
        }
        textoExito = "Nota enviada.";
      } else if (accion === "publicar") {
        await pedir("POST", `/api/calificaciones/${nota.calificacionId}/publicar`);
        textoExito = "Nota publicada.";
      } else if (accion === "recuperacion") {
        await pedir(
          "POST",
          `/api/calificaciones/${nota.calificacionId}/recuperacion`,
        );
        textoExito = "Enviada a recuperación.";
      } else if (accion === "cerrar") {
        const ruta = `/api/calificaciones/${nota.calificacionId}/cerrar`;
        if (campoRecuperacion && !campoRecuperacion.hidden) {
          await pedir("POST", ruta, {
            puntajeRecuperacion: Number(textoRecuperacion),
          });
        } else {
          await pedir("POST", ruta);
        }
        textoExito = "Nota cerrada.";
      } else if (accion === "corregir") {
        const respuesta = await pedir(
          "PATCH",
          `/api/calificaciones/${nota.calificacionId}/puntaje`,
          { puntaje },
        );
        textoExito = "Nota corregida.";
        if (respuesta?.advertencia) {
          avisos.push(respuesta.advertencia);
        }
      }

      const destino = await recargar({
        estudianteId: estudiante.estudianteId,
        numero: columna.numero,
      });
      const texto =
        `${textoExito}${avisos.map((aviso) => ` ${aviso}`).join("")}`;
      mostrarMensaje(
        destino?.querySelector('[data-js="mensaje-calificar"]') ?? mensaje,
        texto,
        "exito",
      );
    } catch (error) {
      mostrarMensaje(mensaje, error.message, "error");
    } finally {
      botones.forEach((boton) => {
        boton.disabled = false;
      });
    }
  }

  async function procesarTodas(accion, estado, mensajeVacio, participio) {
    const lista = notasEnEstado(estado);
    if (lista.length === 0) {
      mostrarMensaje(mensajePagina, mensajeVacio, "error");
      return;
    }

    let exitos = 0;
    const fallos = [];
    for (const { estudiante, columna, nota } of lista) {
      try {
        await pedir(
          "POST",
          `/api/calificaciones/${nota.calificacionId}/${accion}`,
        );
        exitos += 1;
      } catch (error) {
        fallos.push(
          `${estudiante.nombre ?? ""} ${estudiante.apellido ?? ""}, ${columna.etiqueta ?? ""}: ${error.message}`,
        );
      }
    }

    await recargar();
    let texto = `Se ${participio} ${exitos} notas.`;
    if (fallos.length > 0) {
      texto += ` No se pudo con ${fallos.length}: ${fallos.join("; ")}.`;
    }
    mostrarMensaje(mensajePagina, texto, fallos.length > 0 ? "error" : "exito");
  }

  confirmarEnvio?.addEventListener("click", async (evento) => {
    evento.preventDefault();
    location.hash = "";
    await procesarTodas(
      "enviar",
      "Borrador",
      "No hay notas en Borrador para enviar.",
      "enviaron",
    );
  });

  confirmarPublicacion?.addEventListener("click", async (evento) => {
    evento.preventDefault();
    location.hash = "";
    await procesarTodas(
      "publicar",
      "Enviada",
      "No hay notas enviadas para publicar.",
      "publicaron",
    );
  });

  await recargar();
}

function notaDe(estudiante, numero) {
  if (!estudiante || esVacio(numero)) {
    return undefined;
  }
  return estudiante.notas?.find((nota) => nota.numero === numero);
}

function columnaActual(estudiante, columnas) {
  for (const columna of columnas) {
    const nota = notaDe(estudiante, columna.numero);
    if (!nota || nota.estado !== "Cerrada") {
      return { columna, nota };
    }
  }
  const ultima = columnas.at(-1);
  return { columna: ultima, nota: notaDe(estudiante, ultima?.numero) };
}

function textoEstado(estado) {
  return ESTADOS[estado]?.texto ?? (esVacio(estado) ? SIN_NOTA.texto : estado);
}

function esVacio(valor) {
  return valor === null || valor === undefined || valor === "";
}

function formatear(valor) {
  if (esVacio(valor)) {
    return "";
  }
  const numero = Number(valor);
  if (Number.isNaN(numero)) {
    return String(valor);
  }
  return numero.toLocaleString("es-DO", { maximumFractionDigits: 2 });
}
