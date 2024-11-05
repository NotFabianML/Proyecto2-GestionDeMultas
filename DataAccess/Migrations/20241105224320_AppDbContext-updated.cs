using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AppDbContextupdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Usuarios",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<decimal>(
                name: "Longitud",
                table: "Multas",
                type: "decimal(9,5)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitud",
                table: "Multas",
                type: "decimal(9,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,2)");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_UserId",
                table: "Usuarios",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_AspNetUsers_UserId",
                table: "Usuarios",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_AspNetUsers_UserId",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_UserId",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Usuarios");

            migrationBuilder.AlterColumn<decimal>(
                name: "Longitud",
                table: "Multas",
                type: "decimal(9,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,5)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitud",
                table: "Multas",
                type: "decimal(9,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,4)");
        }
    }
}
