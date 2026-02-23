using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Stock_Addition
{
    public class StkAddFullResponseDTO
    {
        public StkAddHeaderDTO Header { get; set; }
        public List<StkAddDetailDTO> Details { get; set; }
    }

    public class StockAddUpdateDTO
    {
        public int trnNo { get; set; }
        public DateTime trnDate { get; set; }
        public short locKey { get; set; }
        public string? description { get; set; }
        public List<StockAddDetailInput> items { get; set; }
    }
}
