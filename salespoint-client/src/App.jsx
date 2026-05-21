import { BrowserRouter, Routes, Route } from "react-router-dom";
import HomePage from "./pages/HomePage";
import CustomerManagerPage from "./pages/CustomerManagerPage";
import ProductManagerPage from "./pages/ProductManagerPage";
import SalesPointPage from "./pages/SalesPointPage";
import InvoiceHistoryPage from "./pages/InvoiceHistoryPage";

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/customers" element={<CustomerManagerPage />} />
        <Route path="/products" element={<ProductManagerPage />} />
        <Route path="/sales" element={<SalesPointPage />} />
        <Route path="/invoices" element={<InvoiceHistoryPage />} />
      </Routes>
    </BrowserRouter>
  );
}