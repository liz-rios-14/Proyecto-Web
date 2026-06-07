using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesPoint.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CompleteErrorLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Detail",
                table: "error_logs",
                newName: "detail");

            migrationBuilder.AddColumn<string>(
                name: "exception_type",
                table: "error_logs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "http_method",
                table: "error_logs",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "path",
                table: "error_logs",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "user_id",
                table: "error_logs",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "exception_type",
                table: "error_logs");

            migrationBuilder.DropColumn(
                name: "http_method",
                table: "error_logs");

            migrationBuilder.DropColumn(
                name: "path",
                table: "error_logs");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "error_logs");

            migrationBuilder.RenameColumn(
                name: "detail",
                table: "error_logs",
                newName: "Detail");
        }
    }
}
