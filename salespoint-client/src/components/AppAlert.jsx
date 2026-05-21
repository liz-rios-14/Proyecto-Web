import { createContext, useContext, useEffect, useState } from "react";

const AlertContext = createContext(null);

export function AppAlertProvider({ children }) {
  const [alertData, setAlertData] = useState(null);

  const showAlert = (message, type = "warning") => {
    setAlertData({
      message,
      type,
    });

    setTimeout(() => {
      setAlertData(null);
    }, 3500);
  };

  useEffect(() => {
    const originalAlert = window.alert;

    window.alert = (message) => {
      showAlert(message, "warning");
    };

    return () => {
      window.alert = originalAlert;
    };
  }, []);

  return (
    <AlertContext.Provider value={{ showAlert }}>
      {children}

      {alertData && (
        <div className={`app-alert app-alert-${alertData.type}`}>
          <div className="app-alert-icon">
            {alertData.type === "success" ? "✅" : "⚠️"}
          </div>

          <div>
            <strong>
              {alertData.type === "success" ? "Correcto" : "Aviso"}
            </strong>
            <p>{alertData.message}</p>
          </div>

          <button onClick={() => setAlertData(null)}>×</button>
        </div>
      )}
    </AlertContext.Provider>
  );
}

export function useAppAlert() {
  return useContext(AlertContext);
}