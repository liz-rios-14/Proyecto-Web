import { createContext, useContext, useEffect, useRef, useState } from "react";

const AlertContext = createContext(null);

export function AppAlertProvider({ children }) {
  const [alertData, setAlertData] = useState(null);
  const [confirmation, setConfirmation] = useState(null);
  const alertTimerRef = useRef(null);

  const showAlert = (message, type = "warning") => {
    if (alertTimerRef.current) {
      clearTimeout(alertTimerRef.current);
    }

    setAlertData({ message, type });

    alertTimerRef.current = setTimeout(() => {
      setAlertData(null);
    }, 3500);
  };

  const showConfirm = (message, options = {}) =>
    new Promise((resolve) => {
      setConfirmation({
        message,
        title: options.title || "Confirmar acción",
        confirmText: options.confirmText || "Confirmar",
        cancelText: options.cancelText || "Cancelar",
        resolve,
      });
    });

  const closeConfirmation = (result) => {
    confirmation?.resolve(result);
    setConfirmation(null);
  };

  useEffect(() => {
    const originalAlert = window.alert;

    window.alert = (message) => {
      showAlert(message, "warning");
    };

    return () => {
      window.alert = originalAlert;

      if (alertTimerRef.current) {
        clearTimeout(alertTimerRef.current);
      }
    };
  }, []);

  const alertTitle =
    alertData?.type === "success"
      ? "Correcto"
      : alertData?.type === "error"
        ? "Error"
        : "Aviso";

  const alertIcon =
    alertData?.type === "success"
      ? "OK"
      : alertData?.type === "error"
        ? "!"
        : "i";

  return (
    <AlertContext.Provider value={{ showAlert, showConfirm }}>
      {children}

      {alertData && (
        <div
          className={`app-alert app-alert-${alertData.type}`}
          role="alert"
          aria-live="assertive"
        >
          <div className="app-alert-icon">{alertIcon}</div>

          <div>
            <strong>{alertTitle}</strong>
            <p>{alertData.message}</p>
          </div>

          <button
            type="button"
            aria-label="Cerrar mensaje"
            onClick={() => setAlertData(null)}
          >
            X
          </button>
        </div>
      )}

      {confirmation && (
        <div
          className="app-confirm-overlay"
          role="dialog"
          aria-modal="true"
          aria-labelledby="app-confirm-title"
        >
          <div className="app-confirm">
            <div className="app-confirm-icon">?</div>

            <div>
              <h2 id="app-confirm-title">{confirmation.title}</h2>
              <p>{confirmation.message}</p>
            </div>

            <div className="app-confirm-actions">
              <button
                type="button"
                className="secondary-button"
                onClick={() => closeConfirmation(false)}
              >
                {confirmation.cancelText}
              </button>

              <button
                type="button"
                className="delete-button"
                autoFocus
                onClick={() => closeConfirmation(true)}
              >
                {confirmation.confirmText}
              </button>
            </div>
          </div>
        </div>
      )}
    </AlertContext.Provider>
  );
}

export function useAppAlert() {
  return useContext(AlertContext);
}
