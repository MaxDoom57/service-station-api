using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Invoice
{
    public class InvoiceDetailsDto
    {
        public object CustomerAccount { get; set; }
        public object Items { get; set; }
        public object PaymentTerm { get; set; }
        public object SalesAccount { get; set; }
    }
}
