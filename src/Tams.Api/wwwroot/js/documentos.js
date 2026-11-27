import { descargar, pedir } from "./api.js";
import { usuarioActual } from "./marco.js";
import { limpiar, mostrarMensaje } from "./ui.js";

const CLASES_TIPO = {
  PDF: "tipo-pdf",
  XLS: "tipo-xls",
  XLSX: "tipo-xls",
  CSV: "tipo-xls",
  XML: "tipo-xml",
  JS: "tipo-js",
  DOC: "tipo-docx",
  DOCX: "tipo-docx",
};

const usuario = await usuarioActual;
if (usuario) {
  await iniciarDocumentos();
}

async function iniciarDocumentos() {
  const mensaje = document.querySelector('[data-js="mensaje-documentos"]');
  const lista = document.querySelector('[data-js="lista-documentos"]');
  const mensajeVacio = document.querySelector(
    '[data-js="mensaje-sin-documentos"]',
  );
  const textoVacio = mensajeVacio?.textContent ?? "No hay documentos.";
  const moldeFila = lista?.querySelector("li.documento-fila")?.cloneNode(true);

  const listaPildoras = document.querySelector(
    '[data-js="archivos-elegidos"]',
  );
  const moldePildora = listaPildoras
    ?.querySelector("li.archivo-pildora")
    ?.cloneNode(true);
  listaPildoras?.querySelectorAll("li.archivo-pildora").forEach((pildora) => {
    pildora.remove();
  });

  for (const id of [
    "ventana-eliminar-2",
    "ventana-eliminar-3",
    "ventana-eliminar-4",
  ]) {
    document.getElementById(id)?.remove();
  }

  const parrafoEliminar = document.querySelector(
    "#ventana-eliminar-1 .ventana-modal-contenido p",
  );
  const botonConfirmar = document.querySelector("#confirmar-eliminar-1");
  const formulario = document.querySelector(
    '[data-js="form-subir-documentos"]',
  );
  const selector = document.querySelector('[data-js="seleccionar-archivos"]');
  const botonAceptar = document.querySelector('[data-js="aceptar-documentos"]');
  const archivos = [];
  let idEliminar = null;

  function quitarEjemplo(elemento) {
    elemento.removeAttribute("data-ejemplo");
    elemento.querySelectorAll("[data-ejemplo]").forEach((hijo) => {
      hijo.removeAttribute("data-ejemplo");
    });
  }

  function formatearFecha(fecha) {
    return fecha ? new Date(fecha).toLocaleDateString("es-DO") : "";
  }

  function formatearTamano(bytes) {
    const valor = Number(bytes) || 0;
    if (valor >= 1024 * 1024) {
      const megabytes = valor / (1024 * 1024);
      return `${megabytes.toLocaleString("es-DO", {
        minimumFractionDigits: 1,
        maximumFractionDigits: 1,
      })} MB`;
    }

    const kilobytes = valor / 1024;
    return `${kilobytes.toLocaleString("es-DO", {
      maximumFractionDigits: 1,
    })} KB`;
  }

  function aplicarTipo(insignia, extension) {
    if (!insignia) {
      return;
    }

    insignia.classList.remove(...Object.values(CLASES_TIPO));
    const clase = CLASES_TIPO[extension];
    if (clase) {
      insignia.classList.add(clase);
    }
    insignia.textContent = extension;
  }

  function crearFila(documento) {
    const fila = moldeFila
      ? moldeFila.cloneNode(true)
      : document.createElement("li");
    fila.classList.add("documento-fila");
    quitarEjemplo(fila);

    const nombre = documento?.nombre ?? "";
    const extension = extensionDe(nombre);
    aplicarTipo(fila.querySelector(".insignia-tipo"), extension);

    const nombreElemento = fila.querySelector(".documento-nombre");
    if (nombreElemento) {
      nombreElemento.textContent = nombre;
    }

    const datos = fila.querySelectorAll(".documento-dato");
    if (datos[0]) {
      datos[0].textContent = formatearTamano(documento?.tamanoBytes);
    }
    if (datos[1]) {
      datos[1].textContent = formatearFecha(documento?.fechaSubida);
    }

    const enlaceDescarga = fila.querySelector('[data-js="descargar-documento"]');
    if (enlaceDescarga) {
      enlaceDescarga.id = `descargar-${documento?.id}`;
      enlaceDescarga.href = "#";
      enlaceDescarga.addEventListener("click", (evento) => {
        evento.preventDefault();
        descargarDocumento(documento?.id);
      });
    }

    const enlaceEliminar = fila.querySelector('[data-js="eliminar-documento"]');
    if (enlaceEliminar) {
      enlaceEliminar.id = `eliminar-${documento?.id}`;
      enlaceEliminar.href = "#ventana-eliminar-1";
      enlaceEliminar.addEventListener("click", () => {
        idEliminar = documento?.id;
        if (parrafoEliminar) {
          parrafoEliminar.textContent = `¿Está seguro de eliminar «${nombre}»?`;
        }
      });
    }

    return fila;
  }

  async function cargarLista() {
    if (!lista) {
      return;
    }

    try {
      const documentos = await pedir("GET", "/api/documentos");
      lista.querySelectorAll("li").forEach((fila) => fila.remove());

      if (!Array.isArray(documentos) || documentos.length === 0) {
        if (mensajeVacio) {
          mensajeVacio.textContent = textoVacio;
          mensajeVacio.classList.remove("mensaje-exito", "mensaje-error");
          mensajeVacio.hidden = false;
        }
        return;
      }

      if (mensajeVacio) {
        mensajeVacio.hidden = true;
      }

      for (const documento of documentos) {
        lista.append(crearFila(documento));
      }
    } catch (error) {
      mostrarMensaje(mensaje, error.message, "error");
    }
  }

  async function descargarDocumento(id) {
    try {
      const resultado = await descargar(`/api/documentos/${id}/descargar`);
      const url = URL.createObjectURL(resultado.blob);
      const enlace = document.createElement("a");
      enlace.href = url;
      enlace.download = resultado.nombre;
      document.body.append(enlace);
      enlace.click();
      setTimeout(() => URL.revokeObjectURL(url), 1000);
      enlace.remove();
    } catch (error) {
      mostrarMensaje(mensaje, error.message, "error");
    }
  }

  async function confirmarEliminacion(evento) {
    evento.preventDefault();

    if (idEliminar === null || idEliminar === undefined) {
      location.hash = "";
      return;
    }

    const id = idEliminar;
    idEliminar = null;

    try {
      await pedir("DELETE", `/api/documentos/${id}`);
      location.hash = "";
      await cargarLista();
      mostrarMensaje(mensaje, "Documento eliminado.", "exito");
    } catch (error) {
      location.hash = "";
      mostrarMensaje(mensaje, error.message, "error");
    }
  }

  function extensionDe(nombre) {
    const partes = String(nombre ?? "").split(".");
    return partes.length > 1 ? partes.pop().toUpperCase() : "";
  }

  function pintarArchivos() {
    if (!listaPildoras) {
      return;
    }

    listaPildoras.querySelectorAll("li").forEach((pildora) => pildora.remove());

    archivos.forEach((archivo, indice) => {
      const pildora = moldePildora
        ? moldePildora.cloneNode(true)
        : document.createElement("li");
      pildora.classList.add("archivo-pildora");
      quitarEjemplo(pildora);

      const extension = extensionDe(archivo.name);
      aplicarTipo(pildora.querySelector(".insignia-tipo"), extension);

      const nombre = pildora.querySelector(".archivo-nombre");
      if (nombre) {
        nombre.textContent = archivo.name;
      }

      const tamano = pildora.querySelector(".archivo-tamano");
      if (tamano) {
        tamano.textContent = formatearTamano(archivo.size);
      }

      const quitar = pildora.querySelector(".quitar-archivo");
      if (quitar) {
        quitar.href = "#ventana-subir-documentos";
        quitar.addEventListener("click", (evento) => {
          evento.preventDefault();
          archivos.splice(indice, 1);
          pintarArchivos();
        });
      }

      listaPildoras.append(pildora);
    });
  }

  async function subirArchivos(evento) {
    evento.preventDefault();
    limpiar(mensaje);

    if (archivos.length === 0) {
      mostrarMensaje(mensaje, "Elige al menos un archivo.", "error");
      return;
    }

    if (botonAceptar) {
      botonAceptar.disabled = true;
    }

    const fallidos = [];
    let subidos = 0;

    try {
      for (const archivo of archivos) {
        const cuerpo = new FormData();
        cuerpo.append("archivo", archivo);

        try {
          await pedir("POST", "/api/documentos", cuerpo);
          subidos += 1;
        } catch (error) {
          fallidos.push(`${archivo.name}: ${error.message}`);
        }
      }
    } finally {
      archivos.length = 0;
      pintarArchivos();
      formulario?.reset();
      location.hash = "";
      await cargarLista();
      if (botonAceptar) {
        botonAceptar.disabled = false;
      }
    }

    if (fallidos.length > 0) {
      mostrarMensaje(mensaje, fallidos.join(" · "), "error");
    } else {
      mostrarMensaje(mensaje, `Se subieron ${subidos} archivo(s).`, "exito");
    }
  }

  botonConfirmar?.addEventListener("click", confirmarEliminacion);
  selector?.addEventListener("change", () => {
    for (const archivo of Array.from(selector.files ?? [])) {
      archivos.push(archivo);
    }
    selector.value = "";
    pintarArchivos();
  });
  formulario?.addEventListener("submit", subirArchivos);

  await cargarLista();
}
