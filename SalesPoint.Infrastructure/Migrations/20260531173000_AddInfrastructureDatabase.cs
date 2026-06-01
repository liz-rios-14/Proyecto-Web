using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SalesPoint.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInfrastructureDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "error_logs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    source = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    stack_trace = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_error_logs", x => x.id));

            migrationBuilder.CreateTable(
                name: "payment_methods",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_payment_methods", x => x.id));

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_roles", x => x.id));

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_id = table.Column<int>(type: "integer", nullable: false),
                    full_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    user_name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    email = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
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
                name: "sales",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customer_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: true),
                    payment_method_id = table.Column<int>(type: "integer", nullable: false),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    sale_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_confirmed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sales", x => x.id);
                    table.ForeignKey("FK_sales_customers_customer_id", x => x.customer_id, "customers", "id", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_sales_payment_methods_payment_method_id", x => x.payment_method_id, "payment_methods", "id", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_sales_users_user_id", x => x.user_id, "users", "id", onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "sale_details",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sale_id = table.Column<int>(type: "integer", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    product_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sale_details", x => x.id);
                    table.ForeignKey("FK_sale_details_products_product_id", x => x.product_id, "products", "id", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_sale_details_sales_sale_id", x => x.sale_id, "sales", "id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stock_movements",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    sale_id = table.Column<int>(type: "integer", nullable: true),
                    invoice_id = table.Column<int>(type: "integer", nullable: true),
                    movement_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    stock_after = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    reason = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_movements", x => x.id);
                    table.ForeignKey("FK_stock_movements_products_product_id", x => x.product_id, "products", "id", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_stock_movements_sales_sale_id", x => x.sale_id, "sales", "id", onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData("roles", new[] { "id", "name" }, new object[,] { { 1, "ADMINISTRATOR" }, { 2, "SELLER" } });
            migrationBuilder.InsertData("payment_methods", new[] { "id", "name", "is_active" }, new object[] { 1, "CASH", true });
            migrationBuilder.InsertData("users", new[] { "id", "role_id", "full_name", "user_name", "email", "password_hash", "is_active" }, new object[] { 1, 1, "ADMINISTRADOR DEL SISTEMA", "admin", "admin@salespoint.local", "CHANGE_ME_HASH_ADMIN_123456", true });

            migrationBuilder.CreateIndex("IX_error_logs_created_at", "error_logs", "created_at");
            migrationBuilder.CreateIndex("IX_payment_methods_name", "payment_methods", "name", unique: true);
            migrationBuilder.CreateIndex("IX_roles_name", "roles", "name", unique: true);
            migrationBuilder.CreateIndex("IX_users_email", "users", "email", unique: true);
            migrationBuilder.CreateIndex("IX_users_role_id", "users", "role_id");
            migrationBuilder.CreateIndex("IX_users_user_name", "users", "user_name", unique: true);
            migrationBuilder.CreateIndex("IX_sales_customer_id", "sales", "customer_id");
            migrationBuilder.CreateIndex("IX_sales_payment_method_id", "sales", "payment_method_id");
            migrationBuilder.CreateIndex("IX_sales_sale_number", "sales", "sale_number", unique: true);
            migrationBuilder.CreateIndex("IX_sales_user_id", "sales", "user_id");
            migrationBuilder.CreateIndex("IX_sale_details_product_id", "sale_details", "product_id");
            migrationBuilder.CreateIndex("IX_sale_details_sale_id", "sale_details", "sale_id");
            migrationBuilder.CreateIndex("IX_stock_movements_created_at", "stock_movements", "created_at");
            migrationBuilder.CreateIndex("IX_stock_movements_product_id", "stock_movements", "product_id");
            migrationBuilder.CreateIndex("IX_stock_movements_sale_id", "stock_movements", "sale_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("error_logs");
            migrationBuilder.DropTable("sale_details");
            migrationBuilder.DropTable("stock_movements");
            migrationBuilder.DropTable("sales");
            migrationBuilder.DropTable("payment_methods");
            migrationBuilder.DropTable("users");
            migrationBuilder.DropTable("roles");
        }
    }
}
