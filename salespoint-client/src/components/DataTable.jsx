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

const formatValue = (value, column) => {
  if (value === null || value === undefined || value === "") {
    return "—";
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
        <thead>
          <tr>
            {columns.map((column) => (
              <th
                key={column.key}
                className={isNumericColumn(column.key) ? "number-cell" : ""}
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
                    className={isNumericColumn(column.key) ? "number-cell" : ""}
                  >
                    {formatValue(item[column.key], column)}
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