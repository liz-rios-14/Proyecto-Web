import { useEffect, useRef, useState } from "react";
import Layout from "../components/Layout";
import DataTable from "../components/DataTable";
import Pagination from "../components/Pagination";
import useKeyboardShortcuts from "../hooks/useKeyboardShortcuts";
import { api } from "../api/apiClient";

const emptyCustomerForm = {
  firstName: "",
  lastName: "",
  phone: "",
  address: "",
  email: "",
};

const customerColumns = [
  { key: "id", label: "Id", type: "number" },
  { key: "firstName", label: "Nombre" },
  { key: "lastName", label: "Apellido" },
  { key: "phone", label: "Teléfono" },
  { key: "address", label: "Dirección" },
  { key: "email", label: "Correo" },
];

const customerSearchFields = [
  { key: "id", label: "Id", type: "number" },
  { key: "firstName", label: "Nombre" },
  { key: "lastName", label: "Apellido" },
  { key: "phone", label: "Teléfono" },
  { key: "address", label: "Dirección" },
  { key: "email", label: "Correo" },
];

export default function CustomerManagerPage() {
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
    setTotalPages(response.data.totalPages ?? 1);
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

    if (searchField === "id" || searchField === "phone") {
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
      firstName: form.firstName.trim().toUpperCase(),
      lastName: form.lastName.trim().toUpperCase(),
      phone: form.phone.trim(),
      address: form.address.trim().toUpperCase(),
      email: form.email.trim().toLowerCase(),
    };

    if (!request.firstName || !request.lastName || !request.phone || !request.address || !request.email) {
      alert("Complete todos los campos.");
      return;
    }

    if (!/^[A-ZÁÉÍÓÚÑ\s]+$/.test(request.firstName)) {
      alert("El nombre solo debe contener letras.");
      return;
    }

    if (!/^[A-ZÁÉÍÓÚÑ\s]+$/.test(request.lastName)) {
      alert("El apellido solo debe contener letras.");
      return;
    }

    if (!/^\d{10}$/.test(request.phone)) {
      alert("El teléfono debe tener exactamente 10 dígitos.");
      return;
    }

    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(request.email)) {
      alert("Ingrese un correo válido.");
      return;
    }

    try {
      if (editingId) {
        await api.put(`/customers/${editingId}`, request);
        alert("Cliente actualizado correctamente.");
      } else {
        await api.post("/customers", request);
        alert("Cliente creado correctamente.");
      }

      resetForm();
      loadCustomers(page);
    } catch (error) {
      console.error(error);
      alert("No se pudo guardar el cliente.");
    }
  };

  const editCustomer = (customer) => {
    setEditingId(customer.id);

    setForm({
      firstName: customer.firstName,
      lastName: customer.lastName,
      phone: customer.phone,
      address: customer.address,
      email: customer.email,
    });
  };

  const deleteCustomer = async (id) => {
    if (!confirm("¿Seguro que desea eliminar este cliente?")) return;

    try {
      await api.delete(`/customers/${id}`);
      alert("Cliente eliminado correctamente.");
      loadCustomers(page);
    } catch (error) {
      console.error(error);
      alert("No se pudo eliminar el cliente.");
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
            type={searchField === "id" ? "number" : "text"}
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
              <button className="table-action edit-button" onClick={() => editCustomer(customer)}>
                ✏️ Editar
              </button>

              <button className="table-action delete-button" onClick={() => deleteCustomer(customer.id)}>
                🗑️ Eliminar
              </button>
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