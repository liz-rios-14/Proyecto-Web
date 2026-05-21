import { Moon, Sun } from "lucide-react";
import { useEffect, useState } from "react";

export default function ThemeToggle() {
  const [isDarkMode, setIsDarkMode] = useState(() => {
    return localStorage.getItem("salespoint-theme") === "dark";
  });

  useEffect(() => {
    if (isDarkMode) {
      document.body.classList.add("dark-mode");
      localStorage.setItem("salespoint-theme", "dark");
    } else {
      document.body.classList.remove("dark-mode");
      localStorage.setItem("salespoint-theme", "light");
    }
  }, [isDarkMode]);

  return (
    <button
      className="theme-toggle-button"
      onClick={() => setIsDarkMode(!isDarkMode)}
      title={isDarkMode ? "Modo claro" : "Modo oscuro"}
    >
      {isDarkMode ? <Sun size={18} /> : <Moon size={18} />}

      <span>
        {isDarkMode ? "Modo claro" : "Modo oscuro"}
      </span>
    </button>
  );
}