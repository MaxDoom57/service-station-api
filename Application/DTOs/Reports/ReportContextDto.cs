using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Reports
{
    public class ReportContextDto
    {
        public string CompanyName { get; set; } = string.Empty;
        public string CurrentUserId { get; set; } = string.Empty;
        public string ReportTitle { get; set; } = string.Empty;
    }
}
