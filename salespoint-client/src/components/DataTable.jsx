const isNumericColumn = (key) => {
  const normalizedKey = key.toLowerCase();

  return (
    normalizedKey.includes("id") ||
    normalizedKey.includes("price") ||
    normalizedKey.includes("stock") ||
    normalizedKey.includes("total") ||
    normalizedKey.includes("subtotal") ||
    normalizedKey.includes("quantity") ||
    normalizedKey.includes("amount")
  );
};

const getColumnClass = (column) => {
  const key = column.key.toLowerCase();

  if (key === "id") return "col-id";
  if (key.includes("status") || key.includes("estado")) return "col-status";
  if (key.includes("access") || key.includes("acceso")) return "col-access";
  if (key.includes("alert") || key.includes("aviso")) return "col-alert";
  if (key.includes("email") || key.includes("correo")) return "col-email";
  if (key.includes("name") || key.includes("nombre")) return "col-name";
  if (isNumericColumn(column.key)) return "col-number";

  return "";
};

const getColumnWidth = (column) => {
  const key = column.key.toLowerCase();

  if (key === "id") return "64px";
  if (key.includes("status") || key.includes("estado")) return "110px";
  if (key.includes("access") || key.includes("acceso")) return "130px";
  if (key.includes("alert") || key.includes("aviso")) return "150px";
  if (key.includes("email") || key.includes("correo")) return "230px";
  if (key.includes("name") || key.includes("nombre")) return "220px";
  if (isNumericColumn(column.key)) return "100px";

  return "160px";
};

const formatValue = (value, column) => {
  if (value === null || value === undefined || value === "") {
    return "-";
  }

  if (column.type === "money") {
    return `$${Number(value).toFixed(2)}`;
  }

  if (column.type === "number") {
    return Number(value);
  }

  return value;
};

export default function DataTable({
  columns,
  data,
  emptyMessage = "Sin registros",
  onRowClick,
  onRowDoubleClick,
  actions,
}) {
  return (
    <div className="table-wrapper">
      <table className="data-table">
        <colgroup>
          {columns.map((column) => (
            <col key={column.key} style={{ width: getColumnWidth(column) }} />
          ))}
          {actions && <col style={{ width: "280px" }} />}
        </colgroup>

        <thead>
          <tr>
            {columns.map((column) => (
              <th
                key={column.key}
                className={`${isNumericColumn(column.key) ? "number-cell" : ""} ${getColumnClass(column)}`}
              >
                {column.label}
              </th>
            ))}

            {actions && <th>Acciones</th>}
          </tr>
        </thead>

        <tbody>
          {data.length === 0 ? (
            <tr>
              <td colSpan={actions ? columns.length + 1 : columns.length}>
                {emptyMessage}
              </td>
            </tr>
          ) : (
            data.map((item) => (
              <tr
                key={item.id ?? item.invoiceNumber}
                tabIndex={onRowClick || onRowDoubleClick ? 0 : -1}
                onClick={onRowClick ? () => onRowClick(item) : undefined}
                onDoubleClick={
                  onRowDoubleClick ? () => onRowDoubleClick(item) : undefined
                }
                onKeyDown={(event) => {
                  if (event.key === "Enter") {
                    if (onRowDoubleClick) {
                      onRowDoubleClick(item);
                      return;
                    }

                    if (onRowClick) {
                      onRowClick(item);
                    }
                  }
                }}
                className={`${onRowClick || onRowDoubleClick ? "clickable-row" : ""} ${
                  item.isSelected ? "selected-row" : ""
                }`}
              >
                {columns.map((column) => (
                  <td
                    key={column.key}
                    className={`${isNumericColumn(column.key) ? "number-cell" : ""} ${getColumnClass(column)}`}
                  >
                    <span className="cell-content">
                      {formatValue(item[column.key], column)}
                    </span>
                  </td>
                ))}

                {actions && (
                  <td onClick={(event) => event.stopPropagation()}>
                    <div className="table-actions">{actions(item)}</div>
                  </td>
                )}
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  );
}
