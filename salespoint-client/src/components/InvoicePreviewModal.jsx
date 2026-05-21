export default function InvoicePreviewModal({
  isOpen,
  invoiceNumber,
  invoiceDate,
  customer,
  details,
  subtotal,
  iva,
  total,
  onBack,
  onCloseAndClean,
}) {
  if (!isOpen) return null;

  const isFinalized = invoiceNumber && invoiceNumber.trim() !== "";
  const visualInvoiceNumber = isFinalized ? invoiceNumber : "Se generará al finalizar";

  const buildInvoiceHtml = () => {
    const printColors = {
      bodyBg: "#ffffff",
      paperBg: "#ffffff",
      text: "#000000",
      subtitle: "#334155",
      tableHeader: "#e5e7eb",
      tableHeaderText: "#000000",
      tableBorder: "#cbd5e1",
      rowBg: "#ffffff",
      rowAltBg: "#f8fafc",
    };

    const rows = details
      .map(
        (item, index) => `
          <tr style="background:${index % 2 === 0 ? printColors.rowBg : printColors.rowAltBg};">
            <td class="number">${item.id}</td>
            <td>${item.name}</td>
            <td class="number">$${Number(item.price).toFixed(2)}</td>
            <td class="number">${item.quantity}</td>
            <td class="number">$${Number(item.subtotal).toFixed(2)}</td>
          </tr>
        `
      )
      .join("");

    return `
      <html>
        <head>
          <title>${visualInvoiceNumber}</title>
          <style>
            @page { size: A4; margin: 14mm; }
            * { box-sizing: border-box; }

            body {
              font-family: Arial, sans-serif;
              background: ${printColors.bodyBg};
              color: ${printColors.text};
              margin: 0;
              padding: 0;
              -webkit-print-color-adjust: exact;
              print-color-adjust: exact;
            }

            .invoice-paper {
              width: 100%;
              min-height: 100%;
              background: ${printColors.paperBg};
              padding: 22px 26px;
            }

            h1 {
              text-align: center;
              font-size: 30px;
              margin: 0 0 6px 0;
              color: ${printColors.text};
            }

            .subtitle {
              text-align: center;
              color: ${printColors.subtitle};
              margin: 0 0 28px 0;
              font-weight: bold;
              font-size: 14px;
            }

            .info {
              display: grid;
              grid-template-columns: 1fr 1fr;
              column-gap: 60px;
              row-gap: 12px;
              font-size: 13px;
              margin-bottom: 26px;
            }

            .info p,
            .info strong {
              color: ${printColors.text};
              margin: 0;
            }

            table {
              width: 100%;
              border-collapse: collapse;
              border: 1px solid ${printColors.tableBorder};
              margin-top: 10px;
            }

            th {
              background: ${printColors.tableHeader};
              color: ${printColors.tableHeaderText};
              padding: 11px 10px;
              text-align: left;
              font-size: 13px;
              border: 1px solid ${printColors.tableBorder};
            }

            td {
              padding: 10px;
              font-size: 12px;
              color: ${printColors.text};
              border: 1px solid ${printColors.tableBorder};
            }

            .number { text-align: right; }

            .totals {
              width: 280px;
              margin-left: auto;
              margin-top: 24px;
              font-size: 13px;
            }

            .totals p {
              display: flex;
              justify-content: space-between;
              margin: 7px 0;
              color: ${printColors.text};
            }

            .totals strong,
            .totals span {
              color: ${printColors.text};
            }

            .total {
              border-top: 2px solid ${printColors.tableBorder};
              padding-top: 10px;
              margin-top: 10px !important;
              font-size: 18px;
              font-weight: bold;
            }
          </style>
        </head>

        <body>
          <div class="invoice-paper">
            <h1>Factura</h1>
            <p class="subtitle">Factura N° ${visualInvoiceNumber}</p>

            <div class="info">
              <p><strong>Fecha:</strong> ${invoiceDate}</p>
              <p><strong>Cliente:</strong> ${customer?.firstName ?? ""} ${customer?.lastName ?? ""}</p>
              <p><strong>Teléfono:</strong> ${customer?.phone ?? ""}</p>
              <p><strong>Dirección:</strong> ${customer?.address ?? ""}</p>
              <p><strong>Correo:</strong> ${customer?.email ?? ""}</p>
            </div>

            <table>
              <thead>
                <tr>
                  <th class="number">Id</th>
                  <th>Producto</th>
                  <th class="number">Precio</th>
                  <th class="number">Cantidad</th>
                  <th class="number">Subtotal</th>
                </tr>
              </thead>
              <tbody>${rows}</tbody>
            </table>

            <div class="totals">
              <p><strong>Subtotal:</strong> <span>$${subtotal.toFixed(2)}</span></p>
              <p><strong>IVA:</strong> <span>$${iva.toFixed(2)}</span></p>
              <p class="total"><strong>Total:</strong> <span>$${total.toFixed(2)}</span></p>
            </div>
          </div>
        </body>
      </html>
    `;
  };

  const printInvoice = () => {
    if (!isFinalized) {
      alert("Primero finalice la venta para generar el número de factura.");
      return;
    }

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
      onBack();

      setTimeout(() => {
        document.body.removeChild(iframe);
      }, 1000);
    };
  };

  return (
    <div className="invoice-preview-overlay">
      <div className="invoice-preview-modal">
        <div className="invoice-preview-actions no-print">
          <button className="back-button" onClick={onBack}>
            ← Regresar
          </button>

          <button
            className={`print-button ${!isFinalized ? "disabled-button" : ""}`}
            onClick={printInvoice}
            disabled={!isFinalized}
            title={!isFinalized ? "Primero finalice la venta antes de imprimir" : ""}
          >
            🖨 Imprimir / Guardar PDF
          </button>

          <button className="finish-button" onClick={onCloseAndClean} disabled={isFinalized}>
            {isFinalized ? "✅ Venta finalizada" : "✅ Finalizar venta"}
          </button>
        </div>

        <div className="invoice-print-area">
          <div className="invoice-document">
            <h1>Factura</h1>
            <p className="invoice-subtitle">Factura N° {visualInvoiceNumber}</p>

            <div className="invoice-info-grid">
              <p><strong>Fecha:</strong> {invoiceDate}</p>
              <p><strong>Cliente:</strong> {customer?.firstName} {customer?.lastName}</p>
              <p><strong>Teléfono:</strong> {customer?.phone}</p>
              <p><strong>Dirección:</strong> {customer?.address}</p>
              <p><strong>Correo:</strong> {customer?.email}</p>
            </div>

            <table className="invoice-table">
              <thead>
                <tr>
                  <th className="number-cell">Id</th>
                  <th>Producto</th>
                  <th className="number-cell">Precio</th>
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
                  details.map((item) => (
                    <tr key={item.id}>
                      <td className="number-cell">{item.id}</td>
                      <td>{item.name}</td>
                      <td className="number-cell">${Number(item.price).toFixed(2)}</td>
                      <td className="number-cell">{item.quantity}</td>
                      <td className="number-cell">${Number(item.subtotal).toFixed(2)}</td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>

            <div className="invoice-totals-preview">
              <p><strong>Subtotal:</strong> ${subtotal.toFixed(2)}</p>
              <p><strong>IVA:</strong> ${iva.toFixed(2)}</p>
              <h2>Total: ${total.toFixed(2)}</h2>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}