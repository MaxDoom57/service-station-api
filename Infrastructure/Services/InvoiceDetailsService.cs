using Application.DTOs.Invoice;

namespace Infrastructure.Services
{
    public class InvoiceDetailsService
    {
        private readonly CustomerAccountService _customerAccountService;
        private readonly ItemService _itemService;
        private readonly PaymentTermService _paymentTermService;
        private readonly SalesAccountService _salesAccountService;

        public InvoiceDetailsService(
            CustomerAccountService customerAccountService,
            ItemService itemService,
            PaymentTermService paymentTermService,
            SalesAccountService salesAccountService)
        {
            _customerAccountService = customerAccountService;
            _itemService = itemService;
            _paymentTermService = paymentTermService;
            _salesAccountService = salesAccountService;
        }

        public async Task<InvoiceDetailsDto> GetDetailsAsync()
        {
            var customer = await _customerAccountService.GetAllAsync();
            var items = await _itemService.GetAllItemsAsync();
            var pmt = await _paymentTermService.GetAllAsync();
            var sales = await _salesAccountService.GetAllAsync();

            return new InvoiceDetailsDto
            {
                CustomerAccount = customer,
                Items = items,
                PaymentTerm = pmt,
                SalesAccount = sales
            };
        }
    }
}
