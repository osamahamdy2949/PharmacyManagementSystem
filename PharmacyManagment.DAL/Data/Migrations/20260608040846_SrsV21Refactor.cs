using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmacyManagement.DAL.Data.Migrations
{
    /// <inheritdoc />
    public partial class SrsV21Refactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CraetedAt",
                table: "Suppliers",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "CraetedAt",
                table: "StockTransactions",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "SalesReturns",
                newName: "RefundAmount");

            migrationBuilder.RenameColumn(
                name: "CraetedAt",
                table: "SalesReturns",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "CraetedAt",
                table: "SalesReturnItems",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "CraetedAt",
                table: "SalesInvoices",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "CraetedAt",
                table: "SalesInvoiceItems",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "CraetedAt",
                table: "PurchaseInvoices",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "CraetedAt",
                table: "PurchaseInvoiceItems",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "CraetedAt",
                table: "Medicines",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "CraetedAt",
                table: "MedicineBatches",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "CraetedAt",
                table: "Customers",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "CraetedAt",
                table: "Categories",
                newName: "CreatedAt");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Suppliers",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Suppliers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Suppliers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TaxRegNumber",
                table: "Suppliers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Suppliers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "StockTransactions",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "StockTransactions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "StockTransactions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "StockTransactions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "StockTransactions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "StockTransactions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "StockTransactions",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SalesReturns",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "SalesReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "SalesReturns",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "SalesReturns",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "SalesReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SalesReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "SalesReturns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SalesReturnItems",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "SalesReturnItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "SalesReturnItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "SalesReturnItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SalesReturnItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MedicineBatchId",
                table: "SalesReturnItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "SalesReturnItems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "SalesReturnItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SalesInvoices",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "SalesInvoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "SalesInvoices",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "SalesInvoices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "SalesInvoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SalesInvoices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "SubTotal",
                table: "SalesInvoices",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "SalesInvoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VatAmount",
                table: "SalesInvoices",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SalesInvoiceItems",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "SalesInvoiceItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "SalesInvoiceItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "SalesInvoiceItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Discount",
                table: "SalesInvoiceItems",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SalesInvoiceItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MedicineBatchId",
                table: "SalesInvoiceItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "SalesInvoiceItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "PurchaseInvoices",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "PurchaseInvoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "PurchaseInvoices",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "PurchaseInvoices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "PurchaseInvoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "PurchaseInvoices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "SubTotal",
                table: "PurchaseInvoices",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "PurchaseInvoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VatAmount",
                table: "PurchaseInvoices",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "PurchaseInvoiceItems",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "PurchaseInvoiceItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "PurchaseInvoiceItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "PurchaseInvoiceItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "PurchaseInvoiceItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MedicineBatchId",
                table: "PurchaseInvoiceItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "PurchaseInvoiceItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Medicines",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Medicines",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Medicines",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Medicines",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Medicines",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MinStockLevel",
                table: "Medicines",
                type: "int",
                nullable: false,
                defaultValue: 10);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Medicines",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "MedicineBatches",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "MedicineBatches",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "MedicineBatches",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "MedicineBatches",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Dose",
                table: "MedicineBatches",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "MedicineBatches",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "MedicineBatches",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "Sku",
                table: "MedicineBatches",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "MedicineBatches",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Customers",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Customers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Customers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalSpent",
                table: "Customers",
                type: "decimal(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Categories",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Categories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Categories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Entity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: true),
                    OldValue = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ReferenceId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseReturns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseInvoiceId = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseReturns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseReturns_PurchaseInvoices_PurchaseInvoiceId",
                        column: x => x.PurchaseInvoiceId,
                        principalTable: "PurchaseInvoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseReturns_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseReturnItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseReturnId = table.Column<int>(type: "int", nullable: false),
                    MedicineBatchId = table.Column<int>(type: "int", nullable: false),
                    MedicineId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseReturnItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnItems_MedicineBatches_MedicineBatchId",
                        column: x => x.MedicineBatchId,
                        principalTable: "MedicineBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnItems_Medicines_MedicineId",
                        column: x => x.MedicineId,
                        principalTable: "Medicines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnItems_PurchaseReturns_PurchaseReturnId",
                        column: x => x.PurchaseReturnId,
                        principalTable: "PurchaseReturns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnItems_MedicineBatchId",
                table: "SalesReturnItems",
                column: "MedicineBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceItems_MedicineBatchId",
                table: "SalesInvoiceItems",
                column: "MedicineBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceItems_MedicineBatchId",
                table: "PurchaseInvoiceItems",
                column: "MedicineBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnItems_MedicineBatchId",
                table: "PurchaseReturnItems",
                column: "MedicineBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnItems_MedicineId",
                table: "PurchaseReturnItems",
                column: "MedicineId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnItems_PurchaseReturnId",
                table: "PurchaseReturnItems",
                column: "PurchaseReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturns_PurchaseInvoiceId",
                table: "PurchaseReturns",
                column: "PurchaseInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturns_SupplierId",
                table: "PurchaseReturns",
                column: "SupplierId");

            migrationBuilder.Sql("""
                UPDATE si
                SET si.MedicineBatchId = COALESCE(
                    (SELECT TOP 1 mb.Id FROM MedicineBatches mb WHERE mb.MedicineId = si.MedicineId ORDER BY mb.ExpiryDate DESC),
                    (SELECT TOP 1 mb.Id FROM MedicineBatches mb ORDER BY mb.Id)
                )
                FROM SalesInvoiceItems si
                WHERE si.MedicineBatchId = 0;
                DELETE FROM SalesInvoiceItems WHERE MedicineBatchId = 0 OR MedicineBatchId IS NULL;
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoiceItems_MedicineBatches_MedicineBatchId",
                table: "PurchaseInvoiceItems",
                column: "MedicineBatchId",
                principalTable: "MedicineBatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoiceItems_MedicineBatches_MedicineBatchId",
                table: "SalesInvoiceItems",
                column: "MedicineBatchId",
                principalTable: "MedicineBatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesReturnItems_MedicineBatches_MedicineBatchId",
                table: "SalesReturnItems",
                column: "MedicineBatchId",
                principalTable: "MedicineBatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoiceItems_MedicineBatches_MedicineBatchId",
                table: "PurchaseInvoiceItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoiceItems_MedicineBatches_MedicineBatchId",
                table: "SalesInvoiceItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesReturnItems_MedicineBatches_MedicineBatchId",
                table: "SalesReturnItems");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PurchaseReturnItems");

            migrationBuilder.DropTable(
                name: "PurchaseReturns");

            migrationBuilder.DropIndex(
                name: "IX_SalesReturnItems_MedicineBatchId",
                table: "SalesReturnItems");

            migrationBuilder.DropIndex(
                name: "IX_SalesInvoiceItems_MedicineBatchId",
                table: "SalesInvoiceItems");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseInvoiceItems_MedicineBatchId",
                table: "PurchaseInvoiceItems");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "TaxRegNumber",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "StockTransactions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "StockTransactions");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "StockTransactions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "StockTransactions");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "StockTransactions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "StockTransactions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "SalesReturns");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "SalesReturns");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SalesReturns");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "SalesReturns");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SalesReturns");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "SalesReturns");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "SalesReturnItems");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SalesReturnItems");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "SalesReturnItems");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SalesReturnItems");

            migrationBuilder.DropColumn(
                name: "MedicineBatchId",
                table: "SalesReturnItems");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "SalesReturnItems");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "SalesReturnItems");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "SubTotal",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "VatAmount",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "SalesInvoiceItems");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "SalesInvoiceItems");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "SalesInvoiceItems");

            migrationBuilder.DropColumn(
                name: "Discount",
                table: "SalesInvoiceItems");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SalesInvoiceItems");

            migrationBuilder.DropColumn(
                name: "MedicineBatchId",
                table: "SalesInvoiceItems");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "SalesInvoiceItems");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "PurchaseInvoices");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "PurchaseInvoices");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "PurchaseInvoices");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "PurchaseInvoices");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "PurchaseInvoices");

            migrationBuilder.DropColumn(
                name: "SubTotal",
                table: "PurchaseInvoices");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "PurchaseInvoices");

            migrationBuilder.DropColumn(
                name: "VatAmount",
                table: "PurchaseInvoices");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "PurchaseInvoiceItems");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "PurchaseInvoiceItems");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "PurchaseInvoiceItems");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "PurchaseInvoiceItems");

            migrationBuilder.DropColumn(
                name: "MedicineBatchId",
                table: "PurchaseInvoiceItems");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "PurchaseInvoiceItems");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "MinStockLevel",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Medicines");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "MedicineBatches");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "MedicineBatches");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "MedicineBatches");

            migrationBuilder.DropColumn(
                name: "Dose",
                table: "MedicineBatches");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "MedicineBatches");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "MedicineBatches");

            migrationBuilder.DropColumn(
                name: "Sku",
                table: "MedicineBatches");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "MedicineBatches");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "TotalSpent",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Categories");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Suppliers",
                newName: "CraetedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "StockTransactions",
                newName: "CraetedAt");

            migrationBuilder.RenameColumn(
                name: "RefundAmount",
                table: "SalesReturns",
                newName: "TotalAmount");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "SalesReturns",
                newName: "CraetedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "SalesReturnItems",
                newName: "CraetedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "SalesInvoices",
                newName: "CraetedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "SalesInvoiceItems",
                newName: "CraetedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "PurchaseInvoices",
                newName: "CraetedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "PurchaseInvoiceItems",
                newName: "CraetedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Medicines",
                newName: "CraetedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "MedicineBatches",
                newName: "CraetedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Customers",
                newName: "CraetedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Categories",
                newName: "CraetedAt");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Suppliers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "StockTransactions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SalesReturns",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SalesReturnItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SalesInvoices",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SalesInvoiceItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "PurchaseInvoices",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "PurchaseInvoiceItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Medicines",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "MedicineBatches",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Customers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Categories",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}
