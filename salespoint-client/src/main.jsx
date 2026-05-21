import React from "react";
import ReactDOM from "react-dom/client";
import App from "./App.jsx";

import "./styles/global.css";
import "./styles/modal.css";

import { AppAlertProvider } from "./components/AppAlert";

ReactDOM.createRoot(document.getElementById("root")).render(
  <React.StrictMode>
    <AppAlertProvider>
      <App />
    </AppAlertProvider>
  </React.StrictMode>
);