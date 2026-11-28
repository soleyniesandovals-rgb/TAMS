import { pedir } from "./api.js";
import { usuarioActual } from "./marco.js";
import {
  limpiar,
  mostrarMensaje,
  quitarEjemplo,
  tomarMolde,
} from "./ui.js";

const TIPOS_TECNICOS = {
  ComercioYMercadeo: "Comercio y Mercadeo",
  DesarrolloAdministracionAplicacionesInformaticas:
    "Desarrollo y Administración de Aplicaciones Informáticas",
};

const usuario = await usuarioActual;
if (usuario) {
  await iniciarMaterias();
}

async function iniciarMaterias() {
  const tbody = document.querySelector('[data-js="tabla-materias"] tbody');
  const mensajePagina = document.querySelector('[data-js="mensaje-pagina"]');
  const mensajeVentana = document.querySelector('[data-js="mensaje-crear-materia"]');
  const formularioBusqueda = document.querySelector("#buscar-materias");
  const campoBusqueda = document.querySelector("#busqueda-materia");
  const formulario = document.querySelector("#form-crear-materia");
  const cantidad = document.querySelector("#materia-cantidad-ra");
  const tbodyRa = document.querySelector("#tabla-ra tbody");
  const suma = document.querySelector("#suma-puntos");

  if (!tbody || !formulario) {
    return;
  }

  const moldeFila = tomarMolde(tbody, "tr");
  const moldeRa = tomarMolde(tbodyRa, "tr");

  let materias = [];
  let textoBusqueda = "";

  function tipoLegible(tipo) {
    return TIPOS_TECNICOS[tipo] ?? "";
  }

  function generarFilasRa() {
    tbodyRa.replaceChildren();
    suma.hidden = true;
    suma.textContent = "";

    const total = Number(cantidad?.value ?? 1) || 1;
    const base = Math.floor(100 / total);
    const resto = 100 % total;

    for (let numero = 1; numero <= total; numero += 1) {
      const fila = moldeRa ? moldeRa.cloneNode(true) : document.createElement("tr");
      quitarEjemplo(fila);

      const celdas = fila.querySelectorAll("td");
      if (celdas[0]) {
        celdas[0].textContent = String(numero);
      }

      const etiquetas = fila.querySelectorAll("label");
      const campos = fila.querySelectorAll("input");
      const puntos = base + (numero <= resto ? 1 : 0);

      etiquetas.forEach((etiqueta, indice) => {
        const nombres = ["Descripción", "Puntos", "Mínimo"];
        etiqueta.textContent = `${nombres[indice] ?? "Dato"} del RA${numero}`;
      });

      const ids = [
        `ra${numero}-descripcion`,
        `ra${numero}-puntos`,
        `ra${numero}-minimo`,
      ];
      campos.forEach((campo, indice) => {
        campo.id = ids[indice];
        if (etiquetas[indice]) {
          etiquetas[indice].setAttribute("for", ids[indice]);
        }
      });

      if (campos[1]) {
        campos[1].value = String(puntos);
      }
      if (campos[2]) {
        campos[2].removeAttribute("max");
        campos[2].setAttribute("min", "0");
        campos[2].setAttribute("max", "100");
        campos[2].value = "";
      }

      tbodyRa.append(fila);
    }
  }

  function pintar() {
    tbody.replaceChildren();
    const filtro = textoBusqueda.trim().toLowerCase();
    for (const materia of materias) {
      if (!String(materia?.nombre ?? "").toLowerCase().includes(filtro)) {
        continue;
      }
      const fila = moldeFila ? moldeFila.cloneNode(true) : document.createElement("tr");
      quitarEjemplo(fila);

      const resultados = Array.isArray(materia?.resultadosAprendizaje)
        ? materia.resultadosAprendizaje
        : [];
      const puntos = resultados.map((resultado) => {
        const base = String(resultado?.puntos ?? "");
        return resultado?.puntajeMinimo === null || resultado?.puntajeMinimo === undefined
          ? base
          : `${base} (mín. ${resultado.puntajeMinimo})`;
      });
      const cantidadRa = Number(materia?.cantidadRa ?? resultados.length);

      const celdas = fila.querySelectorAll("td");
      const datos = [
        materia?.nombre ?? "",
        materia?.esTecnica === true ? "Sí" : "No",
        materia?.esTecnica === true ? tipoLegible(materia?.tipoTecnico) : "",
        cantidadRa > 0 ? String(cantidadRa) : "—",
        puntos.join(" · "),
      ];
      datos.forEach((texto, indice) => {
        if (celdas[indice]) {
          celdas[indice].textContent = texto;
        }
      });

      tbody.append(fila);
    }
  }

  async function cargar() {
    try {
      materias = await pedir("GET", "/api/materias");
      pintar();
    } catch (error) {
      tbody.replaceChildren();
      mostrarMensaje(mensajePagina, error.message, "error");
    }
  }

  generarFilasRa();

  cantidad?.addEventListener("change", generarFilasRa);

  tbodyRa.addEventListener("input", (evento) => {
    if (!evento.target?.id?.endsWith("-puntos")) {
      return;
    }
    const total = Array.from(tbodyRa.querySelectorAll('input[id$="-puntos"]')).reduce(
      (acumulado, campo) => acumulado + (Number(campo.value) || 0),
      0,
    );
    suma.textContent = `Suma: ${total} de 100`;
    suma.hidden = false;
  });

  if (formularioBusqueda) {
    formularioBusqueda.noValidate = true;
    formularioBusqueda.addEventListener("submit", (evento) => {
      evento.preventDefault();
      textoBusqueda = campoBusqueda?.value ?? "";
      pintar();
    });
  }

  formulario.noValidate = true;
  formulario.addEventListener("submit", async (evento) => {
    evento.preventDefault();
    limpiar(mensajeVentana);

    const esTecnica =
      document.querySelector('input[name="esTecnica"]:checked')?.value === "si";
    const resultados = esTecnica
      ? Array.from(tbodyRa.querySelectorAll("tr")).map((fila, indice) => {
          const campos = fila.querySelectorAll("input");
          const minimo = String(campos[2]?.value ?? "").trim();
          return {
            numero: indice + 1,
            descripcion: String(campos[0]?.value ?? "").trim() || null,
            puntos: Number(campos[1]?.value ?? 0),
            puntajeMinimo: minimo === "" ? null : Number(minimo),
          };
        })
      : [];

    const cuerpo = {
      nombre: String(formulario.elements.nombre?.value ?? ""),
      esTecnica,
      tipoTecnico: esTecnica
        ? document.querySelector("#materia-tipo")?.value ?? null
        : null,
      resultadosAprendizaje: resultados,
    };

    try {
      await pedir("POST", "/api/materias", cuerpo);
      formulario.reset();
      if (cantidad) {
        cantidad.value = "3";
      }
      generarFilasRa();
      location.hash = "";
      await cargar();
      mostrarMensaje(mensajePagina, "Materia creada.", "exito");
    } catch (error) {
      mostrarMensaje(mensajeVentana, error.message, "error");
    }
  });

  await cargar();
}
