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

export const reconstructInvoiceByNumber = async (invoiceNumber) => {
  const response = await api.get(`/invoices/audit/${invoiceNumber}`);
  return response.data;
};