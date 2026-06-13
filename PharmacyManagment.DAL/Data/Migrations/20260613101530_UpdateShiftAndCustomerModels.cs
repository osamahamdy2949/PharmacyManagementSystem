using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmacyManagement.DAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateShiftAndCustomerModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OpeningCash",
                table: "Shifts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "OpeningCash",
                table: "Shifts",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
