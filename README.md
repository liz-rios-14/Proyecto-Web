# SalesPoint

Sistema de punto de venta con backend .NET, Clean Architecture, Entity Framework
Core, SQL Server y frontend React con Vite.

## Requisitos

- .NET SDK 10.
- Node.js 20 o superior.
- SQL Server 2022 o compatible.
- `dotnet-ef` disponible: `dotnet tool install --global dotnet-ef`.

## Base de datos

Configure la conexión sin guardar credenciales reales en Git. Puede crear
`SalesPoint.Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=salespoint_db;User Id=sa;Password=SU_CLAVE;TrustServerCertificate=True;Encrypt=False;"
  },
  "Jwt": {
    "Key": "CLAVE_LOCAL_LARGA_Y_SEGURA_DE_AL_MENOS_32_CARACTERES",
    "Issuer": "SalesPointApi",
    "Audience": "SalesPointClient"
  }
}
```

SQL Server local con Docker:

```bash
docker run --name salespoint-sql \
  -e ACCEPT_EULA=Y \
  -e MSSQL_SA_PASSWORD='ClaveLocal123!' \
  -p 1433:1433 \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

Crear o actualizar el esquema:

```bash
dotnet restore
dotnet build SalesPoint.slnx
dotnet ef database update \
  --project SalesPoint.Infrastructure/SalesPoint.Infrastructure.csproj \
  --startup-project SalesPoint.Api/SalesPoint.Api.csproj
```

La migración `AddRefreshTokensAndAuditLogs` crea únicamente
`refresh_tokens` y `audit_logs`; no elimina columnas ni datos existentes.

Si SQL Server se ejecuta en Windows y la aplicación en Linux/WSL, reemplace
`localhost` por la IP del host de Windows. Obtenga la IP con `ipconfig` y
habilite TCP/IP, el puerto 1433 y la regla correspondiente del firewall.

## Backend

```bash
dotnet restore
dotnet build SalesPoint.slnx
dotnet run --project SalesPoint.Api --urls http://localhost:5036
```

- API: `http://localhost:5036`
- Swagger: `http://localhost:5036/swagger`

## Frontend

Cree `salespoint-client/.env`:

```env
VITE_API_URL=http://localhost:5036/api
```

Luego ejecute:

```bash
cd salespoint-client
npm install
npm run dev -- --host 0.0.0.0 --port 5173
```

Frontend: `http://localhost:5173`.

## Funcionalidades opcionales

- Refresh tokens hasheados, rotación, expiración y revocación.
- Recuperación académica de contraseña sin SMTP y con respuesta genérica.
- Auditoría general de login y operaciones relevantes.
- Reportes por fecha, vendedor, productos, stock y clientes.
- Exportación CSV compatible con Excel y vista imprimible para guardar en PDF.
- Pruebas automatizadas de reglas críticas.

La autenticación externa queda pendiente porque requiere proveedores y
configuración de terceros.

## Pruebas

```bash
dotnet test SalesPoint.slnx
cd salespoint-client
npm run build
```

Las pruebas cubren producto duplicado, stock insuficiente, totales, cliente
inválido, política de contraseña, correo y desactivación lógica.

## Prueba rápida

1. Ejecute la migración y levante la API.
2. Use `POST /api/auth/login`; conserva `token` y añade `accessToken`,
   `refreshToken`, `expiration`, `user` y `role`.
3. Use `POST /api/auth/refresh` y `POST /api/auth/logout`.
4. Pruebe `POST /api/auth/forgot-password` y
   `POST /api/auth/reset-password`.
5. Autorice Swagger y consulte `GET /api/audit-logs`.
6. Consulte `GET /api/reports` y `GET /api/reports/export/excel`.

En el frontend, **Reportes** está disponible para ambos roles y **Auditoría**
solo para `ADMINISTRATOR`. Un `SELLER` recibe únicamente sus ventas.
