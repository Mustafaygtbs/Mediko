using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mediko.Migrations
{
    /// <inheritdoc />
    public partial class mig_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_OgrenciNo",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_TcKimlikNo",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "OgrenciNo",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "OgrenciNo",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_OgrenciNo",
                table: "AspNetUsers",
                column: "OgrenciNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_TcKimlikNo",
                table: "AspNetUsers",
                column: "TcKimlikNo",
                unique: true);
        }
    }
}
