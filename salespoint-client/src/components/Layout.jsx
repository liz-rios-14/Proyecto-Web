import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import Sidebar from "./Sidebar";

export default function Layout({ children }) {
  const navigate = useNavigate();

  useEffect(() => {
    const handleGlobalShortcuts = (event) => {
      if (!event.ctrlKey) return;

      switch (event.key) {
        case "1":
          event.preventDefault();
          navigate("/");
          break;

        case "2":
          event.preventDefault();
          navigate("/sales");
          break;

        case "3":
          event.preventDefault();
          navigate("/customers");
          break;

        case "4":
          event.preventDefault();
          navigate("/products");
          break;

        case "5":
          event.preventDefault();
          navigate("/invoices");
          break;

        default:
          break;
      }
    };

    window.addEventListener("keydown", handleGlobalShortcuts);

    return () => {
      window.removeEventListener("keydown", handleGlobalShortcuts);
    };
  }, [navigate]);

  return (
    <div className="app-layout">
      <Sidebar />

      <main className="main-content">
        <div className="container">{children}</div>
      </main>
    </div>
  );
}