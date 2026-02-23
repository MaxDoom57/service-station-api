using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Non_Active_Customer_List
{
    public class NonPerformedCustomerRequestDto
    {
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
    }
}
