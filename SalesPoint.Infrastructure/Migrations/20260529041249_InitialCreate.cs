using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesPoint.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    first_name = table.Column<string>(type: "NVARCHAR2(80)", maxLength: 80, nullable: false),
                    last_name = table.Column<string>(type: "NVARCHAR2(80)", maxLength: 80, nullable: false),
                    phone = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    address = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    email = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "invoices",
                columns: table => new
                {
                    id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    customer_id = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    date = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    invoice_number = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoices", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    name = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: false),
                    price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    stock = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "invoice_details",
                columns: table => new
                {
                    id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    product_id = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    product_name = table.Column<string>(type: "NVARCHAR2(120)", maxLength: 120, nullable: false),
                    price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    quantity = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    InvoiceId = table.Column<int>(type: "NUMBER(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoice_details", x => x.id);
                    table.ForeignKey(
                        name: "FK_invoice_details_invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "invoices",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_invoice_details_InvoiceId",
                table: "invoice_details",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_invoice_number",
                table: "invoices",
                column: "invoice_number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropTable(
                name: "invoice_details");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "invoices");
        }
    }
}
