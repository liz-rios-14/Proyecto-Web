import { useEffect, useMemo, useState } from "react";
import Layout from "../components/Layout";
import DataTable from "../components/DataTable";
import { api } from "../api/apiClient";
import { getApiErrorMessage } from "../api/apiError";
import { useAppAlert } from "../components/AppAlert";
import { getRoleLabel } from "../services/roleLabels";

const emptyForm = {
  fullName: "",
  userName: "",
  email: "",
  password: "",
  confirmPassword: "",
  roleId: "",
  isActive: true,
};

const columns = [
  { key: "id", label: "Id", type: "number" },
  { key: "fullName", label: "Nombre completo" },
  { key: "userName", label: "Usuario" },
  { key: "email", label: "Correo" },
  { key: "roleLabel", label: "Rol" },
  { key: "statusLabel", label: "Estado" },
  { key: "accessLabel", label: "Acceso" },
];

export default function UserManagerPage() {
  const { showAlert, showConfirm } = useAppAlert();
  const [users, setUsers] = useState([]);
  const [roles, setRoles] = useState([]);
  const [form, setForm] = useState(emptyForm);
  const [editingId, setEditingId] = useState(null);
  const [search, setSearch] = useState("");

  const loadData = async () => {
    try {
      const [usersResponse, rolesResponse] = await Promise.all([
        api.get("/users"),
        api.get("/roles"),
      ]);

      setUsers(usersResponse.data ?? []);
      setRoles(rolesResponse.data ?? []);
    } catch (error) {
      showAlert(
        getApiErrorMessage(error, "No se pudieron cargar los usuarios."),
        "error"
      );
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const visibleUsers = useMemo(() => {
    const value = search.trim().toLowerCase();
    const mappedUsers = users.map((user) => ({
      ...user,
      roleLabel: getRoleLabel(user.roleName),
      statusLabel: user.isActive ? "ACTIVO" : "INACTIVO",
      accessLabel: user.isLocked
        ? `BLOQUEADO (${user.failedLoginAttempts}/3)`
        : "HABILITADO",
    }));

    if (!value) return mappedUsers;

    return mappedUsers.filter((user) =>
      [user.fullName, user.userName, user.email, user.roleName]
        .filter(Boolean)
        .some((field) => field.toLowerCase().startsWith(value))
    );
  }, [search, users]);

  const resetForm = () => {
    setForm(emptyForm);
    setEditingId(null);
  };

  const handleChange = (event) => {
    const { name, value } = event.target;
    setForm((current) => ({
      ...current,
      [name]: name === "isActive" ? value === "true" : value,
    }));
  };

  const validate = () => {
    if (!form.fullName.trim() || !form.userName.trim() || !form.email.trim()) {
      return "Complete nombre, usuario y correo.";
    }
    if (form.fullName.trim().length < 3) {
      return "El nombre completo debe tener al menos 3 caracteres.";
    }
    if (form.userName.trim().length < 3) {
      return "El usuario debe tener al menos 3 caracteres.";
    }
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email.trim())) {
      return "Ingrese un correo válido.";
    }
    if (!form.roleId) {
      return "Seleccione un rol.";
    }
    if (!editingId) {
      if (form.password.length < 8 || form.password.length > 10) {
        return "La contraseña debe tener entre 8 y 10 caracteres.";
      }
      if (
        !/[A-Z]/.test(form.password) ||
        !/[a-z]/.test(form.password) ||
        !/[0-9]/.test(form.password) ||
        !/[^a-zA-Z0-9]/.test(form.password)
      ) {
        return "La contraseña debe incluir mayúscula, minúscula, número y carácter especial.";
      }
      if (form.password !== form.confirmPassword) {
        return "Las contraseñas no coinciden.";
      }
    }
    return "";
  };

  const saveUser = async () => {
    const validationMessage = validate();
    if (validationMessage) {
      showAlert(validationMessage, "warning");
      return;
    }

    const request = {
      fullName: form.fullName.trim(),
      userName: form.userName.trim(),
      email: form.email.trim(),
      roleId: Number(form.roleId),
      isActive: form.isActive,
    };

    try {
      if (editingId) {
        await api.put(`/users/${editingId}`, request);
        showAlert("Usuario actualizado correctamente.", "success");
      } else {
        await api.post("/users", { ...request, password: form.password });
        showAlert("Usuario creado correctamente.", "success");
      }

      resetForm();
      await loadData();
    } catch (error) {
      showAlert(
        getApiErrorMessage(error, "No se pudo guardar el usuario."),
        "error"
      );
    }
  };

  const editUser = (user) => {
    setEditingId(user.id);
    setForm({
      fullName: user.fullName,
      userName: user.userName,
      email: user.email,
      password: "",
      confirmPassword: "",
      roleId: String(user.roleId),
      isActive: user.isActive,
    });
    window.scrollTo({ top: 0, behavior: "smooth" });
  };

  const disableUser = async (user) => {
    const confirmed = await showConfirm(
      `¿Desea desactivar al usuario ${user.userName}?`,
      {
        title: "Desactivar usuario",
        confirmText: "Sí, desactivar",
      }
    );

    if (!confirmed) return;

    try {
      await api.post(`/users/${user.id}/disable`);
      showAlert("Usuario desactivado correctamente.", "success");
      await loadData();
    } catch (error) {
      showAlert(
        getApiErrorMessage(error, "No se pudo desactivar el usuario."),
        "error"
      );
    }
  };

  const unlockUser = async (user) => {
    const confirmed = await showConfirm(
      `¿Desea desbloquear al usuario ${user.userName}?`,
      {
        title: "Desbloquear usuario",
        confirmText: "Sí, desbloquear",
      }
    );

    if (!confirmed) return;

    try {
      await api.post(`/users/${user.id}/unlock`);
      showAlert("Usuario desbloqueado correctamente.", "success");
      await loadData();
    } catch (error) {
      showAlert(
        getApiErrorMessage(error, "No se pudo desbloquear el usuario."),
        "error"
      );
    }
  };

  return (
    <Layout>
      <h1>Gestor de usuarios</h1>

      <div className="card">
        <h2>{editingId ? "Editar usuario" : "Nuevo usuario"}</h2>

        <div className="form-grid">
          <input name="fullName" maxLength="120" placeholder="Nombre completo" value={form.fullName} onChange={handleChange} />
          <input name="userName" maxLength="60" placeholder="Nombre de usuario" value={form.userName} onChange={handleChange} />
          <input name="email" type="email" maxLength="120" placeholder="Correo" value={form.email} onChange={handleChange} />
          <select name="roleId" value={form.roleId} onChange={handleChange}>
            <option value="">Seleccione un rol</option>
            {roles.filter((role) => role.isActive).map((role) => (
              <option key={role.id} value={role.id}>
                {getRoleLabel(role.name)}
              </option>
            ))}
          </select>

          {!editingId && (
            <>
              <input name="password" type="password" maxLength="10" placeholder="Contraseña" value={form.password} onChange={handleChange} />
              <input name="confirmPassword" type="password" maxLength="10" placeholder="Confirmar contraseña" value={form.confirmPassword} onChange={handleChange} />
            </>
          )}

          {editingId && (
            <select name="isActive" value={String(form.isActive)} onChange={handleChange}>
              <option value="true">Activo</option>
              <option value="false">Inactivo</option>
            </select>
          )}
        </div>

        {!editingId && (
          <p className="field-help">
            La contraseña debe tener de 8 a 10 caracteres, mayúscula, minúscula,
            número y carácter especial.
          </p>
        )}

        <div className="actions-row">
          <button type="button" onClick={saveUser}>
            {editingId ? "Actualizar" : "Guardar"}
          </button>
          <button type="button" className="secondary-button" onClick={resetForm}>
            Limpiar
          </button>
        </div>
      </div>

      <div className="card">
        <h2>Buscar usuarios</h2>
        <input
          placeholder="Buscar por nombre, usuario, correo o rol"
          value={search}
          onChange={(event) => setSearch(event.target.value)}
        />
      </div>

      <div className="card">
        <h2>Listado de usuarios</h2>
        <DataTable
          columns={columns}
          data={visibleUsers}
          emptyMessage="Sin usuarios registrados"
          actions={(user) => (
            <>
              <button className="table-action edit-button" onClick={() => editUser(user)}>
                Editar
              </button>
              {user.isLocked && (
                <button className="table-action unlock-button" onClick={() => unlockUser(user)}>
                  Desbloquear
                </button>
              )}
              {user.isActive && user.roleName !== "ADMINISTRATOR" && (
                <button className="table-action delete-button" onClick={() => disableUser(user)}>
                  Desactivar
                </button>
              )}
            </>
          )}
        />
      </div>
    </Layout>
  );
}
