using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Costify.Web.Migrations.Identity
{
    /// <inheritdoc />
    public partial class AddBusinessIdToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "AspNetUsers");
        }
    }
}
