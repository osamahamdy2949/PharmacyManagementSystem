using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmacyManagement.DAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class BatchActivationWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Barcode",
                table: "MedicineBatches",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "MedicineBatches",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MinStockLevel",
                table: "MedicineBatches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PurchaseUnit",
                table: "MedicineBatches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SaleUnit",
                table: "MedicineBatches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UnitsPerPurchaseUnit",
                table: "MedicineBatches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_Phone",
                table: "Suppliers",
                column: "Phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicineBatches_Barcode",
                table: "MedicineBatches",
                column: "Barcode",
                unique: true,
                filter: "[Barcode] IS NOT NULL");

            migrationBuilder.Sql("""
                UPDATE b SET
                    b.IsActive = 1,
                    b.PurchaseUnit = m.PurchaseUnit,
                    b.SaleUnit = m.SaleUnit,
                    b.UnitsPerPurchaseUnit = CASE WHEN m.UnitsPerPurchaseUnit < 1 THEN 1 ELSE m.UnitsPerPurchaseUnit END,
                    b.MinStockLevel = CASE WHEN m.MinStockLevel < 1 THEN 10 ELSE m.MinStockLevel END
                FROM MedicineBatches b
                INNER JOIN Medicines m ON m.Id = b.MedicineId
                WHERE b.CurrentQuantity > 0
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Suppliers_Phone",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_MedicineBatches_Barcode",
                table: "MedicineBatches");

            migrationBuilder.DropColumn(
                name: "Barcode",
                table: "MedicineBatches");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "MedicineBatches");

            migrationBuilder.DropColumn(
                name: "MinStockLevel",
                table: "MedicineBatches");

            migrationBuilder.DropColumn(
                name: "PurchaseUnit",
                table: "MedicineBatches");

            migrationBuilder.DropColumn(
                name: "SaleUnit",
                table: "MedicineBatches");

            migrationBuilder.DropColumn(
                name: "UnitsPerPurchaseUnit",
                table: "MedicineBatches");
        }
    }
}
