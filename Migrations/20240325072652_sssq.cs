using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace projectUsers.Migrations
{
    /// <inheritdoc />
    public partial class sssq : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserPhone",
                table: "Users",
                newName: "Phone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "Users",
                newName: "UserPhone");
        }
    }
}
