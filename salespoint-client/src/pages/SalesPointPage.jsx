import Layout from "../components/Layout";
import { useEffect, useMemo, useRef, useState } from "react";
import SearchModal from "../components/SearchModal";
import InvoicePreviewModal from "../components/InvoicePreviewModal";
import useKeyboardShortcuts from "../hooks/useKeyboardShortcuts";
import { api } from "../api/apiClient";

const formatDateTime = (date) => {
  const pad = (value) => String(value).padStart(2, "0");

  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(
    date.getDate()
  )} ${pad(date.getHours())}:${pad(date.getMinutes())}:${pad(
    date.getSeconds()
  )}`;
};

export default function SalesPointPage() {
  const [invoiceDate, setInvoiceDate] = useState(formatDateTime(new Date()));
  const [customer, setCustomer] = useState(null);
  const [product, setProduct] = useState(null);
  const [quantity, setQuantity] = useState("");
  const [details, setDetails] = useState([]);
  const [showCustomerModal, setShowCustomerModal] = useState(false);
  const [showProductModal, setShowProductModal] = useState(false);
  const [showInvoicePreview, setShowInvoicePreview] = useState(false);
  const [createdInvoice, setCreatedInvoice] = useState(null);

  const customerButtonRef = useRef(null);
  const productButtonRef = useRef(null);
  const addButtonRef = useRef(null);
  const cleanButtonRef = useRef(null);
  const previewButtonRef = useRef(null);
  const quantityInputRef = useRef(null);

  useEffect(() => {
    const timer = setInterval(() => {
      setInvoiceDate(formatDateTime(new Date()));
    }, 1000);

    return () => clearInterval(timer);
  }, []);

  const subtotal = useMemo(
    () => details.reduce((sum, item) => sum + item.subtotal, 0),
    [details]
  );

  const iva = Number((subtotal * 0.12).toFixed(2));
  const total = Number((subtotal + iva).toFixed(2));

  const handleQuantityChange = (event) => {
    const value = event.target.value;

    if (value === "") {
      setQuantity("");
      return;
    }

    if (Number(value) > 0) {
      setQuantity(value);
    }
  };

  const fetchCustomers = async (field, value, page) => {
    const params = {
      pageNumber: page,
      pageSize: 5,
    };

    if (field && value) {
      params.field = field;
      params.value = value.trim();
    }
    const response = await api.get("/customers/search", { params });

    return response.data;
  };

  const fetchProducts = async (field, value, page) => {
    const params = {
      pageNumber: page,
      pageSize: 5,
    };

    if (field && value) {
      params.field = field;
      params.value = value.trim();
    }
    const response = await api.get("/products/search", { params });

    return response.data;
  };

  const addProductToDetail = () => {
    if (!product) {
      alert("Seleccione un producto.");
      return;
    }

    if (!quantity || Number(quantity) <= 0) {
      alert("Ingrese una cantidad válida.");
      return;
    }

    const requestedQuantity = Number(quantity);
    const existingDetail = details.find((item) => item.id === product.id);
    const currentQuantity = existingDetail ? Number(existingDetail.quantity) : 0;
    const finalQuantity = currentQuantity + requestedQuantity;

    if (finalQuantity > product.stock) {
      alert("La cantidad no puede superar el stock disponible.");
      return;
    }

    if (existingDetail) {
      setDetails(
        details.map((item) =>
          item.id === product.id
            ? {
              ...item,
              quantity: finalQuantity,
              subtotal: item.price * finalQuantity,
            }
            : item
        )
      );
    } else {
      setDetails([
        ...details,
        {
          id: product.id,
          name: product.name,
          price: Number(product.price),
          stock: Number(product.stock),
          quantity: requestedQuantity,
          subtotal: Number(product.price) * requestedQuantity,
        },
      ]);
    }

    setProduct(null);
    setQuantity("");
    setCreatedInvoice(null);
  };

  const updateDetailQuantity = (productId, value) => {
    if (value === "") {
      setDetails((currentDetails) =>
        currentDetails.map((item) =>
          item.id === productId ? { ...item, quantity: "", subtotal: 0 } : item
        )
      );
      setCreatedInvoice(null);
      return;
    }

    const newQuantity = Number(value);

    setDetails((currentDetails) =>
      currentDetails.map((item) => {
        if (item.id !== productId) return item;

        if (newQuantity <= 0) {
          alert("La cantidad debe ser mayor a cero.");
          return item;
        }

        if (newQuantity > item.stock) {
          alert("No puede superar el stock disponible.");
          return item;
        }

        return {
          ...item,
          quantity: newQuantity,
          subtotal: item.price * newQuantity,
        };
      })
    );

    setCreatedInvoice(null);
  };

  const removeDetail = (indexToRemove) => {
    setDetails(details.filter((_, index) => index !== indexToRemove));
    setCreatedInvoice(null);
  };

  const cleanInvoice = () => {
    setCustomer(null);
    setProduct(null);
    setQuantity("");
    setDetails([]);
    setCreatedInvoice(null);
    setShowInvoicePreview(false);
  };

  const closeAllModals = () => {
    setShowCustomerModal(false);
    setShowProductModal(false);
    setShowInvoicePreview(false);
  };

  const openPreview = () => {
    if (!customer) {
      alert("Seleccione un cliente.");
      return;
    }

    if (details.length === 0) {
      alert("Agregue al menos un producto.");
      return;
    }

    const hasInvalidQuantity = details.some(
      (item) => !item.quantity || Number(item.quantity) <= 0
    );

    if (hasInvalidQuantity) {
      alert("Revise las cantidades del detalle.");
      return;
    }

    setShowInvoicePreview(true);
  };

  const finishInvoice = async () => {
    if (createdInvoice?.invoiceNumber) {
      return;
    }

    if (!customer) {
      alert("Seleccione un cliente.");
      return;
    }

    if (details.length === 0) {
      alert("Agregue al menos un producto.");
      return;
    }

    const request = {
      customerId: customer.id,
      details: details.map((item) => ({
        productId: item.id,
        quantity: Number(item.quantity),
      })),
    };

    try {
      const response = await api.post("/invoices", request);

      setCreatedInvoice(response.data);

      setDetails((currentDetails) =>
        currentDetails.map((item) => ({
          ...item,
          stock: item.stock - item.quantity,
        }))
      );
    } catch (error) {
      console.error(error);
      alert("No se pudo registrar la factura. Revise el stock disponible. Seleccione otro producto, por favor.");
    }
  };

  useKeyboardShortcuts(
    [
      {
        ctrl: true,
        key: "c",
        action: () => setShowCustomerModal(true),
      },
      {
        ctrl: true,
        key: "b",
        action: () => setShowProductModal(true),
      },
      {
        ctrl: true,
        key: "g",
        action: openPreview,
      },
      {
        ctrl: true,
        key: "l",
        action: cleanInvoice,
      },
      {
        alt: true,
        key: "c",
        action: () => customerButtonRef.current?.focus(),
      },
      {
        alt: true,
        key: "b",
        action: () => productButtonRef.current?.focus(),
      },
      {
        alt: true,
        key: "a",
        action: () => addButtonRef.current?.focus(),
      },
      {
        alt: true,
        key: "q",
        action: () => quantityInputRef.current?.focus(),
      },
      {
        alt: true,
        key: "g",
        action: () => previewButtonRef.current?.focus(),
      },
      {
        alt: true,
        key: "l",
        action: () => cleanButtonRef.current?.focus(),
      },
      {
        key: "escape",
        action: closeAllModals,
      },
    ],
    [customer, product, quantity, details]
  );

  return (
    <Layout>
      <h1>Crear Factura</h1>

      <div className="card invoice-header-card">
        <div className="invoice-header-grid">
          <div>
            <label>Factura</label>
            <input
              value={createdInvoice?.invoiceNumber || "Se generará al finalizar"}
              readOnly
            />
          </div>

          <div>
            <label>Fecha y hora</label>
            <input value={invoiceDate} readOnly />
          </div>
        </div>
      </div>

      <div className="card">
        <h2>Cliente</h2>

        <div className="form-grid">
          <input value={customer?.id || ""} placeholder="Id" readOnly />
          <input value={customer?.firstName || ""} placeholder="Nombre" readOnly />
          <input value={customer?.lastName || ""} placeholder="Apellido" readOnly />
          <input value={customer?.phone || ""} placeholder="Teléfono" readOnly />
          <input value={customer?.address || ""} placeholder="Dirección" readOnly />
          <input value={customer?.email || ""} placeholder="Correo" readOnly />
        </div>

        <button
          ref={customerButtonRef}
          style={{ marginTop: "10px" }}
          onClick={() => setShowCustomerModal(true)}
        >
          🔍 Buscar cliente
        </button>
      </div>

      <div className="card">
        <h2>Producto</h2>

        <div className="product-grid">
          <input value={product?.name || ""} placeholder="Nombre producto" readOnly />

          <input
            ref={quantityInputRef}
            placeholder="Cantidad"
            type="number"
            min="1"
            value={quantity}
            onChange={handleQuantityChange}
          />

          <button ref={productButtonRef} onClick={() => setShowProductModal(true)}>
            🔎 Buscar producto
          </button>
        </div>

        {product && (
          <div className="selected-product">
            <strong>Producto seleccionado:</strong> Id: {product.id} |{" "}
            {product.name} | Precio: ${Number(product.price).toFixed(2)} |
            Stock: {product.stock}
          </div>
        )}

        <button
          ref={addButtonRef}
          style={{ marginTop: "10px" }}
          onClick={addProductToDetail}
        >
          ➕ Agregar
        </button>
      </div>

      <div className="card">
        <h2>Detalle factura</h2>

        <table>
          <thead>
            <tr>
              <th className="number-cell">Id</th>
              <th>Producto</th>
              <th className="number-cell">Precio</th>
              <th className="number-cell">Cantidad</th>
              <th className="number-cell">Stock</th>
              <th className="number-cell">Subtotal</th>
              <th>Acciones</th>
            </tr>
          </thead>

          <tbody>
            {details.length === 0 ? (
              <tr>
                <td colSpan="7">Sin productos agregados</td>
              </tr>
            ) : (
              details.map((item, index) => (
                <tr key={`${item.id}-${index}`}>
                  <td className="number-cell">{item.id}</td>

                  <td>{item.name}</td>

                  <td className="number-cell">
                    ${Number(item.price).toFixed(2)}
                  </td>

                  <td className="number-cell">
                    <input
                      className="quantity-input"
                      type="number"
                      min="1"
                      max={item.stock}
                      value={item.quantity}
                      onChange={(event) =>
                        updateDetailQuantity(item.id, event.target.value)
                      }
                    />
                  </td>

                  <td className="number-cell">{item.stock}</td>

                  <td className="number-cell">
                    ${Number(item.subtotal).toFixed(2)}
                  </td>

                  <td>
                    <button
                      className="table-action delete-button"
                      onClick={() => removeDetail(index)}
                    >
                      🗑️ Eliminar
                    </button>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      <div className="card totals-card">
        <div className="totals-box">
          <p>
            <strong>Subtotal:</strong> ${subtotal.toFixed(2)}
          </p>
          <p>
            <strong>IVA 12%:</strong> ${iva.toFixed(2)}
          </p>
          <p>
            <strong>Total:</strong> ${total.toFixed(2)}
          </p>
        </div>

        <div className="actions-row">
          <button
            ref={cleanButtonRef}
            className="secondary-button"
            onClick={cleanInvoice}
          >
            🧹 Limpiar
          </button>

          <button
            ref={previewButtonRef}
            className="invoice-button"
            onClick={openPreview}
          >
            💾 Facturar
          </button>
        </div>
      </div>

      <SearchModal
        isOpen={showCustomerModal}
        title="Buscar cliente"
        firstPlaceholder="Nombre"
        secondPlaceholder="ID"
        secondInputType="number"
        onClose={() => setShowCustomerModal(false)}
        fetchData={fetchCustomers}
        onSelect={(item) => {
          setCustomer(item);
          setCreatedInvoice(null);
          setShowCustomerModal(false);
        }}
        columns={[
          { key: "id", label: "Id", type: "number" },
          { key: "firstName", label: "Nombre" },
          { key: "lastName", label: "Apellido" },
          { key: "phone", label: "Teléfono" },
          { key: "address", label: "Dirección" },
          { key: "email", label: "Correo" },
        ]}
        searchFields={[
          { key: "id", label: "Id", type: "number" },
          { key: "firstName", label: "Nombre" },
          { key: "lastName", label: "Apellido" },
          { key: "phone", label: "Teléfono" },
          { key: "address", label: "Dirección" },
          { key: "email", label: "Correo" },
        ]}
      />

      <SearchModal
        isOpen={showProductModal}
        title="Buscar producto"
        firstPlaceholder="Nombre"
        secondPlaceholder="Id"
        secondInputType="number"
        onClose={() => setShowProductModal(false)}
        fetchData={fetchProducts}
        onSelect={(item) => {
          setProduct(item);
          setShowProductModal(false);
        }}
        columns={[
          { key: "id", label: "Id", type: "number" },
          { key: "name", label: "Nombre" },
          { key: "price", label: "Precio", type: "money" },
          { key: "stock", label: "Stock", type: "number" },
        ]}
        searchFields={[
          { key: "id", label: "Id", type: "number" },
          { key: "name", label: "Nombre" },
          { key: "price", label: "Precio", type: "number" },
          { key: "stock", label: "Stock", type: "number" },
        ]}
      />

      <InvoicePreviewModal
        isOpen={showInvoicePreview}
        invoiceNumber={createdInvoice?.invoiceNumber}
        invoiceDate={invoiceDate}
        customer={customer}
        details={details}
        subtotal={subtotal}
        iva={iva}
        total={total}
        onBack={() => {
          if (createdInvoice?.invoiceNumber) {
            cleanInvoice();
            return;
          }

          setShowInvoicePreview(false);
        }}
        onCloseAndClean={finishInvoice}
      />
    </Layout>
  );
}