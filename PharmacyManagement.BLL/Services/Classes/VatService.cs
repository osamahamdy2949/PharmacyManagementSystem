using Microsoft.Extensions.Options;
using PharmacyManagement.BLL.Common;
using PharmacyManagement.BLL.Services.Interfaces;

namespace PharmacyManagement.BLL.Services.Classes;

public class VatService : IVatService
{
    public VatService(IOptions<VatSettings> options) => Rate = options.Value.Rate;

    public decimal Rate { get; }

    public (decimal SubTotal, decimal VatAmount, decimal Total) Calculate(decimal subTotal)
    {
        var vat = Math.Round(subTotal * Rate, 2);
        return (subTotal, vat, subTotal + vat);
    }

    public decimal LineTotal(decimal unitPrice, int quantity, decimal discount = 0) =>
        Math.Max(0, unitPrice * quantity - discount);
}
