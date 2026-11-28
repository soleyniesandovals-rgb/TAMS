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
  await iniciarCursos();
}

async function iniciarCursos() {
  const lista = document.querySelector('[data-js="lista-cursos"]');
  const mensajePagina = document.querySelector('[data-js="mensaje-pagina"]');
  const mensajeVentana = document.querySelector('[data-js="mensaje-crear-curso"]');
  const formularioBusqueda = document.querySelector("#buscar-cursos");
  const campoBusqueda = document.querySelector("#busqueda-curso");
  const formulario = document.querySelector("#form-crear-curso");
  const selectorAnio = document.querySelector("#curso-anio");

  if (!lista || !formulario) {
    return;
  }

  const moldeTarjeta = tomarMolde(
    lista,
    "a[data-js='ver-estudiantes-curso']",
  );

  let anios = [];
  let cursos = [];
  let textoBusqueda = "";

  function tipoLegible(tipo) {
    return TIPOS_TECNICOS[tipo] ?? "";
  }

  function pintar() {
    lista.replaceChildren();
    const filtro = textoBusqueda.trim().toLowerCase();

    for (const curso of cursos) {
      const nombre = `${curso?.grado ?? ""} ${curso?.seccion ?? ""}`;
      if (!nombre.toLowerCase().includes(filtro)) {
        continue;
      }

      const tarjeta = moldeTarjeta
        ? moldeTarjeta.cloneNode(true)
        : document.createElement("a");
      quitarEjemplo(tarjeta);
      tarjeta.id = `ver-cursos-${curso?.id}`;
      tarjeta.href = `curso-estudiantes.html?cursoId=${curso?.id}`;

      const pestana = tarjeta.querySelector(".tarjeta-carpeta-pestana");
      if (pestana) {
        pestana.textContent = nombre;
      }
      const anio = tarjeta.querySelector(".tarjeta-carpeta-anio");
      if (anio) {
        anio.textContent = curso?.anioEscolarNombre ?? "";
      }
      const tipo = tarjeta.querySelector(".tarjeta-carpeta-tipo");
      if (tipo) {
        if (curso?.esTecnico === true) {
          tipo.textContent = `Técnico - ${tipoLegible(curso?.tipoTecnico)}`;
        } else {
          tipo.remove();
        }
      }

      lista.append(tarjeta);
    }
  }

  function llenarAnios(activoId) {
    if (!selectorAnio) {
      return;
    }
    selectorAnio.replaceChildren();
    for (const anio of anios) {
      const opcion = document.createElement("option");
      opcion.value = String(anio?.id ?? "");
      opcion.textContent = anio?.nombre ?? "";
      if (anio?.id === activoId) {
        opcion.selected = true;
      }
      selectorAnio.append(opcion);
    }
  }

  async function cargar() {
    try {
      anios = await pedir("GET", "/api/anios-escolares");
      const activo = anios.find((anio) => anio?.esActivo === true);
      llenarAnios(activo?.id);

      if (!activo) {
        cursos = [];
        pintar();
        mostrarMensaje(mensajePagina, "No hay un año escolar activo.", "error");
        return;
      }

      cursos = await pedir(
        "GET",
        `/api/cursos?anioEscolarId=${encodeURIComponent(activo.id)}`,
      );
      pintar();
    } catch (error) {
      cursos = [];
      pintar();
      mostrarMensaje(mensajePagina, error.message, "error");
    }
  }

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
      document.querySelector('input[name="esTecnico"]:checked')?.value === "si";
    const cuerpo = {
      grado: String(formulario.elements.grado?.value ?? ""),
      seccion: String(formulario.elements.seccion?.value ?? ""),
      esTecnico: esTecnica,
      tipoTecnico: esTecnica
        ? document.querySelector("#curso-tipo")?.value ?? null
        : null,
      anioEscolarId: Number(selectorAnio?.value ?? 0),
    };

    try {
      await pedir("POST", "/api/cursos", cuerpo);
      formulario.reset();
      const activo = anios.find((anio) => anio?.esActivo === true);
      if (selectorAnio && activo) {
        selectorAnio.value = String(activo.id);
      }
      location.hash = "";
      await cargar();
      mostrarMensaje(mensajePagina, "Curso creado.", "exito");
    } catch (error) {
      mostrarMensaje(mensajeVentana, error.message, "error");
    }
  });

  await cargar();
}
