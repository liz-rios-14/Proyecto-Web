using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SalesPoint.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialSqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    first_name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    last_name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    address = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    email = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "error_logs",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    source = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Detail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    stack_trace = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_error_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "invoices",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    customer_id = table.Column<int>(type: "int", nullable: false),
                    date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    invoice_number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    customer_name_snapshot = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    customer_email_snapshot = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    customer_phone_snapshot = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    customer_address_snapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    seller_id = table.Column<int>(type: "int", nullable: false),
                    seller_username_snapshot = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    seller_full_name_snapshot = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    seller_role_snapshot = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoices", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payment_methods",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_methods", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    stock = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    description = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "invoice_details",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    product_id = table.Column<int>(type: "int", nullable: false),
                    product_name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    InvoiceId = table.Column<int>(type: "int", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    role_id = table.Column<int>(type: "int", nullable: false),
                    full_name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    user_name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    email = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    password_reset_token_hash = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    password_reset_token_expires_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_users_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "password_histories",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_password_histories", x => x.id);
                    table.ForeignKey(
                        name: "FK_password_histories_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sales",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    customer_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    payment_method_id = table.Column<int>(type: "int", nullable: false),
                    date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    sale_number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    is_confirmed = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sales", x => x.id);
                    table.ForeignKey(
                        name: "FK_sales_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sales_payment_methods_payment_method_id",
                        column: x => x.payment_method_id,
                        principalTable: "payment_methods",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sales_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "sale_details",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    sale_id = table.Column<int>(type: "int", nullable: false),
                    product_id = table.Column<int>(type: "int", nullable: false),
                    product_name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sale_details", x => x.id);
                    table.ForeignKey(
                        name: "FK_sale_details_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sale_details_sales_sale_id",
                        column: x => x.sale_id,
                        principalTable: "sales",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stock_movements",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    product_id = table.Column<int>(type: "int", nullable: false),
                    sale_id = table.Column<int>(type: "int", nullable: true),
                    invoice_id = table.Column<int>(type: "int", nullable: true),
                    movement_type = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    stock_after = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    reason = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_movements", x => x.id);
                    table.ForeignKey(
                        name: "FK_stock_movements_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stock_movements_sales_sale_id",
                        column: x => x.sale_id,
                        principalTable: "sales",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "payment_methods",
                columns: new[] { "id", "created_at", "description", "is_active", "is_deleted", "name", "UpdatedAt" },
                values: new object[] { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "PAGO EN EFECTIVO", true, false, "CASH", null });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id", "created_at", "description", "is_active", "is_deleted", "name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Administrador del sistema", true, false, "ADMINISTRATOR", null },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Vendedor del sistema", true, false, "SELLER", null }
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "created_at", "email", "full_name", "is_active", "is_deleted", "password_hash", "password_reset_token_expires_at", "password_reset_token_hash", "role_id", "UpdatedAt", "user_name" },
                values: new object[] { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@salespoint.local", "ADMINISTRADOR DEL SISTEMA", true, false, "CHANGE_ME_HASH_ADMIN_123456", null, null, 1, null, "admin" });

            migrationBuilder.CreateIndex(
                name: "IX_error_logs_created_at",
                table: "error_logs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_invoice_details_InvoiceId",
                table: "invoice_details",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_invoice_number",
                table: "invoices",
                column: "invoice_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_password_histories_user_id",
                table: "password_histories",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_payment_methods_name",
                table: "payment_methods",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_roles_name",
                table: "roles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sale_details_product_id",
                table: "sale_details",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_sale_details_sale_id",
                table: "sale_details",
                column: "sale_id");

            migrationBuilder.CreateIndex(
                name: "IX_sales_customer_id",
                table: "sales",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_sales_payment_method_id",
                table: "sales",
                column: "payment_method_id");

            migrationBuilder.CreateIndex(
                name: "IX_sales_sale_number",
                table: "sales",
                column: "sale_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sales_user_id",
                table: "sales",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_stock_movements_created_at",
                table: "stock_movements",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_stock_movements_product_id",
                table: "stock_movements",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_stock_movements_sale_id",
                table: "stock_movements",
                column: "sale_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_role_id",
                table: "users",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_user_name",
                table: "users",
                column: "user_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "error_logs");

            migrationBuilder.DropTable(
                name: "invoice_details");

            migrationBuilder.DropTable(
                name: "password_histories");

            migrationBuilder.DropTable(
                name: "sale_details");

            migrationBuilder.DropTable(
                name: "stock_movements");

            migrationBuilder.DropTable(
                name: "invoices");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "sales");

            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropTable(
                name: "payment_methods");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "roles");
        }
    }
}
