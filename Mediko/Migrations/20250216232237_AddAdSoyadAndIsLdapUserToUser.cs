using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mediko.Migrations
{
    /// <inheritdoc />
    public partial class AddAdSoyadAndIsLdapUserToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdSoyad",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLdapUser",
                table: "AspNetUsers",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdSoyad",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsLdapUser",
                table: "AspNetUsers");
        }
    }
}
