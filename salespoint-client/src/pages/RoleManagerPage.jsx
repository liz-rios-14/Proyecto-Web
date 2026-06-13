import { useEffect, useState } from "react";
import Layout from "../components/Layout";
import DataTable from "../components/DataTable";
import { api } from "../api/apiClient";
import { getApiErrorMessage } from "../api/apiError";
import { useAppAlert } from "../components/AppAlert";
import { useUnsavedChanges } from "../components/UnsavedChangesContext";
import { getRoleLabel } from "../services/roleLabels";

const emptyForm = {
  name: "",
  description: "",
  isActive: true,
};

const columns = [
  { key: "id", label: "Id", type: "number" },
  { key: "displayName", label: "Nombre" },
  { key: "description", label: "Descripción" },
  { key: "statusLabel", label: "Estado" },
];

export default function RoleManagerPage() {
  const { showAlert, showConfirm } = useAppAlert();
  const [roles, setRoles] = useState([]);
  const [form, setForm] = useState(emptyForm);
  const [formBaseline, setFormBaseline] = useState(emptyForm);
  const [editingId, setEditingId] = useState(null);
  const isDirty = JSON.stringify(form) !== JSON.stringify(formBaseline);
  useUnsavedChanges(isDirty);

  const loadRoles = async () => {
    try {
      const response = await api.get("/roles");
      setRoles(response.data ?? []);
    } catch (error) {
      showAlert(
        getApiErrorMessage(error, "No se pudieron cargar los roles."),
        "error"
      );
    }
  };

  useEffect(() => {
    loadRoles();
  }, []);

  const clearForm = () => {
    setForm(emptyForm);
    setFormBaseline(emptyForm);
    setEditingId(null);
  };

  const resetForm = async () => {
    if (isDirty) {
      const confirmed = await showConfirm(
        "Se perderán los datos ingresados. ¿Desea limpiar el formulario?",
        { title: "Limpiar formulario", confirmText: "Sí, limpiar" }
      );
      if (!confirmed) return;
    }
    clearForm();
  };

  const saveRole = async () => {
    const name = form.name.trim().toUpperCase();
    const description = form.description.trim();

    if (name.length < 2 || name.length > 50) {
      showAlert("El nombre del rol debe tener entre 2 y 50 caracteres.", "warning");
      return;
    }
    if (!/^[A-Z0-9_-]+$/.test(name)) {
      showAlert("El nombre del rol no permite espacios ni caracteres especiales.", "warning");
      return;
    }
    if (description.length > 200) {
      showAlert("La descripción no puede superar los 200 caracteres.", "warning");
      return;
    }

    try {
      if (editingId) {
        const confirmed = await showConfirm(
          `¿Confirma actualizar el rol ${name}?`,
          { title: "Actualizar rol", confirmText: "Sí, actualizar" }
        );
        if (!confirmed) return;
        await api.put(`/roles/${editingId}`, {
          name,
          description,
          isActive: form.isActive,
        });
        showAlert("Rol actualizado correctamente.", "success");
      } else {
        await api.post("/roles", { name, description });
        showAlert("Rol creado correctamente.", "success");
      }

      clearForm();
      await loadRoles();
    } catch (error) {
      showAlert(
        getApiErrorMessage(error, "No se pudo guardar el rol."),
        "error"
      );
    }
  };

  const editRole = async (role) => {
    if (isDirty) {
      const confirmed = await showConfirm(
        "Hay cambios sin guardar. ¿Desea descartarlos y editar otro rol?",
        { title: "Cambiar de rol", confirmText: "Sí, descartar" }
      );
      if (!confirmed) return;
    }

    const nextForm = {
      name: role.name,
      description: role.description ?? "",
      isActive: role.isActive,
    };
    setEditingId(role.id);
    setForm(nextForm);
    setFormBaseline(nextForm);
  };

  return (
    <Layout>
      <h1>Gestor de roles</h1>

      <div className="card">
        <h2>{editingId ? "Editar rol" : "Nuevo rol"}</h2>

        <div className="form-grid">
          <input
            placeholder="Nombre del rol"
            maxLength="50"
            value={form.name}
            onChange={(event) =>
              setForm({
                ...form,
                name: event.target.value
                  .replace(/\s/g, "")
                  .replace(/[^a-zA-Z0-9_-]/g, "")
                  .toUpperCase(),
              })
            }
          />
          <input
            placeholder="Descripción"
            maxLength="200"
            value={form.description}
            onChange={(event) => setForm({ ...form, description: event.target.value })}
          />

          {editingId && (
            <select
              value={String(form.isActive)}
              onChange={(event) => setForm({ ...form, isActive: event.target.value === "true" })}
            >
              <option value="true">Activo</option>
              <option value="false">Inactivo</option>
            </select>
          )}
        </div>

        <div className="actions-row">
          <button type="button" onClick={saveRole}>
            {editingId ? "Actualizar" : "Guardar"}
          </button>
          <button type="button" className="secondary-button" onClick={resetForm}>
            Limpiar
          </button>
        </div>
      </div>

      <div className="card">
        <h2>Listado de roles</h2>
        <DataTable
          columns={columns}
          data={roles.map((role) => ({
            ...role,
            displayName: getRoleLabel(role.name),
            statusLabel: role.isActive ? "ACTIVO" : "INACTIVO",
          }))}
          emptyMessage="Sin roles registrados"
          actions={(role) => (
            <button className="table-action edit-button" onClick={() => editRole(role)}>
              Editar
            </button>
          )}
        />
      </div>
    </Layout>
  );
}
