using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmacyManagement.DAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Medicines_QuantityInStock",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "QuantityInStock",
                table: "Medicines");

            migrationBuilder.AddColumn<string>(
                name: "BatchNumber",
                table: "PurchaseInvoiceItems",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryDate",
                table: "PurchaseInvoiceItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Barcode",
                table: "Medicines",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Medicines",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "StrengthUnit",
                table: "Medicines",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "StrengthValue",
                table: "Medicines",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "MedicineBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedicineId = table.Column<int>(type: "int", nullable: false),
                    BatchNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PurchasePrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    SellingPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CurrentQuantity = table.Column<int>(type: "int", nullable: false),
                    CraetedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicineBatches", x => x.Id);
                    table.CheckConstraint("CK_MedicineBatches_CurrentQuantity", "[CurrentQuantity] >= 0");
                    table.ForeignKey(
                        name: "FK_MedicineBatches_Medicines_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Medicines_Barcode",
                table: "Medicines",
                column: "Barcode",
                unique: true,
                filter: "[Barcode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MedicineBatches_MedicineId",
                table: "MedicineBatches",
                column: "MedicineId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MedicineBatches");

            migrationBuilder.DropIndex(
                name: "IX_Medicines_Barcode",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "BatchNumber",
                table: "PurchaseInvoiceItems");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "PurchaseInvoiceItems");

            migrationBuilder.DropColumn(
                name: "Barcode",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "StrengthUnit",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "StrengthValue",
                table: "Medicines");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryDate",
                table: "Medicines",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "QuantityInStock",
                table: "Medicines",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Medicines_QuantityInStock",
                table: "Medicines",
                sql: "[QuantityInStock] >= 0");
        }
    }
}
