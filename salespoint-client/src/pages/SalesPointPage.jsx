import Layout from "../components/Layout";
import { useEffect, useMemo, useRef, useState } from "react";
import { useLocation } from "react-router-dom";
import SearchModal from "../components/SearchModal";
import InvoicePreviewModal from "../components/InvoicePreviewModal";
import useKeyboardShortcuts from "../hooks/useKeyboardShortcuts";
import { api } from "../api/apiClient";
import { getApiErrorMessage } from "../api/apiError";
import { useAppAlert } from "../components/AppAlert";
import { getAuthUser } from "../services/authStorage";

const formatDateTime = (date) => {
  const pad = (value) => String(value).padStart(2, "0");

  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(
    date.getDate()
  )} ${pad(date.getHours())}:${pad(date.getMinutes())}:${pad(
    date.getSeconds()
  )}`;
};

const toNumber = (value, defaultValue = 0) => {
  const number = Number(value);
  return Number.isFinite(number) ? number : defaultValue;
};

export default function SalesPointPage() {
  const location = useLocation();
  const { showAlert } = useAppAlert();
  const authUser = getAuthUser();

  const [invoiceDate, setInvoiceDate] = useState(formatDateTime(new Date()));
  const [customer, setCustomer] = useState(null);
  const [seller, setSeller] = useState({
    userName: authUser?.userName || "admin",
    fullName: authUser?.userName || "ADMINISTRADOR DEL SISTEMA",
    role: authUser?.roleName || "ADMINISTRATOR",
  });

  const [product, setProduct] = useState(null);
  const [quantity, setQuantity] = useState("");
  const [details, setDetails] = useState([]);
  const [showCustomerModal, setShowCustomerModal] = useState(false);
  const [showProductModal, setShowProductModal] = useState(false);
  const [showInvoicePreview, setShowInvoicePreview] = useState(false);
  const [createdInvoice, setCreatedInvoice] = useState(null);

  const [auditSource, setAuditSource] = useState("");
  const [auditOriginal, setAuditOriginal] = useState(null);

  const customerButtonRef = useRef(null);
  const productButtonRef = useRef(null);
  const addButtonRef = useRef(null);
  const cleanButtonRef = useRef(null);
  const previewButtonRef = useRef(null);
  const quantityInputRef = useRef(null);

  const isAuditReconstruction = Boolean(auditSource);

  useEffect(() => {
    if (isAuditReconstruction) return;

    const timer = setInterval(() => {
      setInvoiceDate(formatDateTime(new Date()));
    }, 1000);

    return () => clearInterval(timer);
  }, [isAuditReconstruction]);

  useEffect(() => {
    const reconstructedInvoice = location.state?.reconstructedInvoice;

    if (!reconstructedInvoice) return;

    const reconstructedCustomer = {
      id:
        reconstructedInvoice.customer?.id ||
        reconstructedInvoice.customer?.customerId ||
        reconstructedInvoice.customerId ||
        0,
      firstName:
        reconstructedInvoice.customer?.firstName ||
        reconstructedInvoice.customer?.name ||
        reconstructedInvoice.customerName ||
        "",
      lastName:
        reconstructedInvoice.customer?.lastName ||
        reconstructedInvoice.customerLastName ||
        "",
      phone:
        reconstructedInvoice.customer?.phone ||
        reconstructedInvoice.customerPhone ||
        "",
      address:
        reconstructedInvoice.customer?.address ||
        reconstructedInvoice.customerAddress ||
        "Dato reconstruido por auditoría",
      email:
        reconstructedInvoice.customer?.email ||
        reconstructedInvoice.customerEmail ||
        "",
    };

    const reconstructedSeller = {
      userName:
        reconstructedInvoice.seller?.userName ||
        reconstructedInvoice.seller?.username ||
        reconstructedInvoice.sellerUserName ||
        "admin",
      fullName:
        reconstructedInvoice.seller?.fullName ||
        reconstructedInvoice.seller?.name ||
        reconstructedInvoice.sellerName ||
        "ADMINISTRADOR DEL SISTEMA",
      role:
        reconstructedInvoice.seller?.role ||
        reconstructedInvoice.sellerRole ||
        "ADMINISTRATOR",
    };

    const reconstructedDetails = (reconstructedInvoice.details ?? []).map(
      (item) => {
        const price = toNumber(item.price ?? item.unitPrice ?? item.appliedPrice);
        const quantityValue = toNumber(item.quantity, 1);
        const subtotalValue =
          toNumber(item.subtotal) || Number((price * quantityValue).toFixed(2));

        return {
          id: item.id || item.productId,
          name: item.name || item.productName || item.description || "",
          price,
          stock: toNumber(item.stock, quantityValue),
          quantity: quantityValue,
          subtotal: subtotalValue,
        };
      }
    );

    const reconstructedSubtotal =
      toNumber(reconstructedInvoice.subtotal) ||
      reconstructedDetails.reduce((sum, item) => sum + toNumber(item.subtotal), 0);

    const reconstructedIva =
      toNumber(reconstructedInvoice.tax ?? reconstructedInvoice.iva) ||
      Number((reconstructedSubtotal * 0.12).toFixed(2));

    const reconstructedTotal =
      toNumber(reconstructedInvoice.total) ||
      Number((reconstructedSubtotal + reconstructedIva).toFixed(2));

    setInvoiceDate(
      reconstructedInvoice.invoiceDate ||
        reconstructedInvoice.date ||
        reconstructedInvoice.createdAt ||
        formatDateTime(new Date())
    );

    setCustomer(reconstructedCustomer);
    setSeller(reconstructedSeller);
    setDetails(reconstructedDetails);
    setCreatedInvoice(null);
    setProduct(null);
    setQuantity("");
    setAuditSource(reconstructedInvoice.invoiceNumber || "");

    setAuditOriginal({
      invoiceNumber: reconstructedInvoice.invoiceNumber || "",
      invoiceDate:
        reconstructedInvoice.invoiceDate ||
        reconstructedInvoice.date ||
        reconstructedInvoice.createdAt ||
        "",
      customer: reconstructedCustomer,
      seller: reconstructedSeller,
      details: reconstructedDetails,
      subtotal: reconstructedSubtotal,
      iva: reconstructedIva,
      total: reconstructedTotal,
    });

    setShowInvoicePreview(false);
  }, [location.state]);

  const subtotal = useMemo(
    () => details.reduce((sum, item) => sum + toNumber(item.subtotal), 0),
    [details]
  );

  const iva = Number((subtotal * 0.12).toFixed(2));
  const total = Number((subtotal + iva).toFixed(2));

  const updateCustomerField = (field, value) => {
    let cleanValue = value;

    if (field === "firstName" || field === "lastName") {
      cleanValue = value
        .replace(/[^a-zA-ZáéíóúÁÉÍÓÚñÑ\s]/g, "")
        .toUpperCase()
        .slice(0, 40);
    }

    if (field === "phone") {
      cleanValue = value.replace(/\D/g, "").slice(0, 10);
    }

    if (field === "address") {
      cleanValue = value.toUpperCase().slice(0, 150);
    }

    if (field === "email") {
      cleanValue = value.toLowerCase().slice(0, 120);
    }

    setCustomer((currentCustomer) => ({
      id: currentCustomer?.id || 0,
      firstName: currentCustomer?.firstName || "",
      lastName: currentCustomer?.lastName || "",
      phone: currentCustomer?.phone || "",
      address: currentCustomer?.address || "",
      email: currentCustomer?.email || "",
      ...currentCustomer,
      [field]: cleanValue,
    }));

    setCreatedInvoice(null);
  };

  const getCustomerValidationMessage = () => {
    if (!customer?.id || Number(customer.id) <= 0) {
      return "Seleccione un cliente válido.";
    }

    const firstName = customer.firstName?.trim() ?? "";
    const lastName = customer.lastName?.trim() ?? "";
    const phone = customer.phone?.trim() ?? "";
    const address = customer.address?.trim() ?? "";
    const email = customer.email?.trim() ?? "";

    if (!firstName || !lastName || !phone || !address || !email) {
      return "Complete todos los datos del cliente antes de facturar.";
    }

    if (firstName.length < 2 || lastName.length < 2) {
      return "El nombre y apellido del cliente deben tener al menos 2 caracteres.";
    }

    if (!/^\d{10}$/.test(phone)) {
      return "El teléfono del cliente debe tener exactamente 10 dígitos.";
    }

    if (address.length < 5) {
      return "La dirección del cliente debe tener al menos 5 caracteres.";
    }

    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      return "El correo del cliente no tiene un formato válido.";
    }

    return "";
  };

  const auditComparison = useMemo(() => {
    if (!auditOriginal) return [];

    const changes = [];

    const originalCustomerName = `${auditOriginal.customer.firstName} ${auditOriginal.customer.lastName}`.trim();
    const currentCustomerName = `${customer?.firstName ?? ""} ${
      customer?.lastName ?? ""
    }`.trim();

    if (originalCustomerName !== currentCustomerName) {
      changes.push({
        label: "Cliente",
        original: originalCustomerName || "Sin dato",
        current: currentCustomerName || "Sin dato",
      });
    }

    if ((auditOriginal.customer.phone || "") !== (customer?.phone || "")) {
      changes.push({
        label: "Teléfono",
        original: auditOriginal.customer.phone || "Sin dato",
        current: customer?.phone || "Sin dato",
      });
    }

    if ((auditOriginal.customer.address || "") !== (customer?.address || "")) {
      changes.push({
        label: "Dirección",
        original: auditOriginal.customer.address || "Sin dato",
        current: customer?.address || "Sin dato",
      });
    }

    if ((auditOriginal.customer.email || "") !== (customer?.email || "")) {
      changes.push({
        label: "Correo",
        original: auditOriginal.customer.email || "Sin dato",
        current: customer?.email || "Sin dato",
      });
    }

    auditOriginal.details.forEach((originalItem) => {
      const currentItem = details.find((item) => item.id === originalItem.id);

      if (!currentItem) {
        changes.push({
          label: `Producto ${originalItem.name}`,
          original: "Existía",
          current: "Eliminado",
        });
        return;
      }

      if (toNumber(originalItem.quantity) !== toNumber(currentItem.quantity)) {
        changes.push({
          label: `Cantidad ${originalItem.name}`,
          original: String(originalItem.quantity),
          current: String(currentItem.quantity),
        });
      }

      if (toNumber(originalItem.price) !== toNumber(currentItem.price)) {
        changes.push({
          label: `Precio ${originalItem.name}`,
          original: `$${toNumber(originalItem.price).toFixed(2)}`,
          current: `$${toNumber(currentItem.price).toFixed(2)}`,
        });
      }

      if (toNumber(originalItem.subtotal).toFixed(2) !== toNumber(currentItem.subtotal).toFixed(2)) {
        changes.push({
          label: `Subtotal ${originalItem.name}`,
          original: `$${toNumber(originalItem.subtotal).toFixed(2)}`,
          current: `$${toNumber(currentItem.subtotal).toFixed(2)}`,
        });
      }
    });

    details.forEach((currentItem) => {
      const exists = auditOriginal.details.some(
        (item) => item.id === currentItem.id
      );

      if (!exists) {
        changes.push({
          label: "Producto agregado",
          original: "No existía",
          current: `${currentItem.name} x ${currentItem.quantity}`,
        });
      }
    });

    if (toNumber(auditOriginal.subtotal).toFixed(2) !== subtotal.toFixed(2)) {
      changes.push({
        label: "Subtotal",
        original: `$${toNumber(auditOriginal.subtotal).toFixed(2)}`,
        current: `$${subtotal.toFixed(2)}`,
      });
    }

    if (toNumber(auditOriginal.iva).toFixed(2) !== iva.toFixed(2)) {
      changes.push({
        label: "IVA",
        original: `$${toNumber(auditOriginal.iva).toFixed(2)}`,
        current: `$${iva.toFixed(2)}`,
      });
    }

    if (toNumber(auditOriginal.total).toFixed(2) !== total.toFixed(2)) {
      changes.push({
        label: "Total",
        original: `$${toNumber(auditOriginal.total).toFixed(2)}`,
        current: `$${total.toFixed(2)}`,
      });
    }

    return changes;
  }, [auditOriginal, customer, details, subtotal, iva, total]);

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
      showAlert("Seleccione un producto.", "warning");
      return;
    }

    if (!quantity || Number(quantity) <= 0) {
      showAlert("Ingrese una cantidad válida.", "warning");
      return;
    }

    const requestedQuantity = Number(quantity);
    const existingDetail = details.find((item) => item.id === product.id);
    const currentQuantity = existingDetail ? Number(existingDetail.quantity) : 0;
    const finalQuantity = currentQuantity + requestedQuantity;

    if (finalQuantity > Number(product.stock)) {
      showAlert(
        "La cantidad no puede superar el stock disponible.",
        "warning"
      );
      return;
    }

    if (existingDetail) {
      setDetails(
        details.map((item) =>
          item.id === product.id
            ? {
                ...item,
                quantity: finalQuantity,
                subtotal: Number((Number(item.price) * finalQuantity).toFixed(2)),
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
          subtotal: Number((Number(product.price) * requestedQuantity).toFixed(2)),
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
          showAlert("La cantidad debe ser mayor a cero.", "warning");
          return item;
        }

        if (newQuantity > Number(item.stock)) {
          showAlert("No puede superar el stock disponible.", "warning");
          return item;
        }

        return {
          ...item,
          quantity: newQuantity,
          subtotal: Number((Number(item.price) * newQuantity).toFixed(2)),
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
    setSeller({
      userName: authUser?.userName || "admin",
      fullName: authUser?.userName || "ADMINISTRADOR DEL SISTEMA",
      role: authUser?.roleName || "ADMINISTRATOR",
    });
    setProduct(null);
    setQuantity("");
    setDetails([]);
    setCreatedInvoice(null);
    setShowInvoicePreview(false);
    setAuditSource("");
    setAuditOriginal(null);
    setInvoiceDate(formatDateTime(new Date()));
  };

  const closeAllModals = () => {
    setShowCustomerModal(false);
    setShowProductModal(false);
    setShowInvoicePreview(false);
  };

  const openPreview = () => {
    const customerValidationMessage = getCustomerValidationMessage();

    if (customerValidationMessage) {
      showAlert(customerValidationMessage, "warning");
      return;
    }

    if (details.length === 0) {
      showAlert("Agregue al menos un producto.", "warning");
      return;
    }

    const hasInvalidQuantity = details.some(
      (item) => !item.quantity || Number(item.quantity) <= 0
    );

    if (hasInvalidQuantity) {
      showAlert("Revise las cantidades del detalle.", "warning");
      return;
    }

    const hasInsufficientStock = details.some(
      (item) => Number(item.quantity) > Number(item.stock)
    );

    if (hasInsufficientStock) {
      showAlert(
        "Una o más cantidades superan el stock disponible.",
        "warning"
      );
      return;
    }

    setShowInvoicePreview(true);
  };

  const finishInvoice = async () => {
    if (createdInvoice?.invoiceNumber) return;

    const customerValidationMessage = getCustomerValidationMessage();

    if (customerValidationMessage) {
      showAlert(customerValidationMessage, "warning");
      return;
    }

    if (details.length === 0) {
      showAlert("Agregue al menos un producto.", "warning");
      return;
    }

    const invalidDetail = details.find(
      (item) =>
        !Number.isInteger(Number(item.quantity)) ||
        Number(item.quantity) <= 0 ||
        Number(item.quantity) > Number(item.stock)
    );

    if (invalidDetail) {
      showAlert(
        `Revise la cantidad de ${invalidDetail.name}. Debe ser mayor a cero y no superar el stock disponible.`,
        "warning"
      );
      return;
    }

    const request = {
      customerId: Number(customer.id),
      customerName: `${customer?.firstName ?? ""} ${customer?.lastName ?? ""}`.trim(),
      customerEmail: customer?.email ?? "",
      customerPhone: customer?.phone ?? "",
      customerAddress: customer?.address ?? "",
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
          stock: Number(item.stock) - Number(item.quantity),
        }))
      );

      showAlert(
        `Factura ${response.data.invoiceNumber} generada correctamente.`,
        "success"
      );
    } catch (error) {
      console.error(error);
      showAlert(
        getApiErrorMessage(error, "No se pudo registrar la factura."),
        "error"
      );
    }
  };

  useKeyboardShortcuts(
    [
      { ctrl: true, key: "c", action: () => setShowCustomerModal(true) },
      { ctrl: true, key: "b", action: () => setShowProductModal(true) },
      { ctrl: true, key: "g", action: openPreview },
      { ctrl: true, key: "l", action: cleanInvoice },
      { alt: true, key: "c", action: () => customerButtonRef.current?.focus() },
      { alt: true, key: "b", action: () => productButtonRef.current?.focus() },
      { alt: true, key: "a", action: () => addButtonRef.current?.focus() },
      { alt: true, key: "q", action: () => quantityInputRef.current?.focus() },
      { alt: true, key: "g", action: () => previewButtonRef.current?.focus() },
      { alt: true, key: "l", action: () => cleanButtonRef.current?.focus() },
      { key: "escape", action: closeAllModals },
    ],
    [customer, product, quantity, details]
  );

  return (
    <Layout>
      <h1>Crear Factura</h1>

      {auditSource && (
        <div className="card">
          <h2>Venta reconstruida por auditoría</h2>
          <p>
            Esta venta fue cargada desde la factura{" "}
            <strong>{auditSource}</strong>. Puede editar los campos antes de
            generar la nueva factura. La venta original no será modificada.
          </p>
        </div>
      )}

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
          <input
            value={customer?.id || ""}
            placeholder="Id"
            readOnly
          />

          <input
            value={customer?.firstName || ""}
            placeholder="Nombre"
            onChange={(event) =>
              updateCustomerField("firstName", event.target.value)
            }
          />

          <input
            value={customer?.lastName || ""}
            placeholder="Apellido"
            onChange={(event) =>
              updateCustomerField("lastName", event.target.value)
            }
          />

          <input
            value={customer?.phone || ""}
            placeholder="Teléfono"
            onChange={(event) => updateCustomerField("phone", event.target.value)}
          />

          <input
            value={customer?.address || ""}
            placeholder="Dirección"
            onChange={(event) =>
              updateCustomerField("address", event.target.value)
            }
          />

          <input
            value={customer?.email || ""}
            placeholder="Correo"
            onChange={(event) => updateCustomerField("email", event.target.value)}
          />
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
        <h2>Vendedor</h2>

        <div className="form-grid">
          <input
            value={seller?.userName || ""}
            placeholder="Usuario vendedor"
            readOnly
          />

          <input
            value={seller?.fullName || ""}
            placeholder="Nombre vendedor"
            readOnly
          />

          <input
            value={seller?.role || ""}
            placeholder="Rol"
            readOnly
          />
        </div>
      </div>

      <div className="card">
        <h2>Producto</h2>

        <div className="product-grid">
          <input
            value={product?.name || ""}
            placeholder="Nombre producto"
            readOnly
          />

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
              <th className="number-cell">Precio aplicado</th>
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
        seller={seller}
        details={details}
        subtotal={subtotal}
        iva={iva}
        total={total}
        auditComparison={auditComparison}
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
