import React from "react";
import ReactDOM from "react-dom/client";
import App from "./App.jsx";
import { installFrontendErrorReporting } from "./api/errorReporter";

import "./styles/global.css";
import "./styles/forms.css";
import "./styles/buttons.css";
import "./styles/layout.css";
import "./styles/tables.css";
import "./styles/modal.css";
import "./styles/modals.css";
import "./styles/alerts.css";
import "./styles/login.css";
import "./styles/dashboard.css";
import "./styles/error-logs.css";
import "./styles/theme.css";
import "./styles/print.css";
import "./styles/bonus.css";

import { AppAlertProvider } from "./components/AppAlert";

installFrontendErrorReporting();

ReactDOM.createRoot(document.getElementById("root")).render(
  <React.StrictMode>
    <AppAlertProvider>
      <App />
    </AppAlertProvider>
  </React.StrictMode>
);
