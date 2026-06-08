import { useEffect, useRef, useState } from "react";
import Pagination from "./Pagination";
import { getApiErrorMessage } from "../api/apiError";
import { useAppAlert } from "./AppAlert";

const isNumericColumn = (key) => {
  const normalizedKey = key.toLowerCase();

  return (
    normalizedKey.includes("id") ||
    normalizedKey.includes("price") ||
    normalizedKey.includes("stock") ||
    normalizedKey.includes("total") ||
    normalizedKey.includes("subtotal") ||
    normalizedKey.includes("quantity")
  );
};

export default function SearchModal({
  isOpen,
  title,
  onClose,
  onSelect,
  fetchData,
  columns,
  searchFields = [],
}) {
  const { showAlert } = useAppAlert();
  const [data, setData] = useState([]);
  const [selectedField, setSelectedField] = useState("");
  const [searchValue, setSearchValue] = useState("");
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [isLoading, setIsLoading] = useState(false);
  const [selectedIndex, setSelectedIndex] = useState(0);

  const inputRef = useRef(null);

  const availableFields =
    searchFields.length > 0
      ? searchFields
      : columns.map((column) => ({
          key: column.key,
          label: column.label,
          type: column.type || "text",
        }));

  const currentField =
    availableFields.find((field) => field.key === selectedField) ||
    availableFields[0];

  const loadData = async (field, value, currentPage) => {
    try {
      setIsLoading(true);

      const result = await fetchData(field, value.trim(), currentPage);

      const items = Array.isArray(result) ? result : result.items ?? [];

      setData(items);
      setSelectedIndex(0);

      setTotalPages(
        !Array.isArray(result) && result.totalPages && result.totalPages > 0
          ? result.totalPages
          : 1
      );
    } catch (error) {
      console.error("Error al cargar datos:", error);
      setData([]);
      setTotalPages(1);
      setSelectedIndex(0);
      showAlert(
        getApiErrorMessage(error, "No se pudieron cargar los resultados."),
        "error"
      );
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    if (!isOpen) return;

    const defaultField = availableFields[0]?.key || "";

    setSelectedField(defaultField);
    setSearchValue("");
    setPage(1);
    setSelectedIndex(0);

    loadData(defaultField, "", 1);

    setTimeout(() => {
      inputRef.current?.focus();
    }, 50);
  }, [isOpen]);

  useEffect(() => {
    if (!isOpen || !selectedField) return;

    const timer = setTimeout(() => {
      setPage(1);
      loadData(selectedField, searchValue, 1);
    }, 300);

    return () => clearTimeout(timer);
  }, [searchValue, selectedField]);

  useEffect(() => {
    if (!isOpen || !selectedField) return;

    loadData(selectedField, searchValue, page);
  }, [page]);

  useEffect(() => {
    if (!isOpen) return;

    const handleKeyboard = (event) => {
      if (event.key === "Escape") {
        event.preventDefault();
        onClose();
      }

      if (event.key === "ArrowDown") {
        event.preventDefault();

        setSelectedIndex((currentIndex) => {
          if (data.length === 0) return 0;
          return currentIndex >= data.length - 1 ? 0 : currentIndex + 1;
        });
      }

      if (event.key === "ArrowUp") {
        event.preventDefault();

        setSelectedIndex((currentIndex) => {
          if (data.length === 0) return 0;
          return currentIndex <= 0 ? data.length - 1 : currentIndex - 1;
        });
      }

      if (event.key === "Enter") {
        if (data.length === 0) return;

        event.preventDefault();
        onSelect(data[selectedIndex]);
      }

      if (event.key === "ArrowRight" && page < totalPages) {
        event.preventDefault();
        setPage((currentPage) => currentPage + 1);
      }

      if (event.key === "ArrowLeft" && page > 1) {
        event.preventDefault();
        setPage((currentPage) => currentPage - 1);
      }
    };

    window.addEventListener("keydown", handleKeyboard);

    return () => {
      window.removeEventListener("keydown", handleKeyboard);
    };
  }, [isOpen, data, selectedIndex, page, totalPages, onClose, onSelect]);

  if (!isOpen) return null;

  return (
    <div className="modal-overlay">
      <div className="modal" role="dialog" aria-modal="true">
        <div className="modal-header">
          <h3 className="modal-title">{title}</h3>

          <button className="close-icon" onClick={onClose}>
            ×
          </button>
        </div>

        <div className="modal-inputs">
          <select
            value={selectedField}
            onChange={(event) => {
              setSelectedField(event.target.value);
              setSearchValue("");
              setPage(1);
            }}
          >
            {availableFields.map((field) => (
              <option key={field.key} value={field.key}>
                Buscar por {field.label}
              </option>
            ))}
          </select>

          <input
            ref={inputRef}
            placeholder={`Ingrese ${currentField?.label?.toLowerCase() || "valor"}`}
            type={currentField?.type === "number" ? "number" : "text"}
            min={currentField?.type === "number" ? "1" : undefined}
            value={searchValue}
            onChange={(event) => setSearchValue(event.target.value)}
          />

          <button onClick={() => loadData(selectedField, searchValue, 1)}>
            🔍 Buscar
          </button>
        </div>

        <div className="table-wrapper modal-table-wrapper">
          <table className="modal-table">
          <thead>
            <tr>
              {columns.map((column) => (
                <th
                  key={column.key}
                  className={isNumericColumn(column.key) ? "number-column" : ""}
                >
                  {column.label}
                </th>
              ))}
            </tr>
          </thead>

          <tbody>
            {isLoading ? (
              <tr>
                <td colSpan={columns.length}>Cargando datos...</td>
              </tr>
            ) : data.length === 0 ? (
              <tr>
                <td colSpan={columns.length}>Sin resultados</td>
              </tr>
            ) : (
              data.map((item, index) => (
                <tr
                  key={item.id}
                  tabIndex="0"
                  className={index === selectedIndex ? "selected-row" : ""}
                  onClick={() => onSelect(item)}
                  onMouseEnter={() => setSelectedIndex(index)}
                >
                  {columns.map((column) => (
                    <td
                      key={column.key}
                      className={
                        isNumericColumn(column.key) ? "number-column" : ""
                      }
                    >
                      {column.type === "money"
                        ? `$${Number(item[column.key]).toFixed(2)}`
                        : item[column.key]}
                    </td>
                  ))}
                </tr>
              ))
            )}
          </tbody>
          </table>
        </div>

        <Pagination
          page={page}
          totalPages={totalPages}
          onPrevious={() => setPage(page - 1)}
          onNext={() => setPage(page + 1)}
          onPageChange={(newPage) => setPage(newPage)}
        />
      </div>
    </div>
  );
}
