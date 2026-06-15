import { useState } from "react";
import Layout from "../components/Layout";
import DataTable from "../components/DataTable";
import { api } from "../api/apiClient";
import { getApiErrorMessage } from "../api/apiError";
import { useAppAlert } from "../components/AppAlert";
import { getAuthRole } from "../services/authStorage";
import { readStoredState, writeStoredState } from "../utils/inputSanitizers";

const formatInputDate = (date) => {
  const offset = date.getTimezoneOffset();
  return new Date(date.getTime() - offset * 60000).toISOString().slice(0, 10);
};

const today = formatInputDate(new Date());
const monthStart = `${today.slice(0, 8)}01`;
const reportStateKey = "salespoint-report-search";
const initialReportState = readStoredState(reportStateKey, {
  startDate: monthStart,
  endDate: today,
});
const moneyColumns = [
  { key: "dateLabel", label: "Fecha" },
  { key: "invoiceCount", label: "Facturas", type: "number" },
  { key: "subtotal", label: "Subtotal", type: "money" },
  { key: "tax", label: "IVA", type: "money" },
  { key: "total", label: "Total", type: "money" },
];

export default function ReportsPage() {
  const { showAlert } = useAppAlert();
  const isAdministrator = getAuthRole() === "ADMINISTRATOR";
  const [startDate, setStartDate] = useState(initialReportState.startDate);
  const [endDate, setEndDate] = useState(initialReportState.endDate);
  const [report, setReport] = useState(null);
  const [loading, setLoading] = useState(false);
  const [exportingExcel, setExportingExcel] = useState(false);

  const applyRange = (days) => {
    const end = new Date();
    const start = new Date();
    start.setDate(end.getDate() - days + 1);
    setStartDate(formatInputDate(start));
    setEndDate(formatInputDate(end));
  };

  const validate = () => {
    if (!startDate || !endDate) {
      showAlert("Seleccione la fecha inicio y la fecha fin.", "warning");
      return false;
    }
    if (endDate < startDate) {
      showAlert("La fecha fin no puede ser menor a la fecha inicio.", "warning");
      return false;
    }
    return true;
  };

  const search = async () => {
    if (!validate()) return;
    try {
      setLoading(true);
      writeStoredState(reportStateKey, { startDate, endDate });
      const response = await api.get("/reports", {
        params: { startDate, endDate },
      });
      setReport(response.data);
      if (!response.data.totals?.invoiceCount) {
        showAlert("No existen datos para el rango seleccionado.", "info");
      }
    } catch (error) {
      setReport(null);
      showAlert(getApiErrorMessage(error, "No se pudo cargar el reporte."), "error");
    } finally {
      setLoading(false);
    }
  };

  const exportExcel = async () => {
    if (!validate()) return;
    try {
      setExportingExcel(true);
      const response = await api.get("/reports/export/excel", {
        params: { startDate, endDate },
        responseType: "blob",
      });
      const url = URL.createObjectURL(response.data);
      const link = document.createElement("a");
      link.href = url;
      link.download = `reporte-ventas-${startDate}-${endDate}.xlsx`;
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.setTimeout(() => URL.revokeObjectURL(url), 1000);
      showAlert("Archivo Excel descargado correctamente.", "success");
    } catch (error) {
      showAlert(getApiErrorMessage(error, "No se pudo exportar el reporte."), "error");
    } finally {
      setExportingExcel(false);
    }
  };

  const exportPdf = () => {
    if (!report?.totals?.invoiceCount) {
      showAlert("No existen datos para exportar.", "warning");
      return;
    }

    document.body.classList.add("printing-report");
    const finishPrinting = () => {
      document.body.classList.remove("printing-report");
      window.removeEventListener("afterprint", finishPrinting);
    };

    window.addEventListener("afterprint", finishPrinting);
    window.requestAnimationFrame(() => window.print());
  };

  return (
    <Layout>
      <section className="page-heading no-print">
        <div>
          <span className="page-eyebrow">Análisis</span>
          <h1>Reportes avanzados</h1>
          <p>Ventas, productos, stock y clientes por rango de fechas.</p>
        </div>
      </section>

      <div className="card no-print">
        <div className="feature-scope-banner">
          <div>
            <strong>
              {isAdministrator ? "Alcance global" : "Alcance personal"}
            </strong>
            <span>
              {isAdministrator
                ? "El reporte incluye las ventas de todos los vendedores."
                : "Por seguridad, solo se incluyen las ventas realizadas por su usuario."}
            </span>
          </div>
          <span className="scope-badge">
            {isAdministrator ? "ADMINISTRADOR" : "VENDEDOR"}
          </span>
        </div>

        <div className="report-quick-ranges">
          <span>Rangos rápidos:</span>
          <button type="button" className="filter-chip" onClick={() => applyRange(1)}>
            Hoy
          </button>
          <button type="button" className="filter-chip" onClick={() => applyRange(7)}>
            7 días
          </button>
          <button type="button" className="filter-chip" onClick={() => applyRange(30)}>
            30 días
          </button>
          <button
            type="button"
            className="filter-chip"
            onClick={() => {
              setStartDate(monthStart);
              setEndDate(today);
            }}
          >
            Mes actual
          </button>
        </div>

        <div className="report-filter-grid">
          <label>
            Fecha inicio
            <input type="date" value={startDate} onChange={(e) => setStartDate(e.target.value)} />
          </label>
          <label>
            Fecha fin
            <input type="date" value={endDate} onChange={(e) => setEndDate(e.target.value)} />
          </label>
          <button type="button" disabled={loading} onClick={search}>{loading ? "Buscando..." : "Buscar"}</button>
          <button type="button" className="secondary-button" onClick={exportPdf}>Exportar PDF</button>
          <button
            type="button"
            className="excel-export-button"
            disabled={exportingExcel}
            onClick={exportExcel}
          >
            {exportingExcel ? "Generando Excel..." : "Descargar Excel (.xlsx)"}
          </button>
        </div>
      </div>

      {report && (
        <section className="report-print-area">
          <div className="card report-document-heading">
            <h1>Reporte de ventas</h1>
            <p>
              Desde {startDate} hasta {endDate} ·{" "}
              {isAdministrator ? "Todas las ventas" : "Ventas del usuario actual"}
            </p>
          </div>

          <div className="report-kpi-grid">
            <article className="report-kpi">
              <span>Facturas</span>
              <strong>{report.totals.invoiceCount}</strong>
              <small>Documentos emitidos</small>
            </article>
            <article className="report-kpi">
              <span>Subtotal</span>
              <strong>${Number(report.totals.subtotal).toFixed(2)}</strong>
              <small>Antes de impuestos</small>
            </article>
            <article className="report-kpi">
              <span>IVA</span>
              <strong>${Number(report.totals.tax).toFixed(2)}</strong>
              <small>Impuesto acumulado</small>
            </article>
            <article className="report-kpi report-kpi-main">
              <span>Total vendido</span>
              <strong>${Number(report.totals.total).toFixed(2)}</strong>
              <small>Valor final del período</small>
            </article>
          </div>
          <ReportTable title="Ventas por fecha" columns={moneyColumns} data={(report.salesByDate || []).map((x) => ({ ...x, dateLabel: x.date.slice(0, 10) }))} />
          <ReportTable title="Ventas por vendedor" columns={[{ key: "sellerName", label: "Vendedor" }, { key: "invoiceCount", label: "Facturas" }, { key: "total", label: "Total", type: "money" }]} data={report.salesBySeller || []} />
          <ReportTable title="Productos más vendidos" columns={[{ key: "productName", label: "Producto" }, { key: "quantity", label: "Cantidad" }, { key: "total", label: "Total", type: "money" }]} data={report.topProducts || []} />
          <ReportTable title="Stock bajo" columns={[{ key: "productName", label: "Producto" }, { key: "stock", label: "Stock" }]} data={report.lowStockProducts || []} />
          <ReportTable title="Clientes con más compras" columns={[{ key: "customerName", label: "Cliente" }, { key: "invoiceCount", label: "Facturas" }, { key: "total", label: "Total", type: "money" }]} data={report.topCustomers || []} />
        </section>
      )}
    </Layout>
  );
}

function ReportTable({ title, columns, data }) {
  return (
    <div className="card">
      <h2>{title}</h2>
      <DataTable columns={columns} data={data} emptyMessage="No existen datos" />
    </div>
  );
}
