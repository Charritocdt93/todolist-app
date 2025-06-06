// Archivo: js/app.js

const apiBase = "/api/todolist";

// Usamos este arreglo como “cache local” para poder consultar
// el detalle de cada TodoItem (por ejemplo, cuántas progresiones tiene,
// fecha de última progresión, etc.).
let itemsCache = [];

// ──────────────────────────────────────────
// 1) Cuando cargue la página, ejecutamos fetchItems()
// ──────────────────────────────────────────
window.addEventListener("DOMContentLoaded", () => {
    fetchCategories(); // Llenar dropdown de categorías
    fetchItems();

    // Formulario “Crear”
    document
        .getElementById("create-form")
        .addEventListener("submit", (e) => {
            e.preventDefault();
            const title = document.getElementById("new-title").value.trim();
            const desc = document.getElementById("new-desc").value.trim();
            const category = document.getElementById("new-category").value;
            createItem(title, desc, category);
        });

    // Cancelar formulario de Progresión
    const progCancelBtn = document.getElementById("prog-cancel");
    progCancelBtn.addEventListener("click", () => {
        hideProgressionForm();
    });
    progCancelBtn.type = "button";

    // Enviar formulario de Progresión
    document
        .getElementById("progression-form")
        .addEventListener("submit", (e) => {
            e.preventDefault();
            const id = document.getElementById("prog-parent-id").value;
            const date = document.getElementById("prog-date").value;
            const percent = parseFloat(
                document.getElementById("prog-percent").value
            );
            registerProgression(id, date, percent);
        });

    // Cancelar formulario de Actualizar
    const updCancelBtn = document.getElementById("upd-cancel");
    updCancelBtn.addEventListener("click", () => {
        hideUpdateForm();
    });
    updCancelBtn.type = "button";

    // Enviar formulario de Actualizar
    document
        .getElementById("update-form")
        .addEventListener("submit", (e) => {
            e.preventDefault();
            const id = document.getElementById("upd-parent-id").value;
            const newDesc = document.getElementById("upd-new-desc").value.trim();
            updateDescription(id, newDesc);
        });
});

// ──────────────────────────────────────────
// 2) Obtener y desplegar categorías (llena el <select>)
// ──────────────────────────────────────────
async function fetchCategories() {
    try {
        const res = await fetch(`${apiBase}/categories`);
        if (!res.ok) throw new Error("No se pudieron cargar las categorías.");
        const categories = await res.json();
        const dropdown = document.getElementById("new-category");
        dropdown.innerHTML = `<option value="">-- Selecciona categoría --</option>`;
        categories.forEach((cat) => {
            const opt = document.createElement("option");
            opt.value = cat;
            opt.textContent = cat;
            dropdown.appendChild(opt);
        });
    } catch (err) {
        console.error(err);
    }
}

// ──────────────────────────────────────────
// 3) Obtener y desplegar la lista de TodoItems
// ──────────────────────────────────────────
async function fetchItems() {
    try {
        const res = await fetch(apiBase);
        if (!res.ok) throw new Error("No se pudo obtener la lista.");
        const items = await res.json();

        // Guardamos en el cache local
        itemsCache = items;

        // Si hay al menos un elemento, mostramos la tarjeta de Listado
        const listSection = document.getElementById("list-section");
        if (items.length > 0) {
            listSection.classList.remove("hidden");
        } else {
            listSection.classList.add("hidden");
        }

        // Limpiamos el contenedor antes de volver a renderizar
        const container = document.getElementById("items-container");
        container.innerHTML = "";

        // Por cada item, creamos su card individual
        items.forEach((item) => {
            const divItem = document.createElement("div");
            divItem.className = "todo-item";

            // 3.1) Cabecera (id, título, descripción, categoría, completado)
            const header = document.createElement("div");
            header.className = "todo-item-header";
            header.innerHTML = `<strong>${item.id})</strong> ${item.title} 
        <span class="text-muted">(${item.category})</span> 
        Completado: ${item.isCompleted}`;
            divItem.appendChild(header);

            // 3.2) Si tiene al menos una progresión, mostramos fecha y porcentaje
            if (
                Array.isArray(item.progressions) &&
                item.progressions.length > 0
            ) {
                // Nos quedamos con la última progresión para mostrar su porcentaje
                const ultimaProg = item.progressions[item.progressions.length - 1];
                const fecha = new Date(ultimaProg.date).toLocaleDateString();
                const porcentaje = ultimaProg.percent;

                const p = document.createElement("p");
                p.textContent = `${fecha} — ${porcentaje}%`;
                divItem.appendChild(p);

                // Barra de progreso con porcentaje
                const barContainer = document.createElement("div");
                barContainer.className = "progress-bar";

                const barInner = document.createElement("div");
                barInner.className = "progress-bar-inner";
                barInner.style.width = `${porcentaje}%`;
                barInner.textContent = `${porcentaje}%`;

                barContainer.appendChild(barInner);
                divItem.appendChild(barContainer);
            }

            // 3.3) Botones debajo de cada item
            const btnProg = document.createElement("button");
            btnProg.textContent = "Añadir Progresión";
            btnProg.className = "btn btn-sm btn-add-prog";
            btnProg.onclick = () => showProgressionForm(item.id);
            divItem.appendChild(btnProg);

            const btnUpd = document.createElement("button");
            btnUpd.textContent = "Actualizar Descripción";
            btnUpd.className = "btn btn-sm btn-upd-desc";
            btnUpd.onclick = () => showUpdateForm(item.id);
            divItem.appendChild(btnUpd);

            const btnDel = document.createElement("button");
            btnDel.textContent = "Eliminar";
            btnDel.className = "btn btn-sm btn-delete";
            btnDel.onclick = () => deleteItem(item.id);
            divItem.appendChild(btnDel);

            container.appendChild(divItem);
        });
    } catch (err) {
        console.error(err);
    }
}

// ──────────────────────────────────────────
// 4) Crear un nuevo TodoItem (POST)
// ──────────────────────────────────────────
async function createItem(title, description, category) {
    const msgDiv = document.getElementById("create-msg");
    msgDiv.textContent = "";
    msgDiv.className = "message";

    try {
        const res = await fetch(apiBase, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ title, description, category }),
        });

        if (res.ok) {
            msgDiv.textContent = "Creado correctamente.";
            msgDiv.classList.add("success");

            // Limpiar inputs
            document.getElementById("new-title").value = "";
            document.getElementById("new-desc").value = "";
            document.getElementById("new-category").value = "";

            // Hacer desaparecer el mensaje a los 3s
            setTimeout(() => {
                msgDiv.textContent = "";
                msgDiv.className = "message";
            }, 3000);

            // Refrescamos la lista (ahora sí habrá al menos un elemento)
            fetchItems();
        } else {
            // Si viene JSON con { error: "..." }, lo mostramos
            let errorMsg = "Error desconocido al crear.";
            try {
                const data = await res.json();
                if (data.error) errorMsg = data.error;
            } catch { }
            msgDiv.textContent = errorMsg;
            msgDiv.classList.add("error");
        }
    } catch (err) {
        msgDiv.textContent = "Error al crear ítem.";
        msgDiv.classList.add("error");
    }
}

// ──────────────────────────────────────────
// 5) Mostrar / Ocultar formulario de Progresión
// ──────────────────────────────────────────
function showProgressionForm(itemId) {
    // Buscamos el ToDoItem en el cache local
    const todo = itemsCache.find((x) => x.id === Number(itemId));

    // Calculamos la fecha mínima para el nuevo registro (día siguiente a la última fecha)
    let minDateString = "";
    if (
        todo &&
        Array.isArray(todo.progressions) &&
        todo.progressions.length > 0
    ) {
        const fechas = todo.progressions.map((p) => new Date(p.date));
        const maxTime = Math.max(...fechas.map((d) => d.getTime()));
        const maxFecha = new Date(maxTime);
        maxFecha.setDate(maxFecha.getDate() + 1);
        minDateString = maxFecha.toISOString().slice(0, 10);
    } else {
        // Si no tiene progresiones, la fecha mínima es hoy
        minDateString = new Date().toISOString().slice(0, 10);
    }

    document.getElementById("prog-item-id").textContent = itemId;
    document.getElementById("prog-parent-id").value = itemId;

    // Fijar atributo "min" en el input[type=date]
    const dateInput = document.getElementById("prog-date");
    dateInput.min = minDateString;
    dateInput.value = ""; // lo dejamos vacío para que el usuario elija la fecha

    document.getElementById("prog-percent").value = "";
    document.getElementById("prog-msg").textContent = "";

    // Mostramos la sección de Progresión y ocultamos la de Actualizar (por si estaba abierta)
    document.getElementById("update-section").classList.add("hidden");
    document.getElementById("progression-section").classList.remove("hidden");
}

function hideProgressionForm() {
    document.getElementById("progression-section").classList.add("hidden");
}

// ──────────────────────────────────────────
// 6) Registrar Progresión (POST /{id}/progressions)
// ──────────────────────────────────────────
async function registerProgression(id, date, percent) {
    const msgDiv = document.getElementById("prog-msg");
    msgDiv.textContent = "";
    msgDiv.className = "message";

    try {
        const res = await fetch(`${apiBase}/${id}/progressions`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ date, percent }),
        });

        if (res.ok) {
            hideProgressionForm();
            fetchItems();
        } else {
            // Si viene JSON con { error: "..." }
            let errorMsg = "Error desconocido al registrar progresión.";
            try {
                const data = await res.json();
                if (data.error) errorMsg = data.error;
            } catch { }
            msgDiv.textContent = errorMsg;
            msgDiv.classList.add("error");
        }
    } catch (err) {
        msgDiv.textContent = "Error al registrar progresión.";
        msgDiv.classList.add("error");
    }
}

// ──────────────────────────────────────────
// 7) Mostrar / Ocultar formulario de Actualizar Descripción
// ──────────────────────────────────────────
function showUpdateForm(itemId) {
    document.getElementById("upd-item-id").textContent = itemId;
    document.getElementById("upd-parent-id").value = itemId;
    document.getElementById("upd-new-desc").value = "";
    document.getElementById("upd-msg").textContent = "";

    // Ocultamos la sección de Progresión por si estaba abierta
    document.getElementById("progression-section").classList.add("hidden");
    document.getElementById("update-section").classList.remove("hidden");
}

function hideUpdateForm() {
    document.getElementById("update-section").classList.add("hidden");
}

// ──────────────────────────────────────────
// 8) Actualizar Descripción (PUT /{id})
// ──────────────────────────────────────────
async function updateDescription(id, newDescription) {
    const msgDiv = document.getElementById("upd-msg");
    msgDiv.textContent = "";
    msgDiv.className = "message";

    try {
        const res = await fetch(`${apiBase}/${id}`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ newDescription }),
        });

        if (res.ok) {
            hideUpdateForm();
            fetchItems();
        } else {
            let errorMsg = "Error desconocido al actualizar descripción.";
            try {
                const data = await res.json();
                if (data.error) errorMsg = data.error;
            } catch { }
            msgDiv.textContent = errorMsg;
            msgDiv.classList.add("error");
        }
    } catch (err) {
        msgDiv.textContent = "Error al actualizar descripción.";
        msgDiv.classList.add("error");
    }
}

// ──────────────────────────────────────────
// 9) Eliminar ítem (DELETE /{id})
// ──────────────────────────────────────────
async function deleteItem(id) {
    // Si el ítem tiene más de 50% de progreso, asumimos que el backend
    // responderá con status != 200 y un JSON { error: "..." }
    if (!confirm("¿Seguro que desea eliminar el ítem " + id + "?")) return;

    try {
        const res = await fetch(`${apiBase}/${id}`, {
            method: "DELETE",
        });
        if (res.ok) {
            fetchItems();
        } else {
            let errorMsg = "Error desconocido al eliminar.";
            try {
                const data = await res.json();
                if (data.error) errorMsg = data.error;
            } catch { }
            alert(errorMsg);
        }
    } catch (err) {
        console.error(err);
    }
}
