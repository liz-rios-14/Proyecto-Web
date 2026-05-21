export default function Pagination({
  page,
  totalPages,
  onPrevious,
  onNext,
  onPageChange,
}) {
  if (!totalPages || totalPages <= 1) return null;

  const getVisiblePages = () => {
    const visiblePages = [];

    if (totalPages <= 7) {
      for (let index = 1; index <= totalPages; index++) {
        visiblePages.push(index);
      }

      return visiblePages;
    }

    visiblePages.push(1);

    if (page > 4) {
      visiblePages.push("left-dots");
    }

    const startPage = Math.max(2, page - 1);
    const endPage = Math.min(totalPages - 1, page + 1);

    for (let index = startPage; index <= endPage; index++) {
      visiblePages.push(index);
    }

    if (page < totalPages - 3) {
      visiblePages.push("right-dots");
    }

    visiblePages.push(totalPages);

    return visiblePages;
  };

  const visiblePages = getVisiblePages();

  return (
    <div className="pagination-container">
      <button
        className="pagination-btn"
        disabled={page === 1}
        onClick={onPrevious}
      >
        ← Anterior
      </button>

      <div className="pagination-pages">
        {visiblePages.map((pageNumber) =>
          typeof pageNumber === "string" ? (
            <span key={pageNumber} className="pagination-dots">
              ...
            </span>
          ) : (
            <button
              key={pageNumber}
              className={`pagination-page ${
                pageNumber === page ? "active-page" : ""
              }`}
              onClick={() => onPageChange(pageNumber)}
            >
              {pageNumber}
            </button>
          )
        )}
      </div>

      <button
        className="pagination-btn"
        disabled={page === totalPages}
        onClick={onNext}
      >
        Siguiente →
      </button>
    </div>
  );
}