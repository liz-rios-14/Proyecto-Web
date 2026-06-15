import { useEffect, useRef, useState } from "react";
import Layout from "../components/Layout";
import DataTable from "../components/DataTable";
import Pagination from "../components/Pagination";
import useKeyboardShortcuts from "../hooks/useKeyboardShortcuts";
import { api } from "../api/apiClient";
import { getApiErrorMessage } from "../api/apiError";
import { useAppAlert } from "../components/AppAlert";
import { useUnsavedChanges } from "../components/UnsavedChangesContext";
import {
  readStoredState,
  sanitizeDigits,
  sanitizeMoney,
  sanitizeSingleSpacedText,
  writeStoredState,
} from "../utils/inputSanitizers";

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
  { key: "stockAlert", label: "Aviso" },
  { key: "statusLabel", label: "Estado" },
];

const productSearchFields = [
  { key: "id", label: "Id", type: "number" },
  { key: "name", label: "Nombre" },
  { key: "price", label: "Precio", type: "number" },
  { key: "stock", label: "Stock", type: "number" },
];

const searchStateKey = "salespoint-product-search";
const initialSearchState = readStoredState(searchStateKey, {
  field: "name",
  value: "",
});

export default function ProductManagerPage() {
  const { showAlert, showConfirm } = useAppAlert();
  const [products, setProducts] = useState([]);
  const [form, setForm] = useState(emptyProductForm);
  const [formBaseline, setFormBaseline] = useState(emptyProductForm);
  const [editingId, setEditingId] = useState(null);
  const [searchField, setSearchField] = useState(initialSearchState.field);
  const [searchValue, setSearchValue] = useState(initialSearchState.value);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(1);
  const isDirty = JSON.stringify(form) !== JSON.stringify(formBaseline);
  useUnsavedChanges(isDirty);

  const saveButtonRef = useRef(null);
  const cleanButtonRef = useRef(null);
  const searchButtonRef = useRef(null);
  const searchInputRef = useRef(null);

  const loadProducts = async (currentPage = page) => {
    try {
      const params = {
        pageNumber: currentPage,
        pageSize,
      };

      if (searchField && searchValue.trim()) {
        params.field = searchField;
        params.value = searchValue.trim().replace(/\s+/g, " ");
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
  }, [page, pageSize]);

  useEffect(() => {
    writeStoredState(searchStateKey, {
      field: searchField,
      value: searchValue,
    });
  }, [searchField, searchValue]);

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
      setSearchValue(sanitizeDigits(value, searchField === "id" ? 9 : 6));
      return;
    }

    if (searchField === "price") {
      const cleanPrice = sanitizeMoney(value, 10);
      if (cleanPrice === null) return;
      setSearchValue(cleanPrice);
      return;
    }

    setSearchValue(sanitizeSingleSpacedText(value, 80));
  };

  const handleChange = (event) => {
    const { name, value } = event.target;

    if (name === "name") {
      setForm({
        ...form,
        name: sanitizeSingleSpacedText(value, 80),
      });
      return;
    }

    if (name === "price") {
      const cleanPrice = sanitizeMoney(value, 10);
      if (cleanPrice === null) return;
      setForm({ ...form, price: cleanPrice.slice(0, 10) });
      return;
    }

    if (name === "stock") {
      setForm({ ...form, stock: sanitizeDigits(value, 6) });
      return;
    }

    setForm({ ...form, [name]: value });
  };

  const clearForm = () => {
    setForm(emptyProductForm);
    setFormBaseline(emptyProductForm);
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

    if (!/^[\p{L}0-9 .,\-/]+$/u.test(request.name)) {
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
        "El stock debe ser un numero entero mayor o igual a cero.",
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
        const confirmed = await showConfirm(
          `¿Confirma actualizar el producto ${request.name}?`,
          { title: "Actualizar producto", confirmText: "Sí, actualizar" }
        );
        if (!confirmed) return;
        await api.put(`/products/${editingId}`, request);
        showAlert("Producto actualizado correctamente.", "success");
      } else {
        await api.post("/products", request);
        showAlert("Producto creado correctamente.", "success");
      }

      clearForm();
      loadProducts(page);
    } catch (error) {
      console.error(error);
      showAlert(
        getApiErrorMessage(error, "No se pudo guardar el producto."),
        "error"
      );
    }
  };

  const editProduct = async (product) => {
    if (isDirty) {
      const confirmed = await showConfirm(
        "Hay cambios sin guardar. ¿Desea descartarlos y editar otro producto?",
        { title: "Cambiar de producto", confirmText: "Sí, descartar" }
      );
      if (!confirmed) return;
    }

    const nextForm = {
      name: product.name,
      price: String(product.price),
      stock: String(product.stock),
    };
    setEditingId(product.id);
    setForm(nextForm);
    setFormBaseline(nextForm);
  };

  const deleteProduct = async (id) => {
    const confirmed = await showConfirm(
      "Seguro que desea eliminar fisicamente este producto?",
      {
        title: "Eliminar producto",
        confirmText: "Si, eliminar",
      }
    );

    if (!confirmed) return;

    const finalConfirmation = await showConfirm(
      "La eliminacion fisica solo se permite si el producto no tiene historial. Si tiene ventas o inventario, use Desactivar.",
      {
        title: "Confirmacion final",
        confirmText: "Eliminar fisicamente",
      }
    );

    if (!finalConfirmation) return;

    try {
      const response = await api.delete(`/products/${id}`);
      showAlert(response.data?.message ?? "Producto eliminado correctamente.", "success");

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

  const deactivateProduct = async (id) => {
    const confirmed = await showConfirm(
      "El producto quedara inactivo, no saldra para vender, pero seguira visible en la tabla. Desea continuar?",
      {
        title: "Desactivar producto",
        confirmText: "Desactivar",
      }
    );

    if (!confirmed) return;

    try {
      const response = await api.post(`/products/${id}/deactivate`);
      showAlert(response.data?.message ?? "Producto desactivado correctamente.", "success");
      loadProducts(page);
    } catch (error) {
      console.error(error);
      showAlert(
        getApiErrorMessage(error, "No se pudo desactivar el producto."),
        "error"
      );
    }
  };

  const activateProduct = async (id) => {
    const confirmed = await showConfirm(
      "¿Desea activar nuevamente este producto para permitir su venta?",
      { title: "Activar producto", confirmText: "Sí, activar" }
    );
    if (!confirmed) return;

    try {
      const response = await api.post(`/products/${id}/activate`);
      showAlert(response.data?.message ?? "Producto activado correctamente.", "success");
      loadProducts(page);
    } catch (error) {
      console.error(error);
      showAlert(
        getApiErrorMessage(error, "No se pudo activar el producto."),
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
            {editingId ? "Actualizar" : "Guardar"}
          </button>

          <button ref={cleanButtonRef} className="secondary-button" onClick={resetForm}>
            Limpiar
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
            placeholder="Ingrese valor de busqueda"
            type="text"
            value={searchValue}
            onChange={handleSearchValueChange}
          />

          <button ref={searchButtonRef} onClick={() => loadProducts(1)}>
            Buscar
          </button>
        </div>
      </div>

      <div className="card">
        <h2>Listado de productos</h2>

        <DataTable
          columns={productColumns}
          data={products.map((product) => ({
            ...product,
            stockAlert: Number(product.stock) === 1 ? "Stock bajo: 1" : "-",
            statusLabel: product.isActive ? "ACTIVO" : "INACTIVO",
          }))}
          emptyMessage="Sin productos registrados"
          actions={(product) => (
            <>
              <button className="table-action edit-button" onClick={() => editProduct(product)}>
                Editar
              </button>

              {product.isActive ? (
                <button className="table-action secondary-button" onClick={() => deactivateProduct(product.id)}>
                  Desactivar
                </button>
              ) : (
                <button className="table-action invoice-button" onClick={() => activateProduct(product.id)}>
                  Activar
                </button>
              )}

              <button className="table-action delete-button" onClick={() => deleteProduct(product.id)}>
                Eliminar
              </button>
            </>
          )}
        />

        <div className="actions-row">
          <label className="page-size-control">
            Registros por pagina
            <select
              value={pageSize}
              onChange={(event) => {
                setPageSize(Number(event.target.value));
                setPage(1);
              }}
            >
              {[10, 15, 20, 30].map((size) => (
                <option key={size} value={size}>
                  {size}
                </option>
              ))}
            </select>
          </label>
        </div>

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
