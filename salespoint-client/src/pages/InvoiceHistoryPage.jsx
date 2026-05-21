import { useEffect, useMemo, useRef, useState } from "react";
import Layout from "../components/Layout";
import DataTable from "../components/DataTable";
import InvoicePreviewModal from "../components/InvoicePreviewModal";
import useKeyboardShortcuts from "../hooks/useKeyboardShortcuts";
import { getInvoices, getInvoiceById } from "../api/invoiceApi";

const invoiceColumns = [
  { key: "invoiceNumber", label: "Factura" },
  { key: "customerName", label: "Cliente" },
  { key: "formattedDate", label: "Fecha" },
  { key: "total", label: "Total", type: "money" },
];

const searchFields = [
  { key: "invoiceNumber", label: "Factura" },
  { key: "customerName", label: "Cliente" },
  { key: "formattedDate", label: "Fecha" },
  { key: "total", label: "Total" },
];

const formatDateTime = (date) => {
  const currentDate = new Date(date);
  const pad = (value) => String(value).padStart(2, "0");

  return `${currentDate.getFullYear()}-${pad(
    currentDate.getMonth() + 1
  )}-${pad(currentDate.getDate())} ${pad(
    currentDate.getHours()
  )}:${pad(currentDate.getMinutes())}:${pad(currentDate.getSeconds())}`;
};

export default function InvoiceHistoryPage() {
  const [invoices, setInvoices] = useState([]);
  const [selectedInvoice, setSelectedInvoice] = useState(null);
  const [showPreview, setShowPreview] = useState(false);
  const [selectedIndex, setSelectedIndex] = useState(0);
  const [searchField, setSearchField] = useState("invoiceNumber");
  const [searchValue, setSearchValue] = useState("");

  const firstViewButtonRef = useRef(null);
  const searchInputRef = useRef(null);

  const loadInvoices = async () => {
    try {
      const data = await getInvoices();

      const formattedInvoices = data.map((invoice) => ({
        ...invoice,
        formattedDate: formatDateTime(invoice.date),
      }));

      setInvoices(formattedInvoices);
      setSelectedIndex(0);
    } catch (error) {
      console.error("Error cargando facturas:", error);
      alert("No se pudo cargar el historial de facturas.");
    }
  };

  const filteredInvoices = useMemo(() => {
    if (!searchValue.trim()) return invoices;

    const value = searchValue.trim().toUpperCase();

    return invoices.filter((invoice) => {
      const fieldValue = invoice[searchField];

      if (fieldValue === null || fieldValue === undefined) return false;

      return String(fieldValue).toUpperCase().includes(value);
    });
  }, [invoices, searchField, searchValue]);

  const viewInvoice = async (id) => {
    try {
      const data = await getInvoiceById(id);

      setSelectedInvoice(data);
      setShowPreview(true);
    } catch (error) {
      console.error("Error cargando detalle:", error);
      alert("No se pudo cargar la factura.");
    }
  };

  const closePreview = () => {
    setShowPreview(false);
    setSelectedInvoice(null);
  };

  const viewSelectedInvoice = () => {
    if (filteredInvoices.length === 0) return;

    const invoice = filteredInvoices[selectedIndex];

    if (!invoice) return;

    viewInvoice(invoice.id);
  };

  useEffect(() => {
    loadInvoices();
  }, []);

  useEffect(() => {
    setSelectedIndex(0);
  }, [searchField, searchValue]);

  useKeyboardShortcuts(
    [
      {
        ctrl: true,
        key: "b",
        action: () => searchInputRef.current?.focus(),
      },
      {
        key: "arrowdown",
        action: () =>
          setSelectedIndex((currentIndex) =>
            filteredInvoices.length === 0
              ? 0
              : currentIndex >= filteredInvoices.length - 1
              ? 0
              : currentIndex + 1
          ),
      },
      {
        key: "arrowup",
        action: () =>
          setSelectedIndex((currentIndex) =>
            filteredInvoices.length === 0
              ? 0
              : currentIndex <= 0
              ? filteredInvoices.length - 1
              : currentIndex - 1
          ),
      },
      {
        key: "enter",
        action: viewSelectedInvoice,
      },
      {
        key: "escape",
        action: closePreview,
      },
      {
        alt: true,
        key: "v",
        action: () => firstViewButtonRef.current?.focus(),
      },
      {
        ctrl: true,
        key: "r",
        action: loadInvoices,
      },
    ],
    [filteredInvoices, selectedIndex, selectedInvoice]
  );

  const detailRows =
    selectedInvoice?.details?.map((item) => ({
      id: item.productId,
      name: item.productName,
      price: item.price,
      quantity: item.quantity,
      subtotal: item.subtotal,
    })) ?? [];

  return (
    <Layout>
      <h1>Historial de Facturas</h1>

      <div className="card">
        <h2>Buscar facturas</h2>

        <div className="search-grid">
          <select
            value={searchField}
            onChange={(event) => {
              setSearchField(event.target.value);
              setSearchValue("");
            }}
          >
            {searchFields.map((field) => (
              <option key={field.key} value={field.key}>
                Buscar por {field.label}
              </option>
            ))}
          </select>

          <input
            ref={searchInputRef}
            placeholder="Ingrese valor de búsqueda"
            value={searchValue}
            onChange={(event) => setSearchValue(event.target.value)}
          />

          <button onClick={loadInvoices}>🔄 Recargar</button>
        </div>
      </div>

      <div className="card">
        <h2>Listado de facturas</h2>

        <DataTable
          columns={invoiceColumns}
          data={filteredInvoices.map((invoice, index) => ({
            ...invoice,
            isSelected: index === selectedIndex,
          }))}
          emptyMessage="Sin facturas encontradas"
          onRowClick={(invoice) => viewInvoice(invoice.id)}
          actions={(invoice) => (
            <button
              ref={invoice.isSelected ? firstViewButtonRef : null}
              onClick={() => viewInvoice(invoice.id)}
            >
              👁️ Ver factura
            </button>
          )}
        />
      </div>

      {selectedInvoice && (
        <InvoicePreviewModal
          isOpen={showPreview}
          invoiceNumber={selectedInvoice.invoiceNumber}
          invoiceDate={formatDateTime(selectedInvoice.date)}
          customer={{
            firstName: selectedInvoice.customerName,
            lastName: "",
            phone: selectedInvoice.customerPhone,
            address: selectedInvoice.customerAddress,
            email: selectedInvoice.customerEmail,
          }}
          details={detailRows}
          subtotal={selectedInvoice.subtotal}
          iva={selectedInvoice.tax}
          total={selectedInvoice.total}
          onBack={() => setShowPreview(false)}
          onCloseAndClean={closePreview}
        />
      )}
    </Layout>
  );
}