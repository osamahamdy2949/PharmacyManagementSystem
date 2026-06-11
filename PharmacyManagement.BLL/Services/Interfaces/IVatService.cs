namespace PharmacyManagement.BLL.Services.Interfaces;

public interface IVatService
{
    decimal Rate { get; }
    (decimal SubTotal, decimal VatAmount, decimal Total) Calculate(decimal subTotal);
    decimal LineTotal(decimal unitPrice, int quantity, decimal discount = 0);
}
