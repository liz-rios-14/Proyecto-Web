import { useEffect, useRef, useState } from "react";
import Layout from "../components/Layout";
import DataTable from "../components/DataTable";
import Pagination from "../components/Pagination";
import useKeyboardShortcuts from "../hooks/useKeyboardShortcuts";
import { api } from "../api/apiClient";
import { getApiErrorMessage } from "../api/apiError";
import { useAppAlert } from "../components/AppAlert";
import { isValidEcuadorianCedula } from "../utils/ecuadorianCedula";
import { getAuthRole } from "../services/authStorage";

const emptyCustomerForm = {
  firstName: "",
  lastName: "",
  cedula: "",
  phone: "",
  address: "",
  email: "",
};

const customerColumns = [
  { key: "id", label: "Id", type: "number" },
  { key: "firstName", label: "Nombre" },
  { key: "lastName", label: "Apellido" },
  { key: "cedula", label: "Cédula" },
  { key: "phone", label: "Teléfono" },
  { key: "address", label: "Dirección" },
  { key: "email", label: "Correo" },
];

const customerSearchFields = [
  { key: "id", label: "Id", type: "number" },
  { key: "firstName", label: "Nombre" },
  { key: "lastName", label: "Apellido" },
  { key: "cedula", label: "Cédula", type: "number" },
  { key: "phone", label: "Teléfono" },
  { key: "address", label: "Dirección" },
  { key: "email", label: "Correo" },
];

export default function CustomerManagerPage() {
  const { showAlert, showConfirm } = useAppAlert();
  const isAdministrator = getAuthRole() === "ADMINISTRATOR";
  const [customers, setCustomers] = useState([]);
  const [form, setForm] = useState(emptyCustomerForm);
  const [editingId, setEditingId] = useState(null);
  const [searchField, setSearchField] = useState("firstName");
  const [searchValue, setSearchValue] = useState("");
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  const saveButtonRef = useRef(null);
  const cleanButtonRef = useRef(null);
  const searchButtonRef = useRef(null);
  const searchInputRef = useRef(null);

  const loadCustomers = async (currentPage = page) => {
    try {
      const params = {
        pageNumber: currentPage,
        pageSize: 8,
      };

      if (searchField && searchValue.trim()) {
        params.field = searchField;
        params.value = searchValue.trim();
      }

      const response = await api.get("/customers/search", { params });

      setCustomers(response.data.items ?? []);
      setTotalPages(Math.max(response.data.totalPages ?? 1, 1));
    } catch (error) {
      setCustomers([]);
      setTotalPages(1);
      showAlert(
        getApiErrorMessage(error, "No se pudo cargar la lista de clientes."),
        "error"
      );
    }
  };

  useEffect(() => {
    loadCustomers(page);
  }, [page]);

  useEffect(() => {
    const timer = setTimeout(() => {
      setPage(1);
      loadCustomers(1);
    }, 300);

    return () => clearTimeout(timer);
  }, [searchField, searchValue]);

  const handleSearchValueChange = (event) => {
    const value = event.target.value;

    if (searchField === "id" || searchField === "phone" || searchField === "cedula") {
      setSearchValue(value.replace(/\D/g, ""));
      return;
    }

    if (searchField === "email") {
      setSearchValue(value.toLowerCase().slice(0, 120));
      return;
    }

    setSearchValue(value.toUpperCase().slice(0, 150));
  };

  const handleChange = (event) => {
    const { name, value } = event.target;

    if (name === "firstName" || name === "lastName") {
      setForm({
        ...form,
        [name]: value
          .replace(/[^a-zA-ZáéíóúÁÉÍÓÚñÑ\s]/g, "")
          .toUpperCase()
          .slice(0, 40),
      });
      return;
    }

    if (name === "phone") {
      setForm({ ...form, phone: value.replace(/\D/g, "").slice(0, 10) });
      return;
    }

    if (name === "cedula") {
      setForm({ ...form, cedula: value.replace(/\D/g, "").slice(0, 10) });
      return;
    }

    if (name === "address") {
      setForm({ ...form, address: value.toUpperCase().slice(0, 150) });
      return;
    }

    if (name === "email") {
      setForm({ ...form, email: value.toLowerCase().slice(0, 120) });
      return;
    }

    setForm({ ...form, [name]: value });
  };

  const resetForm = () => {
    setForm(emptyCustomerForm);
    setEditingId(null);
  };

  const saveCustomer = async () => {
    const request = {
      firstName: form.firstName.trim().replace(/\s+/g, " ").toUpperCase(),
      lastName: form.lastName.trim().replace(/\s+/g, " ").toUpperCase(),
      cedula: form.cedula.trim(),
      phone: form.phone.trim(),
      address: form.address.trim().replace(/\s+/g, " ").toUpperCase(),
      email: form.email.trim().toLowerCase(),
    };

    if (!request.firstName || !request.lastName || !request.cedula || !request.phone || !request.address || !request.email) {
      showAlert("Complete todos los campos.", "warning");
      return;
    }

    if (request.firstName.length < 2 || request.firstName.length > 40) {
      showAlert("El nombre debe tener entre 2 y 40 caracteres.", "warning");
      return;
    }

    if (request.lastName.length < 2 || request.lastName.length > 40) {
      showAlert("El apellido debe tener entre 2 y 40 caracteres.", "warning");
      return;
    }

    if (!/^[A-ZÁÉÍÓÚÑ\s]+$/.test(request.firstName)) {
      showAlert("El nombre solo debe contener letras.", "warning");
      return;
    }

    if (!/^[A-ZÁÉÍÓÚÑ\s]+$/.test(request.lastName)) {
      showAlert("El apellido solo debe contener letras.", "warning");
      return;
    }

    if (!isValidEcuadorianCedula(request.cedula)) {
      showAlert("Ingrese una cédula ecuatoriana válida.", "warning");
      return;
    }

    if (!/^09\d{8}$/.test(request.phone)) {
      showAlert("Ingrese un número de teléfono válido.", "warning");
      return;
    }

    if (request.address.length < 5 || request.address.length > 150) {
      showAlert("La dirección debe tener entre 5 y 150 caracteres.", "warning");
      return;
    }

    if (!/^[A-ZÁÉÍÓÚÜÑ0-9 .,#-]+$/.test(request.address)) {
      showAlert("La dirección contiene caracteres no permitidos.", "warning");
      return;
    }

    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(request.email)) {
      showAlert("Ingrese un correo válido.", "warning");
      return;
    }

    try {
      const [emailResponse, cedulaResponse] = await Promise.all([
        api.get("/customers/search", {
          params: {
            field: "email",
            value: request.email,
            pageNumber: 1,
            pageSize: 10,
          },
        }),
        api.get("/customers/search", {
          params: {
            field: "cedula",
            value: request.cedula,
            pageNumber: 1,
            pageSize: 10,
          },
        }),
      ]);

      const duplicateCustomer = (emailResponse.data.items ?? []).find(
        (customer) =>
          customer.email?.trim().toLowerCase() === request.email &&
          customer.id !== editingId
      );

      if (duplicateCustomer) {
        showAlert(
          editingId
            ? "Ya existe otro cliente con el mismo correo."
            : "Ya existe un cliente con el mismo correo.",
          "warning"
        );
        return;
      }

      const duplicateCedula = (cedulaResponse.data.items ?? []).find(
        (customer) =>
          customer.cedula === request.cedula &&
          customer.id !== editingId
      );

      if (duplicateCedula) {
        showAlert(
          editingId
            ? "Ya existe otro cliente registrado con esta cédula."
            : "Ya existe un cliente registrado con esta cédula.",
          "warning"
        );
        return;
      }

      if (editingId) {
        await api.put(`/customers/${editingId}`, request);
        showAlert("Cliente actualizado correctamente.", "success");
      } else {
        await api.post("/customers", request);
        showAlert("Cliente creado correctamente.", "success");
      }

      resetForm();
      loadCustomers(page);
    } catch (error) {
      console.error(error);
      showAlert(
        getApiErrorMessage(error, "No se pudo guardar el cliente."),
        "error"
      );
    }
  };

  const editCustomer = (customer) => {
    setEditingId(customer.id);

    setForm({
      firstName: customer.firstName,
      lastName: customer.lastName,
      cedula: customer.cedula ?? "",
      phone: customer.phone,
      address: customer.address,
      email: customer.email,
    });
  };

  const deleteCustomer = async (id) => {
    const confirmed = await showConfirm(
      "¿Seguro que desea eliminar este cliente?",
      {
        title: "Eliminar cliente",
        confirmText: "Sí, continuar",
      }
    );

    if (!confirmed) return;

    const finalConfirmation = await showConfirm(
      "Si el cliente tiene historial se desactivará para conservar la auditoría. Si no tiene historial se eliminará físicamente. ¿Desea continuar?",
      {
        title: "Confirmación final",
        confirmText: "Confirmar",
      }
    );

    if (!finalConfirmation) return;

    try {
      const response = await api.delete(`/customers/${id}`);
      showAlert(response.data?.message ?? "Cliente procesado correctamente.", "success");

      if (customers.length === 1 && page > 1) {
        setPage(page - 1);
      } else {
        loadCustomers(page);
      }
    } catch (error) {
      console.error(error);
      showAlert(
        getApiErrorMessage(error, "No se pudo eliminar el cliente."),
        "error"
      );
    }
  };

  useKeyboardShortcuts(
    [
      { ctrl: true, key: "b", action: () => searchInputRef.current?.focus() },
      { ctrl: true, key: "g", action: saveCustomer },
      { ctrl: true, key: "l", action: resetForm },
      { alt: true, key: "g", action: () => saveButtonRef.current?.focus() },
      { alt: true, key: "l", action: () => cleanButtonRef.current?.focus() },
      { alt: true, key: "b", action: () => searchButtonRef.current?.focus() },
    ],
    [form, editingId, searchField, searchValue]
  );

  return (
    <Layout>
      <h1>Gestor Clientes</h1>

      <div className="card">
        <h2>{editingId ? "Editar cliente" : "Nuevo cliente"}</h2>

        <div className="form-grid">
          <input name="firstName" placeholder="Nombre" maxLength="40" value={form.firstName} onChange={handleChange} />
          <input name="lastName" placeholder="Apellido" maxLength="40" value={form.lastName} onChange={handleChange} />
          <input name="cedula" placeholder="Cédula ecuatoriana" inputMode="numeric" maxLength="10" value={form.cedula} onChange={handleChange} />
          <input name="phone" placeholder="Teléfono" inputMode="numeric" maxLength="10" value={form.phone} onChange={handleChange} />
          <input name="address" placeholder="Dirección" maxLength="150" value={form.address} onChange={handleChange} />
          <input name="email" placeholder="Correo" type="email" maxLength="120" value={form.email} onChange={handleChange} />
        </div>

        <div className="actions-row">
          <button ref={saveButtonRef} onClick={saveCustomer}>
            {editingId ? "💾 Actualizar" : "💾 Guardar"}
          </button>

          <button ref={cleanButtonRef} className="secondary-button" onClick={resetForm}>
            🧹 Limpiar
          </button>
        </div>
      </div>

      <div className="card">
        <h2>Buscar clientes</h2>

        <div className="search-grid">
          <select
            value={searchField}
            onChange={(event) => {
              setSearchField(event.target.value);
              setSearchValue("");
            }}
          >
            {customerSearchFields.map((field) => (
              <option key={field.key} value={field.key}>
                Buscar por {field.label}
              </option>
            ))}
          </select>

          <input
            ref={searchInputRef}
            placeholder="Ingrese valor de búsqueda"
            type={searchField === "id" || searchField === "cedula" ? "number" : "text"}
            value={searchValue}
            onChange={handleSearchValueChange}
          />

          <button ref={searchButtonRef} onClick={() => loadCustomers(1)}>
            🔍 Buscar
          </button>
        </div>
      </div>

      <div className="card">
        <h2>Listado de clientes</h2>

        <DataTable
          columns={customerColumns}
          data={customers}
          emptyMessage="Sin clientes registrados"
          actions={(customer) => (
            <>
              {isAdministrator && (
                <>
                  <button className="table-action edit-button" onClick={() => editCustomer(customer)}>
                    ✏️ Editar
                  </button>

                  <button className="table-action delete-button" onClick={() => deleteCustomer(customer.id)}>
                    🗑️ Eliminar / desactivar
                  </button>
                </>
              )}
            </>
          )}
        />

        <Pagination
          page={page}
          totalPages={totalPages}
          onPrevious={() => setPage(page - 1)}
          onNext={() => setPage(page + 1)}
          onPageChange={(newPage) => setPage(newPage)}
        />
      </div>
    </Layout>
  );
}
