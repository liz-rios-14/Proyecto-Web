using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesPoint.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditInvoiceHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_invoice_histories",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    original_invoice_number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    generated_invoice_id = table.Column<int>(type: "int", nullable: false),
                    generated_invoice_number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    generated_by_user_id = table.Column<int>(type: "int", nullable: false),
                    total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    generated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_invoice_histories", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_invoice_histories_generated_invoice_number",
                table: "audit_invoice_histories",
                column: "generated_invoice_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_audit_invoice_histories_original_invoice_number",
                table: "audit_invoice_histories",
                column: "original_invoice_number");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_invoice_histories");
        }
    }
}
