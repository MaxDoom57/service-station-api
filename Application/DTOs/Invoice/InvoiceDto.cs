using System;
using System.Collections.Generic;

namespace Application.DTOs.Invoice
{
    public class InvoiceDto
    {
        public string? DocNo { get; set; }
        public string? YurRef { get; set; }
        public required string AdrCd { get; set; }
        public required int AccKy { get; set; }
        public required string PmtTrm1 { get; set; }
        public string? PmtTrm2 { get; set; }
        public required decimal PmtTrm1Amt { get; set; }
        public decimal? PmtTrm2Amt { get; set; }
        public required decimal Amt { get; set; }
        public decimal DisAmt { get; set; }

        public List<InvoiceItemDto> Items { get; set; } = new();
    }

    public class InvoiceItemDto
    {
        public required int ItmKy { get; set; }
        public required float Qty { get; set; }
        public decimal CosPri { get; set; }
        public decimal SlsPri { get; set; }
        public required decimal TrnPri { get; set; }
    }
}
