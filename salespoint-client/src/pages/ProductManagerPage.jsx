import { useEffect, useRef, useState } from "react";
import Layout from "../components/Layout";
import DataTable from "../components/DataTable";
import Pagination from "../components/Pagination";
import useKeyboardShortcuts from "../hooks/useKeyboardShortcuts";
import { api } from "../api/apiClient";

const emptyProductForm = {
  name: "",
  price: "",
  stock: "",
};

const productColumns = [
  { key: "id", label: "Id", type: "number" },
  { key: "name", label: "Nombre" },
  { key: "price", label: "Precio", type: "money" },
  { key: "stock", label: "Stock", type: "number" },
];

const productSearchFields = [
  { key: "id", label: "Id", type: "number" },
  { key: "name", label: "Nombre" },
  { key: "price", label: "Precio", type: "number" },
  { key: "stock", label: "Stock", type: "number" },
];

export default function ProductManagerPage() {
  const [products, setProducts] = useState([]);
  const [form, setForm] = useState(emptyProductForm);
  const [editingId, setEditingId] = useState(null);
  const [searchField, setSearchField] = useState("name");
  const [searchValue, setSearchValue] = useState("");
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  const saveButtonRef = useRef(null);
  const cleanButtonRef = useRef(null);
  const searchButtonRef = useRef(null);
  const searchInputRef = useRef(null);

  const loadProducts = async (currentPage = page) => {
    const params = {
      pageNumber: currentPage,
      pageSize: 8,
    };

    if (searchField && searchValue.trim()) {
      params.field = searchField;
      params.value = searchValue.trim();
    }

    const response = await api.get("/products/search", { params });

    setProducts(response.data.items ?? []);
    setTotalPages(response.data.totalPages ?? 1);
  };

  useEffect(() => {
    loadProducts(page);
  }, [page]);

  useEffect(() => {
    const timer = setTimeout(() => {
      setPage(1);
      loadProducts(1);
    }, 300);

    return () => clearTimeout(timer);
  }, [searchField, searchValue]);

  const handleSearchValueChange = (event) => {
    const value = event.target.value;

    if (searchField === "id" || searchField === "stock") {
      setSearchValue(value.replace(/\D/g, ""));
      return;
    }

    if (searchField === "price") {
      const cleanPrice = value.replace(/[^0-9.]/g, "");
      if (cleanPrice.split(".").length > 2) return;
      setSearchValue(cleanPrice);
      return;
    }

    setSearchValue(
      value
        .replace(/[^a-zA-ZáéíóúÁÉÍÓÚñÑ0-9\s]/g, "")
        .toUpperCase()
        .slice(0, 80)
    );
  };

  const handleChange = (event) => {
    const { name, value } = event.target;

    if (name === "name") {
      setForm({
        ...form,
        name: value
          .replace(/[^a-zA-ZáéíóúÁÉÍÓÚñÑ0-9\s]/g, "")
          .toUpperCase()
          .slice(0, 80),
      });
      return;
    }

    if (name === "price") {
      const cleanPrice = value.replace(/[^0-9.]/g, "");
      if (cleanPrice.split(".").length > 2) return;
      setForm({ ...form, price: cleanPrice.slice(0, 10) });
      return;
    }

    if (name === "stock") {
      setForm({ ...form, stock: value.replace(/\D/g, "").slice(0, 6) });
      return;
    }

    setForm({ ...form, [name]: value });
  };

  const resetForm = () => {
    setForm(emptyProductForm);
    setEditingId(null);
  };

  const saveProduct = async () => {
    const request = {
      name: form.name.trim().toUpperCase(),
      price: Number(form.price),
      stock: Number(form.stock),
    };

    if (!request.name || form.price === "" || form.stock === "") {
      alert("Complete todos los campos.");
      return;
    }

    if (request.name.length < 2) {
      alert("El nombre del producto debe tener al menos 2 caracteres.");
      return;
    }

    if (Number.isNaN(request.price) || request.price <= 0) {
      alert("El precio debe ser mayor a cero.");
      return;
    }

    if (!Number.isInteger(request.stock) || request.stock < 0) {
      alert("El stock debe ser un número entero mayor o igual a cero.");
      return;
    }

    try {
      if (editingId) {
        await api.put(`/products/${editingId}`, request);
        alert("Producto actualizado correctamente.");
      } else {
        await api.post("/products", request);
        alert("Producto creado correctamente.");
      }

      resetForm();
      loadProducts(page);
    } catch (error) {
      console.error(error);
      alert("No se pudo guardar el producto.");
    }
  };

  const editProduct = (product) => {
    setEditingId(product.id);

    setForm({
      name: product.name,
      price: String(product.price),
      stock: String(product.stock),
    });
  };

  const deleteProduct = async (id) => {
    if (!confirm("¿Seguro que desea eliminar este producto?")) return;

    try {
      await api.delete(`/products/${id}`);
      alert("Producto eliminado correctamente.");
      loadProducts(page);
    } catch (error) {
      console.error(error);
      alert("No se pudo eliminar el producto.");
    }
  };

  useKeyboardShortcuts(
    [
      { ctrl: true, key: "b", action: () => searchInputRef.current?.focus() },
      { ctrl: true, key: "g", action: saveProduct },
      { ctrl: true, key: "l", action: resetForm },
      { alt: true, key: "g", action: () => saveButtonRef.current?.focus() },
      { alt: true, key: "l", action: () => cleanButtonRef.current?.focus() },
      { alt: true, key: "b", action: () => searchButtonRef.current?.focus() },
    ],
    [form, editingId, searchField, searchValue]
  );

  return (
    <Layout>
      <h1>Gestor Productos</h1>

      <div className="card">
        <h2>{editingId ? "Editar producto" : "Nuevo producto"}</h2>

        <div className="form-grid">
          <input name="name" placeholder="Nombre del producto" maxLength="80" value={form.name} onChange={handleChange} />
          <input name="price" placeholder="Precio" inputMode="decimal" value={form.price} onChange={handleChange} />
          <input name="stock" placeholder="Stock" inputMode="numeric" maxLength="6" value={form.stock} onChange={handleChange} />
        </div>

        <div className="actions-row">
          <button ref={saveButtonRef} onClick={saveProduct}>
            {editingId ? "💾 Actualizar" : "💾 Guardar"}
          </button>

          <button ref={cleanButtonRef} className="secondary-button" onClick={resetForm}>
            🧹 Limpiar
          </button>
        </div>
      </div>

      <div className="card">
        <h2>Buscar productos</h2>

        <div className="search-grid">
          <select
            value={searchField}
            onChange={(event) => {
              setSearchField(event.target.value);
              setSearchValue("");
            }}
          >
            {productSearchFields.map((field) => (
              <option key={field.key} value={field.key}>
                Buscar por {field.label}
              </option>
            ))}
          </select>

          <input
            ref={searchInputRef}
            placeholder="Ingrese valor de búsqueda"
            type={searchField === "id" || searchField === "stock" || searchField === "price" ? "number" : "text"}
            value={searchValue}
            onChange={handleSearchValueChange}
          />

          <button ref={searchButtonRef} onClick={() => loadProducts(1)}>
            🔍 Buscar
          </button>
        </div>
      </div>

      <div className="card">
        <h2>Listado de productos</h2>

        <DataTable
          columns={productColumns}
          data={products}
          emptyMessage="Sin productos registrados"
          actions={(product) => (
            <>
              <button className="table-action edit-button" onClick={() => editProduct(product)}>
                ✏️ Editar
              </button>

              <button className="table-action delete-button" onClick={() => deleteProduct(product.id)}>
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