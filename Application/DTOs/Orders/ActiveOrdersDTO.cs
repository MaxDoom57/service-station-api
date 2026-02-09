using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Orders
{
    public class ActiveOrdersDTO
    {
        public int OrderNo { get; set; }

        public string VehicleNo { get; set; } = "";

        public bool fBay { get; set; }

        public bool fQuality { get; set; }

        public bool fInvoice { get; set; }

        public bool fDispatch { get; set; }
    }
}
