/* eslint-disable react-refresh/only-export-components */
import {
  createContext,
  useContext,
  useEffect,
  useState,
} from "react";
import { useNavigate } from "react-router-dom";
import { useAppAlert } from "./AppAlert";

const UnsavedChangesContext = createContext(null);

export function UnsavedChangesProvider({ children }) {
  const navigate = useNavigate();
  const { showConfirm } = useAppAlert();
  const [isDirty, setIsDirty] = useState(false);

  useEffect(() => {
    const handleBeforeUnload = (event) => {
      if (!isDirty) return;
      event.preventDefault();
      event.returnValue = "";
    };

    window.addEventListener("beforeunload", handleBeforeUnload);
    return () => window.removeEventListener("beforeunload", handleBeforeUnload);
  }, [isDirty]);

  const requestNavigation = async (to, options) => {
    if (isDirty) {
      const confirmed = await showConfirm(
        "Existen cambios sin guardar. Si sale de esta pantalla, se perderán.",
        {
          title: "Salir sin guardar",
          confirmText: "Sí, salir",
          cancelText: "No, permanecer",
        }
      );

      if (!confirmed) return false;
    }

    setIsDirty(false);
    navigate(to, options);
    return true;
  };

  return (
    <UnsavedChangesContext.Provider
      value={{ isDirty, setIsDirty, requestNavigation }}
    >
      {children}
    </UnsavedChangesContext.Provider>
  );
}

export function useUnsavedChanges(isDirty) {
  const context = useContext(UnsavedChangesContext);
  const setIsDirty = context?.setIsDirty;

  useEffect(() => {
    setIsDirty?.(Boolean(isDirty));
    return () => setIsDirty?.(false);
  }, [setIsDirty, isDirty]);

  return context;
}

export function useSafeNavigation() {
  return useContext(UnsavedChangesContext);
}
