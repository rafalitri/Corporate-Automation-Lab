
"use strict";

// Corporate Automation Lab - Dashboard
// Comunicación con la API REST de ASP.NET Core.

const API = {
    tareas: "/api/tareas",
    resumen: "/api/tareas/resumen"
};

const elementos = {
    total: document.getElementById("stat-total"),
    pendientes: document.getElementById("stat-pending"),
    completadas: document.getElementById("stat-completed"),
    prioridadAlta: document.getElementById("stat-high"),

    tabla: document.getElementById("tasks-table-body"),
    filtro: document.getElementById("status-filter"),

    formulario: document.getElementById("create-task-form"),
    nombre: document.getElementById("task-name"),
    prioridad: document.getElementById("task-priority"),

    actualizar: document.getElementById("refresh-button"),
    mensaje: document.getElementById("app-message")
};

let cargaActual = 0;

// Mostrar mensajes al usuario.
function mostrarMensaje(texto, tipo = "info") {
    elementos.mensaje.textContent = texto;

    elementos.mensaje.className = "app-message";

    if (texto) {
        elementos.mensaje.classList.add(`is-${tipo}`);
    }
}

// Realizar peticiones HTTP y comprobar errores.
async function solicitarJSON(url, opciones = {}) {
    const respuesta = await fetch(url, {
        ...opciones,
        headers: {
            Accept: "application/json",
            ...opciones.headers
        }
    });

    if (!respuesta.ok) {
        throw new Error(
            `La API devolvió el código HTTP ${respuesta.status}`
        );
    }

    return respuesta.json();
}

// Actualizar las tarjetas de estadísticas.
function mostrarEstadisticas(resumen) {
    elementos.total.textContent = resumen.total;
    elementos.pendientes.textContent = resumen.pendientes;
    elementos.completadas.textContent = resumen.completadas;
    elementos.prioridadAlta.textContent = resumen.prioridadAlta;
}

// Crear una etiqueta visual para prioridad o estado.
function crearEtiqueta(texto, clase) {
    const etiqueta = document.createElement("span");

    etiqueta.textContent = texto;
    etiqueta.className = clase;

    return etiqueta;
}

// Mostrar las tareas en la tabla.
function mostrarTareas(tareas) {
    elementos.tabla.replaceChildren();

    if (tareas.length === 0) {
        const fila = document.createElement("tr");
        const celda = document.createElement("td");

        celda.colSpan = 4;
        celda.className = "empty-state";
        celda.textContent = "No hay tareas para mostrar.";

        fila.appendChild(celda);
        elementos.tabla.appendChild(fila);

        return;
    }

    const fragmento = document.createDocumentFragment();

    for (const tarea of tareas) {
        const fila = document.createElement("tr");

        const celdaId = document.createElement("td");
        celdaId.textContent = tarea.id;

        const celdaNombre = document.createElement("td");
        celdaNombre.textContent = tarea.nombre;

        const celdaPrioridad = document.createElement("td");

        const clasesPrioridad = {
            Baja: "priority-low",
            Media: "priority-medium",
            Alta: "priority-high"
        };

        celdaPrioridad.appendChild(
            crearEtiqueta(
                tarea.prioridad,
                `priority-pill ${
                    clasesPrioridad[tarea.prioridad] || "priority-medium"
                }`
            )
        );

        const celdaEstado = document.createElement("td");

        const claseEstado = tarea.estado === "Completada"
            ? "status-completed"
            : "status-pending";

        celdaEstado.appendChild(
            crearEtiqueta(
                tarea.estado,
                `status-pill ${claseEstado}`
            )
        );

        fila.append(
            celdaId,
            celdaNombre,
            celdaPrioridad,
            celdaEstado
        );

        fragmento.appendChild(fila);
    }

    elementos.tabla.appendChild(fragmento);
}

// Recuperar todas las páginas de tareas.
// La API permite un máximo de 100 tareas por petición.
async function obtenerTareas(estado) {
    const tareas = [];
    const tamanoPagina = 100;

    let pagina = 1;

    while (true) {
        const parametros = new URLSearchParams({
            pagina: String(pagina),
            tamanoPagina: String(tamanoPagina)
        });

        if (estado) {
            parametros.set("estado", estado);
        }

        const respuesta = await fetch(
            `${API.tareas}?${parametros.toString()}`,
            {
                headers: {
                    Accept: "application/json"
                }
            }
        );

        if (!respuesta.ok) {
            throw new Error(
                `Error al consultar tareas: HTTP ${respuesta.status}`
            );
        }

        const resultado = await respuesta.json();

        if (!Array.isArray(resultado)) {
            throw new Error("La API devolvió datos inesperados.");
        }

        tareas.push(...resultado);

        const total = Number(
            respuesta.headers.get("X-Total-Count")
        );

        if (
            resultado.length < tamanoPagina ||
            (total > 0 && tareas.length >= total)
        ) {
            break;
        }

        pagina++;
    }

    return tareas;
}

// Cargar estadísticas y tareas desde SQL Server.
async function cargarDashboard() {
    const identificador = ++cargaActual;

    mostrarMensaje("Cargando información...", "info");

    try {
        const estadoSeleccionado = elementos.filtro.value;

        const [resumen, tareas] = await Promise.all([
            solicitarJSON(API.resumen),
            obtenerTareas(estadoSeleccionado)
        ]);

        // Ignorar una respuesta antigua si el filtro cambió.
        if (identificador !== cargaActual) {
            return false;
        }

        mostrarEstadisticas(resumen);
        mostrarTareas(tareas);

        mostrarMensaje("");

        return true;
    } catch (error) {
        if (identificador !== cargaActual) {
            return false;
        }

        console.error("Error al cargar dashboard:", error);

        mostrarMensaje(
            "No se pudieron cargar los datos. Comprueba la API.",
            "error"
        );

        return false;
    }
}

// Crear una nueva tarea mediante POST.
async function crearTarea(evento) {
    evento.preventDefault();

    const nombre = elementos.nombre.value.trim();
    const prioridad = elementos.prioridad.value;

    if (!nombre) {
        mostrarMensaje(
            "Debes introducir el nombre de la tarea.",
            "error"
        );

        return;
    }

    const boton = elementos.formulario.querySelector(
        'button[type="submit"]'
    );

    boton.disabled = true;

    mostrarMensaje("Creando tarea...", "info");

    try {
        await solicitarJSON(API.tareas, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                nombre,
                prioridad
            })
        });

        elementos.formulario.reset();

        const actualizado = await cargarDashboard();

        if (actualizado) {
            mostrarMensaje(
                "Tarea creada correctamente.",
                "success"
            );
        } else {
            mostrarMensaje(
                "La tarea se creó, pero no se pudo actualizar el panel.",
                "error"
            );
        }

        elementos.nombre.focus();
    } catch (error) {
        console.error("Error al crear tarea:", error);

        mostrarMensaje(
            "No se pudo crear la tarea. Revisa los datos.",
            "error"
        );
    } finally {
        boton.disabled = false;
    }
}

// Eventos de la interfaz.
elementos.actualizar.addEventListener(
    "click",
    cargarDashboard
);

elementos.filtro.addEventListener(
    "change",
    cargarDashboard
);

elementos.formulario.addEventListener(
    "submit",
    crearTarea
);

// Carga inicial.
cargarDashboard();
