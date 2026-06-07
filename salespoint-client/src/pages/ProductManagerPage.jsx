import { useEffect, useRef, useState } from "react";
import Layout from "../components/Layout";
import DataTable from "../components/DataTable";
import Pagination from "../components/Pagination";
import useKeyboardShortcuts from "../hooks/useKeyboardShortcuts";
import { api } from "../api/apiClient";
import { getApiErrorMessage } from "../api/apiError";
import { useAppAlert } from "../components/AppAlert";

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
  const { showAlert, showConfirm } = useAppAlert();
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
    try {
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
      setTotalPages(Math.max(response.data.totalPages ?? 1, 1));
    } catch (error) {
      setProducts([]);
      setTotalPages(1);
      showAlert(
        getApiErrorMessage(error, "No se pudo cargar la lista de productos."),
        "error"
      );
    }
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
      if (!/^\d{0,6}(\.\d{0,2})?$/.test(cleanPrice)) return;
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
      name: form.name.trim().replace(/\s+/g, " ").toUpperCase(),
      price: Number(form.price),
      stock: Number(form.stock),
    };

    if (!request.name || form.price === "" || form.stock === "") {
      showAlert("Complete todos los campos.", "warning");
      return;
    }

    if (request.name.length < 2) {
      showAlert(
        "El nombre del producto debe tener al menos 2 caracteres.",
        "warning"
      );
      return;
    }

    if (request.name.length > 80) {
      showAlert(
        "El nombre del producto no puede superar los 80 caracteres.",
        "warning"
      );
      return;
    }

    if (!/^[A-ZÁÉÍÓÚÜÑ0-9 .,\-/]+$/.test(request.name)) {
      showAlert("El nombre del producto contiene caracteres no permitidos.", "warning");
      return;
    }

    if (Number.isNaN(request.price) || request.price <= 0) {
      showAlert("El precio debe ser mayor a cero.", "warning");
      return;
    }

    if (request.price > 999999.99) {
      showAlert("El precio ingresado es demasiado alto.", "warning");
      return;
    }

    if (!Number.isInteger(request.stock) || request.stock < 0) {
      showAlert(
        "El stock debe ser un número entero mayor o igual a cero.",
        "warning"
      );
      return;
    }

    if (request.stock > 999999) {
      showAlert("El stock ingresado es demasiado alto.", "warning");
      return;
    }

    try {
      const duplicateResponse = await api.get("/products/search", {
        params: {
          field: "name",
          value: request.name,
          pageNumber: 1,
          pageSize: 10,
        },
      });

      const duplicateProduct = (duplicateResponse.data.items ?? []).find(
        (product) =>
          product.name?.trim().toUpperCase() === request.name &&
          product.id !== editingId
      );

      if (duplicateProduct) {
        showAlert(
          editingId
            ? "Ya existe otro producto con el mismo nombre."
            : "Ya existe un producto con el mismo nombre.",
          "warning"
        );
        return;
      }

      if (editingId) {
        await api.put(`/products/${editingId}`, request);
        showAlert("Producto actualizado correctamente.", "success");
      } else {
        await api.post("/products", request);
        showAlert("Producto creado correctamente.", "success");
      }

      resetForm();
      loadProducts(page);
    } catch (error) {
      console.error(error);
      showAlert(
        getApiErrorMessage(error, "No se pudo guardar el producto."),
        "error"
      );
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
    const confirmed = await showConfirm(
      "¿Seguro que desea eliminar este producto?",
      {
        title: "Eliminar producto",
        confirmText: "Sí, continuar",
      }
    );

    if (!confirmed) return;

    const finalConfirmation = await showConfirm(
      "Si el producto tiene historial se desactivará para conservar la auditoría. Si no tiene historial se eliminará físicamente. ¿Desea continuar?",
      {
        title: "Confirmación final",
        confirmText: "Confirmar",
      }
    );

    if (!finalConfirmation) return;

    try {
      const response = await api.delete(`/products/${id}`);
      showAlert(response.data?.message ?? "Producto procesado correctamente.", "success");

      if (products.length === 1 && page > 1) {
        setPage(page - 1);
      } else {
        loadProducts(page);
      }
    } catch (error) {
      console.error(error);
      showAlert(
        getApiErrorMessage(error, "No se pudo eliminar el producto."),
        "error"
      );
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
                🗑️ Eliminar / desactivar
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
