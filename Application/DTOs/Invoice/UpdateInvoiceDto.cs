using System.Collections.Generic;

namespace Application.DTOs.Invoice
{
    public class UpdateInvoiceDto
    {
        public int TrnKy { get; set; }          // invoice key to update
        public string? DocNo { get; set; }
        public string? YurRef { get; set; }
        public int AccKy { get; set; }
        public string PmtTrm1 { get; set; }
        public string? PmtTrm2 { get; set; }
        public decimal PmtTrm1Amt { get; set; }
        public decimal? PmtTrm2Amt { get; set; }
        public decimal Amt { get; set; }
        public decimal DisAmt { get; set; }
        public string? Description { get; set; }

        public List<InvoiceItemDto> Items { get; set; }
    }
}
