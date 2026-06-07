export function getApiErrorMessage(error, fallbackMessage) {
  const data = error?.response?.data;

  if (typeof data === "string" && data.trim()) {
    return data;
  }

  if (typeof data?.message === "string" && data.message.trim()) {
    return data.message;
  }

  if (data?.errors && typeof data.errors === "object") {
    const validationMessage = Object.values(data.errors)
      .flat()
      .find((message) => typeof message === "string" && message.trim());

    if (validationMessage) {
      return validationMessage;
    }
  }

  if (!error?.response) {
    return "No se pudo conectar con el servidor.";
  }

  return fallbackMessage;
}
