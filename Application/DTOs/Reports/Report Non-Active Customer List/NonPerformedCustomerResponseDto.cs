using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Non_Active_Customer_List
{
    public class NonPerformedCustomerResponseDto
    {
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }

        public List<NonPerformedCustomerRowDto> Rows { get; set; } = new();

        public ReportContextDto Context { get; set; } = new();
    }
}
