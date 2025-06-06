// File: wwwroot/js/app.js

const apiBase = "/api/todolist";

// ------------------------------------------------------------
// 1) Al cargar la página, obtenemos y mostramos los ítems
// ------------------------------------------------------------
window.addEventListener("DOMContentLoaded", () => {
    fetchItems();

    // Manejador del formulario “Crear”
    document.getElementById("create-form").addEventListener("submit", e => {
        e.preventDefault(); // cancelar envío tradicional
        const title = document.getElementById("new-title").value.trim();
        const desc = document.getElementById("new-desc").value.trim();
        const category = document.getElementById("new-category").value;
        createItem(title, desc, category);
    });

    // Cancelar y enviar de formularios ocultos:
    document.getElementById("prog-cancel").addEventListener("click", () => {
        hideProgressionForm();
    });
    document.getElementById("prog-cancel").type = "button"; // no submit

    document.getElementById("progression-form").addEventListener("submit", e => {
        e.preventDefault();
        const id = document.getElementById("prog-parent-id").value;
        const date = document.getElementById("prog-date").value;
        const percent = parseFloat(document.getElementById("prog-percent").value);
        registerProgression(id, date, percent);
    });

    document.getElementById("upd-cancel").addEventListener("click", () => {
        hideUpdateForm();
    });
    document.getElementById("upd-cancel").type = "button";

    document.getElementById("update-form").addEventListener("submit", e => {
        e.preventDefault();
        const id = document.getElementById("upd-parent-id").value;
        const newDesc = document.getElementById("upd-new-desc").value.trim();
        updateDescription(id, newDesc);
    });
});

// ------------------------------------------------------------
// 2) Obtener y desplegar la lista de TodoItems
// ------------------------------------------------------------
async function fetchItems() {
    const container = document.getElementById("items-container");
    container.innerHTML = "";

    try {
        const res = await fetch(apiBase);
        const items = await res.json();

        items.forEach(item => {
            // Cada ítem en un <div class="todo-item">
            const div = document.createElement("div");
            div.className = "todo-item";

            // Cabecera
            const header = document.createElement("div");
            header.innerHTML = `
                <strong>${item.id}) ${item.title}</strong>
                - ${item.description} (<em>${item.category}</em>)
                Completed: ${item.isCompleted}`;
            div.appendChild(header);

            // Progresiones (usamos prog.percent en lugar de prog.accumulatedPercent)
            item.progressions.forEach(prog => {
                const p = document.createElement("p");
                // Mostramos fecha y el porcentaje exacto de esta progresión
                p.textContent = `${new Date(prog.date).toLocaleString()} - ${prog.percent}%`;
                div.appendChild(p);

                const barContainer = document.createElement("div");
                barContainer.className = "progress-bar";

                const barInner = document.createElement("div");
                barInner.className = "progress-bar-inner";
                // Aquí usamos prog.percent
                barInner.style.width = `${prog.percent}%`;
                barInner.textContent = `${prog.percent}%`;

                barContainer.appendChild(barInner);
                div.appendChild(barContainer);
            });

            // Botones “Añadir Progresión”, “Actualizar Descripción” y “Eliminar”
            const btnProg = document.createElement("button");
            btnProg.textContent = "Añadir Progresión";
            btnProg.onclick = () => showProgressionForm(item.id);
            div.appendChild(btnProg);

            const btnUpd = document.createElement("button");
            btnUpd.textContent = "Actualizar Descripción";
            btnUpd.onclick = () => showUpdateForm(item.id);
            div.appendChild(btnUpd);

            const btnDel = document.createElement("button");
            btnDel.textContent = "Eliminar";
            btnDel.onclick = () => deleteItem(item.id);
            div.appendChild(btnDel);

            container.appendChild(div);
        });
    } catch (err) {
        console.error(err);
    }
}

// ------------------------------------------------------------
// 3) Crear un nuevo TodoItem (POST)
// ------------------------------------------------------------
async function createItem(title, description, category) {
    const msgDiv = document.getElementById("create-msg");
    msgDiv.textContent = "";
    msgDiv.className = "message";

    try {
        const res = await fetch(apiBase, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ title, description, category })
        });
        if (res.ok) {
            msgDiv.textContent = "Creado correctamente.";
            document.getElementById("new-title").value = "";
            document.getElementById("new-desc").value = "";
            fetchItems();
        } else {
            // Intentamos leer JSON y extraer la propiedad `error`
            let errorMsg = "Error desconocido al crear.";
            try {
                const data = await res.json();
                if (data.error) {
                    errorMsg = data.error;
                }
            } catch {
                /* no hacemos nada, mantenemos el mensaje por defecto */
            }
            msgDiv.textContent = errorMsg;
            msgDiv.className = "message error";
        }
    } catch (err) {
        msgDiv.textContent = "Error al crear ítem.";
        msgDiv.className = "message error";
    }
}

// ------------------------------------------------------------
// 4) Mostrar/Ocultar formulario de Progresión
// ------------------------------------------------------------
function showProgressionForm(itemId) {
    document.getElementById("prog-item-id").textContent = itemId;
    document.getElementById("prog-parent-id").value = itemId;
    document.getElementById("prog-date").value = "";     // limpiar
    document.getElementById("prog-percent").value = "";
    document.getElementById("prog-msg").textContent = "";
    document.getElementById("progression-section").classList.remove("hidden");
}

function hideProgressionForm() {
    document.getElementById("progression-section").classList.add("hidden");
}

// ------------------------------------------------------------
// 5) Registrar Progresión (POST /{id}/progressions)
// ------------------------------------------------------------
async function registerProgression(id, date, percent) {
    const msgDiv = document.getElementById("prog-msg");
    msgDiv.textContent = "";
    msgDiv.className = "message";

    try {
        const res = await fetch(`${apiBase}/${id}/progressions`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ date, percent })
        });
        if (res.ok) {
            hideProgressionForm();
            fetchItems();
        } else {
            // Extraemos el JSON { error: "..." } y mostramos solo el texto
            let errorMsg = "Error desconocido al registrar progresión.";
            try {
                const data = await res.json();
                if (data.error) {
                    errorMsg = data.error;
                }
            } catch {
                /* fallback */
            }
            msgDiv.textContent = errorMsg;
            msgDiv.className = "message error";
        }
    } catch (err) {
        msgDiv.textContent = "Error al registrar progresión.";
        msgDiv.className = "message error";
    }
}

// ------------------------------------------------------------
// 6) Mostrar/Ocultar formulario de Actualizar Descripción
// ------------------------------------------------------------
function showUpdateForm(itemId) {
    document.getElementById("upd-item-id").textContent = itemId;
    document.getElementById("upd-parent-id").value = itemId;
    document.getElementById("upd-new-desc").value = "";  // limpiar
    document.getElementById("upd-msg").textContent = "";
    document.getElementById("update-section").classList.remove("hidden");
}

function hideUpdateForm() {
    document.getElementById("update-section").classList.add("hidden");
}

// ------------------------------------------------------------
// 7) Actualizar Descripción (PUT /{id})
// ------------------------------------------------------------
async function updateDescription(id, newDescription) {
    const msgDiv = document.getElementById("upd-msg");
    msgDiv.textContent = "";
    msgDiv.className = "message";

    try {
        const res = await fetch(`${apiBase}/${id}`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ newDescription })
        });
        if (res.ok) {
            hideUpdateForm();
            fetchItems();
        } else {
            // Extraemos el JSON { error: "..." } y mostramos solo esa cadena
            let errorMsg = "Error desconocido al actualizar descripción.";
            try {
                const data = await res.json();
                if (data.error) {
                    errorMsg = data.error;
                }
            } catch {
                /* fallback */
            }
            msgDiv.textContent = errorMsg;
            msgDiv.className = "message error";
        }
    } catch (err) {
        msgDiv.textContent = "Error al actualizar descripción.";
        msgDiv.className = "message error";
    }
}

// ------------------------------------------------------------
// 8) Eliminar ítem (DELETE /{id})
// ------------------------------------------------------------
async function deleteItem(id) {
    if (!confirm("¿Seguro que desea eliminar el ítem " + id + "?")) return;

    try {
        const res = await fetch(`${apiBase}/${id}`, { method: "DELETE" });
        if (res.ok) {
            fetchItems();
        } else {
            // Extraemos el JSON { error: "..." } si existe
            let errorMsg = "Error desconocido al eliminar.";
            try {
                const data = await res.json();
                if (data.error) {
                    errorMsg = data.error;
                }
            } catch {
                /* fallback: si no vienen JSON */
            }
            alert("Error: " + errorMsg);
        }
    } catch (err) {
        console.error(err);
    }
}
