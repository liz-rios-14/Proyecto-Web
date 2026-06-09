import { useEffect, useMemo, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import Layout from "../components/Layout";
import DataTable from "../components/DataTable";
import InvoicePreviewModal from "../components/InvoicePreviewModal";
import useKeyboardShortcuts from "../hooks/useKeyboardShortcuts";
import Pagination from "../components/Pagination";
import { getAuditInvoiceHistory, getInvoices, reconstructInvoiceByNumber } from "../api/invoiceApi";
import { getApiErrorMessage } from "../api/apiError";
import { useAppAlert } from "../components/AppAlert";
import { getAuthRole } from "../services/authStorage";

const invoiceColumns = [
  { key: "invoiceNumber", label: "Factura" },
  { key: "customerName", label: "Cliente" },
  { key: "formattedDate", label: "Fecha" },
  { key: "total", label: "Total", type: "money" },
];

const auditHistoryColumns = [
  { key: "originalInvoiceNumber", label: "Factura original" },
  { key: "generatedInvoiceNumber", label: "Factura generada" },
  { key: "formattedGeneratedAt", label: "Fecha" },
  { key: "generatedByLabel", label: "Usuario" },
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

const splitCustomerName = (fullName = "") => {
  const cleanName = String(fullName).trim().replace(/\s+/g, " ");

  if (!cleanName) {
    return {
      firstName: "",
      lastName: "",
    };
  }

  const parts = cleanName.split(" ");

  if (parts.length <= 2) {
    return {
      firstName: parts[0] ?? "",
      lastName: parts.slice(1).join(" "),
    };
  }

  return {
    firstName: parts.slice(0, 2).join(" "),
    lastName: parts.slice(2).join(" "),
  };
};

const getValue = (...values) => {
  const value = values.find(
    (item) => item !== null && item !== undefined && item !== ""
  );

  return value ?? "";
};

const getNumber = (...values) => {
  const value = getValue(...values);
  const number = Number(value);
  return Number.isFinite(number) ? number : 0;
};

export default function InvoiceHistoryPage() {
  const navigate = useNavigate();
  const { showAlert } = useAppAlert();
  const isAdministrator = getAuthRole() === "ADMINISTRATOR";

  const [invoices, setInvoices] = useState([]);
  const [selectedInvoice, setSelectedInvoice] = useState(null);
  const [showPreview, setShowPreview] = useState(false);
  const [selectedIndex, setSelectedIndex] = useState(0);
  const [searchField, setSearchField] = useState("invoiceNumber");
  const [searchValue, setSearchValue] = useState("");
  const [auditInvoiceNumber, setAuditInvoiceNumber] = useState("");
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(1);
  const [auditHistory, setAuditHistory] = useState([]);
  const [auditHistoryPage, setAuditHistoryPage] = useState(1);
  const [auditHistoryTotalPages, setAuditHistoryTotalPages] = useState(1);
  const [auditHistoryPageSize, setAuditHistoryPageSize] = useState(10);

  const firstViewButtonRef = useRef(null);
  const searchInputRef = useRef(null);
  const auditInputRef = useRef(null);

  const loadInvoices = async (currentPage = page) => {
    try {
      const data = await getInvoices(currentPage, pageSize);

      const formattedInvoices = (data.items ?? []).map((invoice) => ({
        ...invoice,
        formattedDate: formatDateTime(invoice.date),
      }));

      setInvoices(formattedInvoices);
      setTotalPages(data.totalPages ?? 1);
      setSelectedIndex(0);
    } catch (error) {
      console.error("Error cargando facturas:", error);
      showAlert(
        getApiErrorMessage(error, "No se pudo cargar el historial de facturas."),
        "error"
      );
    }
  };

  const handleReload = () => {
    setSearchValue("");
    setAuditInvoiceNumber("");
    setPage(1);
    loadInvoices(1);
    if (isAdministrator) {
      loadAuditHistory(1, auditHistoryPageSize);
    }
  };

  const loadAuditHistory = async (
    currentPage = auditHistoryPage,
    currentPageSize = auditHistoryPageSize
  ) => {
    try {
      const data = await getAuditInvoiceHistory(currentPage, currentPageSize);

      setAuditHistory(
        (data.items ?? []).map((item) => ({
          ...item,
          formattedGeneratedAt: formatDateTime(item.generatedAt),
          generatedByLabel: `Usuario ${item.generatedByUserId}`,
        }))
      );
      setAuditHistoryTotalPages(Math.max(data.totalPages ?? 1, 1));
    } catch (error) {
      setAuditHistory([]);
      setAuditHistoryTotalPages(1);
      showAlert(
        getApiErrorMessage(error, "No se pudo cargar el historico de auditoria."),
        "error"
      );
    }
  };

  const filteredInvoices = useMemo(() => {
    if (!searchValue.trim()) return invoices;

    const value = searchValue.trim().toUpperCase();

    return invoices.filter((invoice) => {
      const fieldValue = invoice[searchField];

      if (fieldValue === null || fieldValue === undefined) {
        return false;
      }

      if (searchField === "total") {
        return Number(fieldValue) === Number(searchValue);
      }

      return String(fieldValue).toUpperCase().startsWith(value);
    });
  }, [invoices, searchField, searchValue]);

  const mapReconstructedInvoice = (data) => {
    const rawCustomer = data.customer ?? {};

    const fullCustomerName = getValue(
      rawCustomer.fullName,
      rawCustomer.customerName,
      rawCustomer.name,
      data.customerName
    );

    const separatedName = splitCustomerName(fullCustomerName);

    const firstName = getValue(
      rawCustomer.firstName,
      rawCustomer.names,
      separatedName.firstName
    );

    const lastName = getValue(
      rawCustomer.lastName,
      rawCustomer.surnames,
      rawCustomer.lastname,
      separatedName.lastName
    );

    const mappedDetails =
      data.details?.map((item) => {
        const price = getNumber(
          item.unitPrice,
          item.price,
          item.appliedPrice,
          item.salePrice
        );

        const quantity = getNumber(item.quantity, item.amount);

        const subtotal =
          getNumber(item.subtotal, item.lineSubtotal) ||
          Number((price * quantity).toFixed(2));

        return {
          id: getValue(item.productId, item.id),
          productId: getValue(item.productId, item.id),
          name: getValue(item.productName, item.name, item.description),
          productName: getValue(item.productName, item.name, item.description),
          price,
          unitPrice: price,
          quantity,
          subtotal,
          stock: getNumber(item.stock, quantity),
        };
      }) ?? [];

    const subtotal =
      getNumber(data.subtotal, data.subTotal) ||
      mappedDetails.reduce((sum, item) => sum + Number(item.subtotal), 0);

    const tax =
      getNumber(data.tax, data.iva, data.vat) ||
      Number((subtotal * 0.12).toFixed(2));

    const total = getNumber(data.total) || Number((subtotal + tax).toFixed(2));

    return {
      invoiceNumber: data.invoiceNumber,
      date: getValue(data.date, data.invoiceDate, data.createdAt),
      customer: {
        id: getNumber(rawCustomer.customerId, rawCustomer.id, data.customerId),
        customerId: getNumber(
          rawCustomer.customerId,
          rawCustomer.id,
          data.customerId
        ),
        firstName,
        lastName,
        cedula: getValue(
          rawCustomer.cedula,
          rawCustomer.customerCedula,
          data.customerCedula
        ),
        phone: getValue(rawCustomer.phone, rawCustomer.telephone, data.phone),
        address: getValue(
          rawCustomer.address,
          rawCustomer.direction,
          rawCustomer.customerAddress,
          data.customerAddress,
          data.address
        ),
        email: getValue(rawCustomer.email, data.email),
      },
      seller: {
        sellerId: getNumber(data.seller?.sellerId, data.seller?.id),
        userName: getValue(
          data.seller?.userName,
          data.seller?.username,
          data.sellerUserName,
          "admin"
        ),
        fullName: getValue(
          data.seller?.fullName,
          data.seller?.name,
          data.sellerName,
          "ADMINISTRADOR DEL SISTEMA"
        ),
        role: getValue(data.seller?.role, data.sellerRole, "ADMINISTRATOR"),
      },
      details: mappedDetails,
      subtotal,
      tax,
      iva: tax,
      total,
    };
  };

  const reconstructByNumber = async (invoiceNumber) => {
    const cleanNumber = invoiceNumber?.trim().toUpperCase();

    if (!cleanNumber) {
      showAlert("Ingrese el número de factura para auditoría.", "warning");
      auditInputRef.current?.focus();
      return;
    }

    try {
      setAuditInvoiceNumber(cleanNumber);

      const data = await reconstructInvoiceByNumber(cleanNumber);
      const mappedInvoice = mapReconstructedInvoice(data);

      setSelectedInvoice(mappedInvoice);
      setShowPreview(true);
      showAlert(
        `Factura ${mappedInvoice.invoiceNumber} reconstruida correctamente.`,
        "success"
      );
    } catch (error) {
      console.error("Error reconstruyendo factura:", error);
      showAlert(
        getApiErrorMessage(error, "No se pudo reconstruir la factura."),
        "error"
      );
    }
  };

  const reconstructInvoice = async () => {
    await reconstructByNumber(auditInvoiceNumber);
  };

  const closePreview = () => {
    setShowPreview(false);
    setSelectedInvoice(null);
  };

  const selectInvoiceRow = (invoice) => {
    setAuditInvoiceNumber(invoice.invoiceNumber);
  };

  const viewSelectedInvoice = () => {
    if (filteredInvoices.length === 0) return;

    const invoice = filteredInvoices[selectedIndex];

    if (!invoice) return;

    reconstructByNumber(invoice.invoiceNumber);
  };

  const createNewSaleFromAudit = () => {
    if (!selectedInvoice) return;

    navigate("/sales", {
      state: {
        reconstructedInvoice: selectedInvoice,
      },
    });
  };

  useEffect(() => {
    loadInvoices(page);
  }, [page, pageSize]);

  useEffect(() => {
    if (!isAdministrator) return;

    loadAuditHistory(auditHistoryPage, auditHistoryPageSize);
  }, [auditHistoryPage, auditHistoryPageSize, isAdministrator]);

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
        action: handleReload,
      },
    ],
    [filteredInvoices, selectedIndex, selectedInvoice]
  );

  return (
    <Layout>
      <h1>Historial de Facturas</h1>

      <div className="card">
        <h2>Reconstrucción por auditoría</h2>
        <p>
          Ingrese el número de factura para reconstruir la venta con los datos
          originales almacenados.
        </p>

        <div className="search-grid">
          <input
            ref={auditInputRef}
            placeholder="Ejemplo: FAC-0000000001"
            value={auditInvoiceNumber}
            onChange={(event) =>
              setAuditInvoiceNumber(event.target.value.toUpperCase())
            }
          />

          <button type="button" onClick={reconstructInvoice}>
            🧾 Reconstruir venta
          </button>

          <button type="button" onClick={handleReload}>
            🔄 Recargar
          </button>
        </div>
      </div>

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

          <button type="button" onClick={handleReload}>
            🔄 Recargar
          </button>
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
          onRowClick={selectInvoiceRow}
          onRowDoubleClick={(invoice) =>
            reconstructByNumber(invoice.invoiceNumber)
          }
          actions={(invoice) => (
            <button
              type="button"
              ref={invoice.isSelected ? firstViewButtonRef : null}
              onClick={() => reconstructByNumber(invoice.invoiceNumber)}
            >
              👁️ Ver factura
            </button>
          )}
        />

        <Pagination
          page={page}
          totalPages={totalPages}
          onPrevious={() => setPage((current) => Math.max(current - 1, 1))}
          onNext={() => setPage((current) => Math.min(current + 1, totalPages))}
          onPageChange={(newPage) => setPage(newPage)}
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
      </div>

      {isAdministrator && (
      <div className="card">
        <h2>Historico facturas auditoria</h2>

        <DataTable
          columns={auditHistoryColumns}
          data={auditHistory}
          emptyMessage="Sin facturas generadas por auditoria"
        />

        <div className="actions-row">
          <label className="page-size-control">
            Registros por pagina
            <select
              value={auditHistoryPageSize}
              onChange={(event) => {
                setAuditHistoryPageSize(Number(event.target.value));
                setAuditHistoryPage(1);
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
          page={auditHistoryPage}
          totalPages={auditHistoryTotalPages}
          onPrevious={() => setAuditHistoryPage((current) => Math.max(current - 1, 1))}
          onNext={() => setAuditHistoryPage((current) => Math.min(current + 1, auditHistoryTotalPages))}
          onPageChange={(newPage) => setAuditHistoryPage(newPage)}
        />
      </div>
      )}

      {selectedInvoice && (
        <InvoicePreviewModal
          isOpen={showPreview}
          invoiceNumber={selectedInvoice.invoiceNumber}
          invoiceDate={formatDateTime(selectedInvoice.date)}
          customer={selectedInvoice.customer}
          seller={selectedInvoice.seller}
          details={selectedInvoice.details}
          subtotal={selectedInvoice.subtotal}
          iva={selectedInvoice.tax}
          total={selectedInvoice.total}
          isAuditReconstruction={true}
          onBack={() => setShowPreview(false)}
          onCloseAndClean={closePreview}
          onCreateNewSale={createNewSaleFromAudit}
        />
      )}
    </Layout>
  );
}
