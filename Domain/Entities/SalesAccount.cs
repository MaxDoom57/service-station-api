using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class SalesAccount
    {
        public short CKy { get; set; }
        public int AccKy { get; set; }
        public string? AccCd { get; set; }
        public string? AccNm { get; set; }

        // Needed for filtering
        public string? AccTyp { get; set; }
        public int AccTypKy { get; set; }
    }
}
