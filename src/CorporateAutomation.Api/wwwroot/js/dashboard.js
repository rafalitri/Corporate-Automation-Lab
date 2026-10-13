
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
    mensaje: document.getElementById("app-message"),

    dialogoEdicion: document.getElementById("edit-task-dialog"),
    formularioEdicion: document.getElementById("edit-task-form"),
    nombreEdicion: document.getElementById("edit-task-name"),
    prioridadEdicion: document.getElementById("edit-task-priority"),
    errorEdicion: document.getElementById("edit-task-error"),
    cancelarEdicion: document.getElementById("edit-cancel-button"),
    guardarEdicion: document.getElementById("edit-save-button")
};

let cargaActual = 0;
let tareaEnEdicion = null;
let guardandoEdicion = false;

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

        celda.colSpan = 5;
        celda.className = "empty-state";
        celda.textContent = "No hay tareas para mostrar.";

        fila.appendChild(celda);
        elementos.tabla.appendChild(fila);

        return;
    }

    const fragmento = document.createDocumentFragment();

    for (const tarea of tareas) {
        const fila = document.createElement("tr");

        // Identificador.
        const celdaId = document.createElement("td");
        celdaId.textContent = tarea.id;

        // Nombre.
        const celdaNombre = document.createElement("td");
        celdaNombre.textContent = tarea.nombre;

        // Prioridad.
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

        // Estado.
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

        // Acciones disponibles.
        const celdaAcciones = document.createElement("td");
        const acciones = document.createElement("div");

        acciones.className = "task-actions";

        // Permitir editar tanto pendientes como completadas.
        const botonEditar = document.createElement("button");

        botonEditar.type = "button";
        botonEditar.className =
            "button button-secondary task-action-button";
        botonEditar.textContent = "Editar";

        botonEditar.setAttribute(
            "aria-label",
            `Editar tarea: ${tarea.nombre}`
        );

        botonEditar.addEventListener("click", () => {
            abrirEdicion(tarea);
        });

        acciones.appendChild(botonEditar);

        // Solo las tareas pendientes pueden completarse.
        if (tarea.estado === "Pendiente") {
            const botonCompletar = document.createElement("button");

            botonCompletar.type = "button";
            botonCompletar.className =
                "button button-secondary task-action-button";
            botonCompletar.textContent = "Completar";

            botonCompletar.setAttribute(
                "aria-label",
                `Completar tarea: ${tarea.nombre}`
            );

            botonCompletar.addEventListener("click", () => {
                completarTarea(tarea, botonCompletar);
            });

            acciones.appendChild(botonCompletar);
        }

        celdaAcciones.appendChild(acciones);

        fila.append(
            celdaId,
            celdaNombre,
            celdaPrioridad,
            celdaEstado,
            celdaAcciones
        );

        fragmento.appendChild(fila);
    }

    elementos.tabla.appendChild(fragmento);
}

// Abrir la ventana de edición con los datos actuales.
function abrirEdicion(tarea) {
    if (guardandoEdicion) {
        return;
    }

    tareaEnEdicion = tarea;

    elementos.nombreEdicion.value = tarea.nombre;
    elementos.prioridadEdicion.value = tarea.prioridad;
    elementos.errorEdicion.textContent = "";

    mostrarMensaje("");

    elementos.dialogoEdicion.showModal();
    elementos.nombreEdicion.focus();
}

// Guardar cambios mediante PUT, conservando el estado.
async function guardarCambiosTarea(evento) {
    evento.preventDefault();

    if (!tareaEnEdicion || guardandoEdicion) {
        return;
    }

    const nombre = elementos.nombreEdicion.value.trim();
    const prioridad = elementos.prioridadEdicion.value;
    const tarea = tareaEnEdicion;

    // Validar el nombre antes de enviarlo.
    if (!nombre || nombre.length > 100) {
        elementos.errorEdicion.textContent =
            "El nombre debe contener entre 1 y 100 caracteres.";

        elementos.nombreEdicion.focus();
        return;
    }

    // Validar la prioridad.
    if (!["Baja", "Media", "Alta"].includes(prioridad)) {
        elementos.errorEdicion.textContent =
            "Selecciona una prioridad válida.";

        return;
    }

    guardandoEdicion = true;

    elementos.guardarEdicion.disabled = true;
    elementos.cancelarEdicion.disabled = true;
    elementos.errorEdicion.textContent = "";

    try {
        await solicitarJSON(`${API.tareas}/${tarea.id}`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                nombre,
                estado: tarea.estado,
                prioridad
            })
        });

        // Cerrar la ventana después de guardar.
        elementos.dialogoEdicion.close();

        // Actualizar la tabla y las estadísticas.
        const actualizado = await cargarDashboard();

        if (actualizado) {
            mostrarMensaje(
                "Tarea actualizada correctamente.",
                "success"
            );
        } else {
            mostrarMensaje(
                "Los cambios se guardaron, pero no se pudo actualizar el panel.",
                "error"
            );
        }
    } catch (error) {
        console.error("Error al editar la tarea:", error);

        // Si falla el guardado, mantener la ventana abierta.
        if (elementos.dialogoEdicion.open) {
            elementos.errorEdicion.textContent =
                "No se pudieron guardar los cambios. Inténtalo de nuevo.";
        } else {
            mostrarMensaje(
                "No se pudo actualizar el panel.",
                "error"
            );
        }
    } finally {
        guardandoEdicion = false;

        elementos.guardarEdicion.disabled = false;
        elementos.cancelarEdicion.disabled = false;
    }
}

// Marcar una tarea como completada mediante PUT.
async function completarTarea(tarea, boton) {
    boton.disabled = true;

    mostrarMensaje(
        `Completando la tarea "${tarea.nombre}"...`,
        "info"
    );

    try {
        await solicitarJSON(`${API.tareas}/${tarea.id}`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                nombre: tarea.nombre,
                estado: "Completada",
                prioridad: tarea.prioridad
            })
        });

        const actualizado = await cargarDashboard();

        if (actualizado) {
            mostrarMensaje(
                `Tarea "${tarea.nombre}" completada correctamente.`,
                "success"
            );
        } else {
            mostrarMensaje(
                "La tarea se ha completado. Actualiza el panel para consultar su estado.",
                "info"
            );
        }
    } catch (error) {
        console.error("Error al completar tarea:", error);

        mostrarMensaje(
            "No se pudo completar la tarea. Inténtalo de nuevo.",
            "error"
        );
    } finally {
        boton.disabled = false;
    }
}

// Recuperar todas las páginas de tareas.
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

        // Ignorar respuestas antiguas si cambia el filtro.
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

    if (!nombre || nombre.length > 100) {
        mostrarMensaje(
            "El nombre debe contener entre 1 y 100 caracteres.",
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

// Guardar edición.
elementos.formularioEdicion.addEventListener(
    "submit",
    guardarCambiosTarea
);

// Cancelar edición.
elementos.cancelarEdicion.addEventListener("click", () => {
    if (!guardandoEdicion) {
        elementos.dialogoEdicion.close();
    }
});

// Impedir cerrar la ventana mientras se guardan cambios.
elementos.dialogoEdicion.addEventListener("cancel", (evento) => {
    if (guardandoEdicion) {
        evento.preventDefault();
    }
});

// Limpiar la información temporal al cerrar la ventana.
elementos.dialogoEdicion.addEventListener("close", () => {
    tareaEnEdicion = null;
    elementos.errorEdicion.textContent = "";
});

// Carga inicial.
cargarDashboard();
