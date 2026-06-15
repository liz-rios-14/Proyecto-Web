import { useEffect, useState } from "react";
import { api } from "../api/apiClient";
import { getApiErrorMessage } from "../api/apiError";
import { useAppAlert } from "./AppAlert";
import { isValidEcuadorianCedula } from "../utils/ecuadorianCedula";
import {
  normalizeSpaces,
  sanitizeDigits,
  sanitizeEmail,
  sanitizePersonNames,
  sanitizeSingleSpacedText,
} from "../utils/inputSanitizers";

const emptyForm = {
  id: "",
  firstName: "",
  lastName: "",
  cedula: "",
  phone: "",
  address: "",
  email: "",
};

export default function CreateCustomerModal({ isOpen, onClose, onLoad }) {
  const { showAlert } = useAppAlert();
  const [form, setForm] = useState(emptyForm);
  const [loadedCustomer, setLoadedCustomer] = useState(null);
  const [isSaving, setIsSaving] = useState(false);
  const [isLoadingById, setIsLoadingById] = useState(false);

  useEffect(() => {
    if (!isOpen) return;

    setForm(emptyForm);
    setLoadedCustomer(null);
  }, [isOpen]);

  if (!isOpen) return null;

  const updateField = (field, value) => {
    let cleanValue = value;

    if (field === "firstName" || field === "lastName") {
      cleanValue = sanitizePersonNames(value, 40);
    }

    if (field === "id") {
      cleanValue = sanitizeDigits(value, 9);
      setLoadedCustomer(null);
    }

    if (field === "cedula") {
      cleanValue = sanitizeDigits(value, 10);
      setLoadedCustomer(null);
    }

    if (field === "phone") {
      cleanValue = sanitizeDigits(value, 10);
    }

    if (field === "address") {
      cleanValue = sanitizeSingleSpacedText(value, 150);
    }

    if (field === "email") {
      cleanValue = sanitizeEmail(value);
      setLoadedCustomer(null);
    }

    setForm((current) => ({
      ...current,
      [field]: cleanValue,
    }));
  };

  const fillFromCustomer = (customer) => {
    setLoadedCustomer(customer);
    setForm({
      id: String(customer.id ?? ""),
      firstName: customer.firstName ?? "",
      lastName: customer.lastName ?? "",
      cedula: customer.cedula ?? "",
      phone: customer.phone ?? "",
      address: customer.address ?? "",
      email: customer.email ?? "",
    });
  };

  const loadById = async () => {
    if (!form.id || Number(form.id) <= 0) {
      showAlert("Ingrese un ID valido para cargar el cliente.", "warning");
      return null;
    }

    try {
      setIsLoadingById(true);
      const response = await api.get(`/customers/${Number(form.id)}`);
      fillFromCustomer(response.data);
      showAlert("Cliente cargado correctamente.", "success");
      return response.data;
    } catch (error) {
      setLoadedCustomer(null);
      showAlert(
        getApiErrorMessage(error, "No se pudo cargar el cliente por ID."),
        "error"
      );
      return null;
    } finally {
      setIsLoadingById(false);
    }
  };

  const createCustomer = async () => {
    const request = {
      firstName: normalizeSpaces(form.firstName),
      lastName: normalizeSpaces(form.lastName),
      cedula: form.cedula.trim(),
      phone: form.phone.trim(),
      address: normalizeSpaces(form.address),
      email: form.email.trim(),
    };

    if (
      !request.firstName ||
      !request.lastName ||
      !request.cedula ||
      !request.phone ||
      !request.address ||
      !request.email
    ) {
      showAlert("Complete todos los datos del cliente antes de guardar.", "warning");
      return;
    }

    if (!/^[\p{L}]+(?: [\p{L}]+)*$/u.test(request.firstName)) {
      showAlert("Los nombres solo deben contener letras y un espacio entre palabras.", "warning");
      return;
    }

    if (!/^[\p{L}]+(?: [\p{L}]+)*$/u.test(request.lastName)) {
      showAlert("Los apellidos solo deben contener letras y un espacio entre palabras.", "warning");
      return;
    }

    if (!isValidEcuadorianCedula(request.cedula)) {
      showAlert("Ingrese una cedula ecuatoriana valida.", "warning");
      return;
    }

    if (!/^09\d{8}$/.test(request.phone)) {
      showAlert("Ingrese un numero de telefono valido.", "warning");
      return;
    }

    if (request.address.length < 5) {
      showAlert("La direccion del cliente debe tener al menos 5 caracteres.", "warning");
      return;
    }

    if (!/^[\p{L}0-9 .,#/-]+$/u.test(request.address)) {
      showAlert("La direccion contiene caracteres no permitidos.", "warning");
      return;
    }

    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(request.email)) {
      showAlert("El correo del cliente no tiene un formato valido.", "warning");
      return;
    }

    try {
      setIsSaving(true);

      const [cedulaResponse, emailResponse] = await Promise.all([
        api.get("/customers/search", {
          params: {
            field: "cedula",
            value: request.cedula,
            pageNumber: 1,
            pageSize: 1,
            onlyActive: false,
          },
        }),
        api.get("/customers/search", {
          params: {
            field: "email",
            value: request.email,
            pageNumber: 1,
            pageSize: 1,
            onlyActive: false,
          },
        }),
      ]);

      if ((cedulaResponse.data?.items ?? []).length > 0) {
        showAlert("Ya existe un cliente registrado con esta cedula.", "warning");
        return;
      }

      if ((emailResponse.data?.items ?? []).length > 0) {
        showAlert("Ya existe un cliente con el mismo correo.", "warning");
        return;
      }

      const response = await api.post("/customers", request);
      fillFromCustomer(response.data);
      showAlert(
        `Cliente ${response.data.id} creado correctamente. Presione Cargar cliente para usarlo en la factura.`,
        "success"
      );
    } catch (error) {
      setLoadedCustomer(null);
      showAlert(
        getApiErrorMessage(error, "No se pudo crear el cliente."),
        "error"
      );
    } finally {
      setIsSaving(false);
    }
  };

  const loadCustomer = async () => {
    if (loadedCustomer) {
      onLoad(loadedCustomer);
      onClose();
      return;
    }

    if (form.id) {
      const customer = await loadById();

      if (customer) {
        onLoad(customer);
        onClose();
      }

      return;
    }

    showAlert("Cree el cliente o carguelo por ID antes de continuar.", "warning");
  };

  return (
    <div className="modal-overlay">
      <div className="modal" role="dialog" aria-modal="true">
        <div className="modal-header">
          <h3 className="modal-title">Crear cliente para factura</h3>

          <button className="close-icon" onClick={onClose}>
            x
          </button>
        </div>

        <div className="form-grid">
          <input
            value={form.id}
            placeholder="ID"
            inputMode="numeric"
            disabled
            onChange={(event) => updateField("id", event.target.value)}
          />

          <input
            value={form.firstName}
            placeholder="Nombres"
            disabled={Boolean(loadedCustomer)}
            onChange={(event) => updateField("firstName", event.target.value)}
          />

          <input
            value={form.lastName}
            placeholder="Apellidos"
            disabled={Boolean(loadedCustomer)}
            onChange={(event) => updateField("lastName", event.target.value)}
          />

          <input
            value={form.cedula}
            placeholder="Cedula ecuatoriana"
            inputMode="numeric"
            maxLength="10"
            onChange={(event) => updateField("cedula", event.target.value)}
          />

          <input
            value={form.phone}
            placeholder="Telefono"
            inputMode="numeric"
            maxLength="10"
            disabled={Boolean(loadedCustomer)}
            onChange={(event) => updateField("phone", event.target.value)}
          />

          <input
            value={form.address}
            placeholder="Direccion"
            disabled={Boolean(loadedCustomer)}
            onChange={(event) => updateField("address", event.target.value)}
          />

          <input
            value={form.email}
            placeholder="Correo"
            disabled={Boolean(loadedCustomer)}
            onKeyDown={(event) => {
              if (event.key === " ") event.preventDefault();
            }}
            onChange={(event) => updateField("email", event.target.value)}
          />
        </div>

        <div className="actions-row" style={{ marginTop: "16px" }}>
          <button
            type="button"
            className="secondary-button"
            onClick={loadById}
            disabled
          >
            {isLoadingById ? "Cargando..." : "Buscar ID"}
          </button>

          <button
            type="button"
            onClick={createCustomer}
            disabled={isSaving || isLoadingById || Boolean(loadedCustomer)}
          >
            {isSaving ? "Guardando..." : "Guardar cliente"}
          </button>

          <button
            type="button"
            className="invoice-button"
            onClick={loadCustomer}
            disabled={isSaving || isLoadingById}
          >
            Cargar cliente
          </button>
        </div>
      </div>
    </div>
  );
}
