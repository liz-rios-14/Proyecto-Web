import { getRoleLabel } from "../services/roleLabels";

export default function InvoicePreviewModal({
  isOpen,
  invoiceNumber,
  invoiceDate,
  customer,
  seller,
  details = [],
  subtotal = 0,
  iva = 0,
  total = 0,
  auditComparison = [],
  onBack,
  onCloseAndClean,
  onCreateNewSale,
}) {
  if (!isOpen) return null;

  const isFinalized = invoiceNumber && String(invoiceNumber).trim() !== "";
  const visualInvoiceNumber = isFinalized
    ? invoiceNumber
    : "Se generará al finalizar";

  const hasChanges = auditComparison && auditComparison.length > 0;

  const safeValue = (value, fallback = "No registrado") => {
    if (value === null || value === undefined || value === "") return fallback;
    return value;
  };

  const money = (value) => Number(value || 0).toFixed(2);

  const customerData = {
    id: safeValue(customer?.id),
    name:
      `${customer?.firstName ?? ""} ${customer?.lastName ?? ""}`.trim() ||
      "No registrado",
    phone: safeValue(customer?.phone),
    address: safeValue(customer?.address, "No registrada"),
    email: safeValue(customer?.email),
  };

  const sellerData = {
    userName: safeValue(seller?.userName || seller?.username, "admin"),
    fullName: safeValue(
      seller?.fullName || seller?.name,
      "ADMINISTRADOR DEL SISTEMA"
    ),
    role: getRoleLabel(safeValue(seller?.role, "ADMINISTRATOR")),
  };

  const buildInvoiceHtml = () => {
    const rows =
      details.length > 0
        ? details
            .map(
              (item, index) => `
          <tr class="${index % 2 === 0 ? "row-normal" : "row-alt"}">
            <td class="number">${safeValue(item.id, "-")}</td>
            <td>${safeValue(item.name || item.productName, "-")}</td>
            <td class="number">$${money(item.price || item.unitPrice)}</td>
            <td class="number">${safeValue(item.quantity, "0")}</td>
            <td class="number">$${money(item.subtotal)}</td>
          </tr>
        `
            )
            .join("")
        : `<tr><td colspan="5">Sin productos agregados</td></tr>`;

    const changesRows =
      hasChanges
        ? auditComparison
            .map(
              (change) => `
            <tr>
              <td>${safeValue(change.label, "-")}</td>
              <td>${safeValue(change.original, "-")}</td>
              <td>${safeValue(change.current, "-")}</td>
            </tr>
          `
            )
            .join("")
        : "";

    return `
      <html>
        <head>
          <title>Factura ${visualInvoiceNumber}</title>
          <style>
            @page { size: A4; margin: 12mm; }
            * { box-sizing: border-box; }
            body {
              font-family: Arial, sans-serif;
              color: #111827;
              margin: 0;
              padding: 0;
              -webkit-print-color-adjust: exact;
              print-color-adjust: exact;
            }
            .invoice-paper { width: 100%; padding: 18px 22px; }
            .header {
              text-align: center;
              margin-bottom: 22px;
              border-bottom: 2px solid #111827;
              padding-bottom: 14px;
            }
            h1 { font-size: 32px; margin: 0; }
            .subtitle {
              margin: 8px 0 0;
              color: #475569;
              font-size: 14px;
              font-weight: bold;
            }
            .audit-note {
              margin-top: 8px;
              font-size: 12px;
              color: #92400e;
              font-weight: bold;
            }
            .audit-grid {
              display: grid;
              grid-template-columns: 1fr 1fr;
              gap: 16px;
              margin-bottom: 22px;
            }
            .audit-card {
              border: 1px solid #cbd5e1;
              border-radius: 12px;
              padding: 14px;
              background: #f8fafc;
            }
            .audit-card h3 {
              margin: 0 0 12px;
              padding-left: 10px;
              border-left: 4px solid #2563eb;
              font-size: 15px;
            }
            .audit-card p {
              display: grid;
              grid-template-columns: 95px 1fr;
              gap: 8px;
              margin: 7px 0;
              font-size: 12px;
            }
            .section-title {
              margin: 18px 0 10px;
              padding-left: 10px;
              border-left: 4px solid #16a34a;
              font-size: 15px;
              font-weight: bold;
            }
            .change-title { border-left-color: #f97316; }
            table {
              width: 100%;
              border-collapse: collapse;
              border: 1px solid #cbd5e1;
            }
            th {
              background: #111827;
              color: #fff;
              padding: 10px;
              text-align: left;
              font-size: 12px;
            }
            td {
              padding: 9px 10px;
              font-size: 12px;
              border: 1px solid #cbd5e1;
            }
            .changes-table th { background: #f97316; }
            .changes-table td {
              background: #fff7ed;
              color: #7c2d12;
              font-weight: bold;
            }
            .row-normal { background: #fff; }
            .row-alt { background: #f8fafc; }
            .number { text-align: right; }
            .totals {
              width: 310px;
              margin-left: auto;
              margin-top: 20px;
              font-size: 13px;
              border: 1px solid #cbd5e1;
              border-radius: 10px;
              padding: 12px;
              background: #f8fafc;
            }
            .totals p {
              display: flex;
              justify-content: space-between;
              margin: 7px 0;
            }
            .total {
              border-top: 2px solid #cbd5e1;
              padding-top: 10px;
              margin-top: 10px !important;
              font-size: 19px;
              font-weight: bold;
            }
          </style>
        </head>
        <body>
          <div class="invoice-paper">
            <div class="header">
              <h1>Factura</h1>
              <p class="subtitle">Factura N° ${visualInvoiceNumber}</p>
              <p class="audit-note">Representación generada para reconstrucción y auditoría de venta</p>
            </div>

            <div class="audit-grid">
              <section class="audit-card">
                <h3>Información del cliente</h3>
                <p><strong>Fecha:</strong> <span>${safeValue(invoiceDate)}</span></p>
                <p><strong>ID:</strong> <span>${customerData.id}</span></p>
                <p><strong>Cliente:</strong> <span>${customerData.name}</span></p>
                <p><strong>Teléfono:</strong> <span>${customerData.phone}</span></p>
                <p><strong>Dirección:</strong> <span>${customerData.address}</span></p>
                <p><strong>Correo:</strong> <span>${customerData.email}</span></p>
              </section>

              <section class="audit-card">
                <h3>Información del vendedor</h3>
                <p><strong>Usuario:</strong> <span>${sellerData.userName}</span></p>
                <p><strong>Nombre:</strong> <span>${sellerData.fullName}</span></p>
                <p><strong>Rol:</strong> <span>${sellerData.role}</span></p>
                <p><strong>Origen:</strong> <span>Reconstrucción por auditoría</span></p>
              </section>
            </div>

            ${
              hasChanges
                ? `
                  <p class="section-title change-title">Cambios detectados antes de generar la nueva factura</p>
                  <table class="changes-table">
                    <thead>
                      <tr>
                        <th>Campo</th>
                        <th>Dato original</th>
                        <th>Dato nuevo</th>
                      </tr>
                    </thead>
                    <tbody>${changesRows}</tbody>
                  </table>
                `
                : ""
            }

            <p class="section-title">Productos vendidos</p>

            <table>
              <thead>
                <tr>
                  <th class="number">Id</th>
                  <th>Producto</th>
                  <th class="number">Precio aplicado</th>
                  <th class="number">Cantidad</th>
                  <th class="number">Subtotal</th>
                </tr>
              </thead>
              <tbody>${rows}</tbody>
            </table>

            <div class="totals">
              <p><strong>Subtotal:</strong> <span>$${money(subtotal)}</span></p>
              <p><strong>IVA 12%:</strong> <span>$${money(iva)}</span></p>
              <p class="total"><strong>Total:</strong> <span>$${money(total)}</span></p>
            </div>
          </div>
        </body>
      </html>
    `;
  };

  const printInvoice = () => {
    const iframe = document.createElement("iframe");
    iframe.style.position = "fixed";
    iframe.style.right = "0";
    iframe.style.bottom = "0";
    iframe.style.width = "0";
    iframe.style.height = "0";
    iframe.style.border = "0";

    document.body.appendChild(iframe);

    const iframeDocument = iframe.contentWindow.document;
    iframeDocument.open();
    iframeDocument.write(buildInvoiceHtml());
    iframeDocument.close();

    iframe.onload = () => {
      iframe.contentWindow.focus();
      iframe.contentWindow.print();

      setTimeout(() => {
        document.body.removeChild(iframe);
      }, 1000);
    };
  };

  return (
    <div className="invoice-preview-overlay">
      <style>
        {`
          .invoice-preview-overlay {
            position: fixed;
            inset: 0;
            background: rgba(15, 23, 42, 0.78);
            z-index: 9999;
            display: flex;
            align-items: flex-start;
            justify-content: center;
            padding: 10px 18px;
            overflow-y: auto;
          }

          .invoice-preview-modal {
            width: min(1120px, 100%);
            max-height: calc(100vh - 20px);
            background: #ffffff;
            border-radius: 18px;
            padding: 20px 24px;
            display: flex;
            flex-direction: column;
            overflow: hidden;
          }

          .invoice-preview-actions {
            display: grid;
            grid-template-columns: repeat(4, max-content);
            justify-content: space-between;
            align-items: center;
            gap: 12px;
            margin-bottom: 16px;
            flex-shrink: 0;
          }

          .invoice-print-area {
            overflow-y: auto;
            padding-right: 4px;
          }

          .invoice-document {
            border: 1px solid #cbd5e1;
            border-radius: 14px;
            padding: 40px 46px;
            background: #ffffff;
            color: #111827;
          }

          .invoice-document h1 {
            text-align: center;
            font-size: 36px;
            margin: 0 0 10px;
          }

          .invoice-subtitle {
            text-align: center;
            color: #64748b;
            margin: 6px 0 22px;
          }

          .invoice-audit-grid {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 22px;
            margin: 26px auto 26px;
            max-width: 900px;
          }

          .invoice-audit-card {
            border: 1px solid #cbd5e1;
            border-radius: 16px;
            background: #f8fafc;
            padding: 22px;
          }

          .invoice-audit-card h3 {
            margin: 0 0 18px;
            padding-left: 12px;
            border-left: 4px solid #2563eb;
            font-size: 20px;
          }

          .invoice-audit-card p {
            display: grid;
            grid-template-columns: 115px 1fr;
            gap: 10px;
            margin: 10px 0;
            font-size: 15px;
          }

          .audit-changes-card {
            max-width: 900px;
            margin: 0 auto 26px;
            background: transparent;
            border: 0;
            padding: 0;
          }

          .audit-changes-card h3 {
            border-left-color: #f97316;
          }

          .invoice-section-title {
            max-width: 900px;
            margin: 26px auto 18px;
            padding-left: 12px;
            border-left: 4px solid #16a34a;
            font-size: 22px;
          }

          .invoice-table {
            width: 100%;
            max-width: 900px;
            margin: 0 auto;
            border-collapse: collapse;
            overflow: hidden;
            border-radius: 12px;
          }

          .invoice-table th {
            background: #111827;
            color: white;
            padding: 13px 14px;
            font-size: 15px;
            text-align: left;
          }

          .invoice-table td {
            padding: 13px 14px;
            font-size: 15px;
            border-bottom: 1px solid #cbd5e1;
          }

          .audit-changes-card .invoice-table th {
            background: #f97316;
          }

          .audit-changes-card .invoice-table td {
            background: #fff7ed;
            color: #7c2d12;
            font-weight: 700;
          }

          .number-cell {
            text-align: right;
          }

          .invoice-totals-preview {
            width: 310px;
            margin: 24px 74px 0 auto;
            border-top: 1px solid #cbd5e1;
            padding-top: 12px;
          }

          .invoice-totals-preview p {
            display: flex;
            justify-content: space-between;
            margin: 8px 0;
            font-size: 15px;
          }

          .invoice-totals-preview h2 {
            border-top: 1px solid #cbd5e1;
            padding-top: 18px;
            margin-top: 16px;
            font-size: 26px;
            text-align: left;
          }

          @media (max-width: 850px) {
            .invoice-preview-actions {
              grid-template-columns: 1fr 1fr;
            }

            .invoice-audit-grid {
              grid-template-columns: 1fr;
            }

            .invoice-document {
              padding: 28px 18px;
            }

            .invoice-totals-preview {
              margin-right: 0;
              width: 100%;
            }
          }
        `}
      </style>

      <div className="invoice-preview-modal">
        <div className="invoice-preview-actions no-print">
          <button className="back-button" onClick={onBack}>
            ← Regresar
          </button>

          <button className="print-button" onClick={printInvoice}>
            🖨 Imprimir / Guardar PDF
          </button>

          {isFinalized && onCreateNewSale && (
            <button
              className="invoice-button"
              type="button"
              onClick={onCreateNewSale}
            >
              🧾 Reconstruir Factura
            </button>
          )}

          <button
            className="finish-button"
            onClick={onCloseAndClean}
            disabled={isFinalized}
          >
            {isFinalized ? "✅ Venta finalizada" : "✅ Finalizar venta"}
          </button>
        </div>

        <div className="invoice-print-area">
          <div className="invoice-document">
            <h1>Factura</h1>

            <p className="invoice-subtitle">Factura N° {visualInvoiceNumber}</p>

            <p className="invoice-subtitle">
              Representación generada para reconstrucción y auditoría de venta
            </p>

            <div className="invoice-audit-grid">
              <section className="invoice-audit-card">
                <h3>Información del cliente</h3>

                <p><strong>Fecha:</strong><span>{safeValue(invoiceDate)}</span></p>
                <p><strong>ID:</strong><span>{customerData.id}</span></p>
                <p><strong>Cliente:</strong><span>{customerData.name}</span></p>
                <p><strong>Teléfono:</strong><span>{customerData.phone}</span></p>
                <p><strong>Dirección:</strong><span>{customerData.address}</span></p>
                <p><strong>Correo:</strong><span>{customerData.email}</span></p>
              </section>

              <section className="invoice-audit-card">
                <h3>Información del vendedor</h3>

                <p><strong>Usuario:</strong><span>{sellerData.userName}</span></p>
                <p><strong>Nombre:</strong><span>{sellerData.fullName}</span></p>
                <p><strong>Rol:</strong><span>{sellerData.role}</span></p>
                <p><strong>Origen:</strong><span>Reconstrucción por auditoría</span></p>
              </section>
            </div>

            {hasChanges && (
              <section className="audit-changes-card">
                <h3>Cambios detectados antes de generar la nueva factura</h3>

                <table className="invoice-table">
                  <thead>
                    <tr>
                      <th>Campo</th>
                      <th>Dato original</th>
                      <th>Dato nuevo</th>
                    </tr>
                  </thead>

                  <tbody>
                    {auditComparison.map((change, index) => (
                      <tr key={`${change.label}-${index}`}>
                        <td>{change.label}</td>
                        <td>{change.original}</td>
                        <td>{change.current}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </section>
            )}

            <h3 className="invoice-section-title">Productos vendidos</h3>

            <table className="invoice-table">
              <thead>
                <tr>
                  <th className="number-cell">Id</th>
                  <th>Producto</th>
                  <th className="number-cell">Precio aplicado</th>
                  <th className="number-cell">Cantidad</th>
                  <th className="number-cell">Subtotal</th>
                </tr>
              </thead>

              <tbody>
                {details.length === 0 ? (
                  <tr>
                    <td colSpan="5">Sin productos agregados</td>
                  </tr>
                ) : (
                  details.map((item, index) => (
                    <tr key={`${item.id}-${index}`}>
                      <td className="number-cell">{item.id}</td>
                      <td>{item.name || item.productName}</td>
                      <td className="number-cell">
                        ${money(item.price || item.unitPrice)}
                      </td>
                      <td className="number-cell">{item.quantity}</td>
                      <td className="number-cell">${money(item.subtotal)}</td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>

            <div className="invoice-totals-preview">
              <p><strong>Subtotal:</strong> ${money(subtotal)}</p>
              <p><strong>IVA 12%:</strong> ${money(iva)}</p>
              <h2>Total: ${money(total)}</h2>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
