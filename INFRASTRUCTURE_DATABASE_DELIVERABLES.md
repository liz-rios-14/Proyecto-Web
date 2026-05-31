# Entregables Infrastructure + Base de Datos

Rama objetivo: `feature/infrastructure-database`

## Implementado

- `AppDbContext` actualizado con DbSet para ventas, usuarios, roles, movimientos de stock, logs y métodos de pago.
- Configuraciones EF Core agregadas:
  - `SaleConfiguration`
  - `SaleDetailConfiguration`
  - `UserConfiguration`
  - `RoleConfiguration`
  - `StockMovementConfiguration`
  - `ErrorLogConfiguration`
  - `PaymentMethodConfiguration`
- Relaciones configuradas:
  - Role 1:N User
  - Customer 1:N Sale
  - User 1:N Sale
  - PaymentMethod 1:N Sale
  - Sale 1:N SaleDetail
  - Product 1:N SaleDetail
  - Product 1:N StockMovement
  - Sale 1:N StockMovement
- Repositorios agregados:
  - `UserRepository`
  - `RoleRepository`
  - `StockMovementRepository`
  - `ErrorLogRepository`
- Interfaces agregadas para los repositorios anteriores.
- `UnitOfWork` agregado para transacciones.
- Confirmación de venta/factura protegida con transacción en `InvoiceRepository`.
- Registro automático de movimiento de stock cuando se confirma una factura/venta.
- Seed inicial:
  - Rol `ADMINISTRATOR`
  - Rol `SELLER`
  - Usuario `admin`
  - Método de pago `CASH`
- Migración manual agregada:
  - `20260531173000_AddInfrastructureDatabase.cs`
- Seeder masivo SQL agregado:
  - `Scripts/seed-massive-data.sql`

## Comandos recomendados

Desde la raíz del proyecto:

```bash
dotnet restore
dotnet ef database update --project SalesPoint.Infrastructure --startup-project SalesPoint.Api
```

Para ejecutar el seeder masivo en PostgreSQL:

```bash
psql -d salespoint -f Scripts/seed-massive-data.sql
```

## Nota importante

El proyecto original usa `Invoice/InvoiceDetail` para ventas en los controladores actuales. Para no romper el frontend ni los endpoints existentes, se mantuvo esa estructura y además se agregó el modelo nuevo `Sale/SaleDetail` solicitado para infraestructura.
