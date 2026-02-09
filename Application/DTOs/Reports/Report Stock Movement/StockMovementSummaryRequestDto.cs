using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports.Report_Stock_Movement
{
    public class StockMovementSummaryRequestDto
    {
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }

        // optAcend.Value = True → Descending, False → Ascending
        public bool SortDescending { get; set; }
    }
}
