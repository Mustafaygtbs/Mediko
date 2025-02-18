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
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Doctors");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Doctors",
                newName: "Name");

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "Policlinics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Policlinics_DepartmentId",
                table: "Policlinics",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Policlinics_Departments_DepartmentId",
                table: "Policlinics",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Policlinics_Departments_DepartmentId",
                table: "Policlinics");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Policlinics_DepartmentId",
                table: "Policlinics");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Policlinics");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Doctors",
                newName: "LastName");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Doctors",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
