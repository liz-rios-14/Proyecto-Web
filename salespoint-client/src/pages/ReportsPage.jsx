import { useState } from "react";
import Layout from "../components/Layout";
import DataTable from "../components/DataTable";
import { api } from "../api/apiClient";
import { getApiErrorMessage } from "../api/apiError";
import { useAppAlert } from "../components/AppAlert";

const today = new Date().toISOString().slice(0, 10);
const monthStart = `${today.slice(0, 8)}01`;
const moneyColumns = [
  { key: "dateLabel", label: "Fecha" },
  { key: "invoiceCount", label: "Facturas", type: "number" },
  { key: "subtotal", label: "Subtotal", type: "money" },
  { key: "tax", label: "IVA", type: "money" },
  { key: "total", label: "Total", type: "money" },
];

export default function ReportsPage() {
  const { showAlert } = useAppAlert();
  const [startDate, setStartDate] = useState(monthStart);
  const [endDate, setEndDate] = useState(today);
  const [report, setReport] = useState(null);
  const [loading, setLoading] = useState(false);

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
      const response = await api.get("/reports/export/excel", {
        params: { startDate, endDate },
        responseType: "blob",
      });
      const url = URL.createObjectURL(response.data);
      const link = document.createElement("a");
      link.href = url;
      link.download = `reporte-ventas-${startDate}-${endDate}.csv`;
      link.click();
      URL.revokeObjectURL(url);
      showAlert("Reporte exportado correctamente.", "success");
    } catch (error) {
      showAlert(getApiErrorMessage(error, "No se pudo exportar el reporte."), "error");
    }
  };

  const exportPdf = () => {
    if (!report?.totals?.invoiceCount) {
      showAlert("No existen datos para exportar.", "warning");
      return;
    }
    window.print();
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
        <div className="report-filter-grid">
          <label>Fecha inicio<input type="date" value={startDate} onChange={(e) => setStartDate(e.target.value)} /></label>
          <label>Fecha fin<input type="date" value={endDate} onChange={(e) => setEndDate(e.target.value)} /></label>
          <button type="button" disabled={loading} onClick={search}>{loading ? "Buscando..." : "Buscar"}</button>
          <button type="button" className="secondary-button" onClick={exportPdf}>Exportar PDF</button>
          <button type="button" className="secondary-button" onClick={exportExcel}>Exportar Excel</button>
        </div>
      </div>

      {report && (
        <section className="report-print-area">
          <div className="card">
            <h1>Reporte de ventas</h1>
            <p>Desde {startDate} hasta {endDate}</p>
          </div>
          <div className="card report-totals">
            <h2>Totales generales</h2>
            <strong>Facturas: {report.totals.invoiceCount}</strong>
            <strong>Subtotal: ${Number(report.totals.subtotal).toFixed(2)}</strong>
            <strong>IVA: ${Number(report.totals.tax).toFixed(2)}</strong>
            <strong>Total: ${Number(report.totals.total).toFixed(2)}</strong>
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
