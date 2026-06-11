using PharmacyManagement.DAL.Data.Entities;

namespace PharmacyManagement.BLL.Common;

public static class UnitConversionHelper
{
    public static int PurchaseUnitsToSaleUnits(Medicine medicine, int purchaseQuantity) =>
        purchaseQuantity * Math.Max(1, medicine.UnitsPerPurchaseUnit);

    public static string FormatUnitLabel(Medicine medicine, int quantity, bool purchaseUnit) =>
        purchaseUnit
            ? $"{quantity} {medicine.PurchaseUnit}"
            : $"{quantity} {medicine.SaleUnit}";
}
