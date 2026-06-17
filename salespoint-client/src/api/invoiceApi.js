import { api } from "./apiClient";

export const getInvoices = async (pageNumber = 1, pageSize = 8) => {
  const response = await api.get("/invoices", {
    params: {
      pageNumber,
      pageSize,
    },
  });

  return response.data;
};

export const getInvoiceById = async (id) => {
  const response = await api.get(`/invoices/${id}`);
  return response.data;
};

export const reconstructInvoiceByNumber = async (
  invoiceNumber,
  { validateForSale = false, validateEvenRules = false } = {}
) => {
  const response = await api.get(`/invoices/audit/${invoiceNumber}`, {
    params: { validateForSale, validateEvenRules },
  });
  return response.data;
};

export const getAuditInvoiceHistory = async (pageNumber = 1, pageSize = 10) => {
  const response = await api.get("/invoices/audit-history", {
    params: {
      pageNumber,
      pageSize,
    },
  });

  return response.data;
};
