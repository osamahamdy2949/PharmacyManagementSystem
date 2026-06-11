using AutoMapper;
using PharmacyManagement.BLL.ViewModels.CategoryViewModels;
using PharmacyManagement.BLL.ViewModels.CustomerViewModels;
using PharmacyManagement.BLL.ViewModels.InvoiceItemViewModels;
using PharmacyManagement.BLL.ViewModels.MedicineViewModels;
using PharmacyManagement.BLL.ViewModels.PosViewModels;
using PharmacyManagement.BLL.ViewModels.PurchaseInvoiceViewModels;
using PharmacyManagement.BLL.ViewModels.ReportViewModels;
using PharmacyManagement.BLL.ViewModels.SalesInvoiceViewModels;
using PharmacyManagement.BLL.ViewModels.SupplierViewModels;
using PharmacyManagement.DAL.Data.Entities;

namespace PharmacyManagement.BLL.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Category, CategoryViewModel>().ReverseMap();
        CreateMap<Medicine, MedicineViewModel>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category.Name))
            .ForMember(d => d.QuantityInStock, o => o.Ignore())
            .ForMember(d => d.IsNearExpiry, o => o.Ignore());
        CreateMap<MedicineViewModel, Medicine>();
        CreateMap<Medicine, MedicineSaleLookupViewModel>()
            .ForMember(d => d.QuantityInStock, o => o.Ignore());
        CreateMap<Supplier, SupplierViewModel>().ReverseMap();
        CreateMap<Customer, CustomerViewModel>().ReverseMap();
        CreateMap<PurchaseInvoice, PurchaseInvoiceViewModel>()
            .ForMember(d => d.SupplierName, o => o.MapFrom(s => s.Supplier.Name))
            .ForMember(d => d.Items, o => o.MapFrom(s => s.Items));
        CreateMap<PurchaseInvoiceItem, InvoiceItemViewModel>()
            .ForMember(d => d.MedicineName, o => o.MapFrom(s => s.Medicine.TradeName))
            .ForMember(d => d.SerialNumber, o => o.MapFrom(s => s.Medicine.SerialNumber))
            .ForMember(d => d.UnitLabel, o => o.MapFrom(s => s.Medicine.PurchaseUnit.ToString()))
            .ForMember(d => d.Subtotal, o => o.MapFrom(s => s.Quantity * s.UnitPrice));
        CreateMap<SalesInvoice, SalesInvoiceViewModel>()
            .ForMember(d => d.CustomerName, o => o.MapFrom(s => s.Customer.Name))
            .ForMember(d => d.DoctorName, o => o.MapFrom(s => s.DoctorName))
            .ForMember(d => d.SubTotal, o => o.MapFrom(s => s.SubTotal))
            .ForMember(d => d.VatAmount, o => o.MapFrom(s => s.VatAmount))
            .ForMember(d => d.Items, o => o.MapFrom(s => s.Items));
        CreateMap<SalesInvoiceItem, InvoiceItemViewModel>()
            .ForMember(d => d.MedicineName, o => o.MapFrom(s => s.Medicine.TradeName))
            .ForMember(d => d.SerialNumber, o => o.MapFrom(s => s.Medicine.SerialNumber))
            .ForMember(d => d.UnitLabel, o => o.MapFrom(s => s.Medicine.SaleUnit.ToString()))
            .ForMember(d => d.Subtotal, o => o.MapFrom(s => s.Quantity * s.UnitPrice));
        CreateMap<Medicine, InventoryReportItemViewModel>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category.Name))
            .ForMember(d => d.QuantityInStock, o => o.Ignore());
        CreateMap<Medicine, FinishedMedicineReportItemViewModel>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category.Name))
            .ForMember(d => d.PurchasePricePerPurchaseUnit, o => o.MapFrom(s => s.PurchasePrice));
        CreateMap<SalesInvoice, SalesReportItemViewModel>()
            .ForMember(d => d.InvoiceId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.CustomerName, o => o.MapFrom(s => s.Customer.Name));
    }
}
