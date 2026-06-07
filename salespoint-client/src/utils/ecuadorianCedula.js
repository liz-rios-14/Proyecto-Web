export function isValidEcuadorianCedula(value) {
  const cedula = String(value ?? "").trim();

  if (!/^\d{10}$/.test(cedula)) return false;

  const province = Number(cedula.slice(0, 2));
  const digits = [...cedula].map(Number);

  if (province < 1 || province > 24 || digits[2] >= 6) return false;

  let sum = 0;

  for (let index = 0; index < 9; index += 1) {
    let digit = digits[index];

    if (index % 2 === 0) {
      digit *= 2;
      if (digit > 9) digit -= 9;
    }

    sum += digit;
  }

  return (10 - (sum % 10)) % 10 === digits[9];
}
