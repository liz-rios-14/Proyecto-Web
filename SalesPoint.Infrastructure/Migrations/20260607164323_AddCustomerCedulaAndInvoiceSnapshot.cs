using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesPoint.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerCedulaAndInvoiceSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "customer_cedula_snapshot",
                table: "invoices",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "cedula",
                table: "customers",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_customers_cedula",
                table: "customers",
                column: "cedula",
                unique: true,
                filter: "[cedula] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_customers_cedula",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "customer_cedula_snapshot",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "cedula",
                table: "customers");
        }
    }
}
