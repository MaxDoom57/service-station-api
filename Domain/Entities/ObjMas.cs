using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ObjMas
    {
        [Key]
        public int ObjKy { get; set; }
        public short ObjTypKy { get; set; }
        public int PrntObjKy { get; set; }
        public string ObjNm { get; set; } = null!;
        public bool? FInAct { get; set; }
        public string ObjTyp { get; set; } = null!;
        public string? ObjCd { get; set; }
        public string? ShortCap { get; set; }
        public string? ObjCap { get; set; }
        public int? AsoObjKy { get; set; }
        public bool FUsrAcs { get; set; }
        public bool FCCAcs { get; set; }
        public bool FDevAcs { get; set; }
        public bool FDefault { get; set; }
        public bool FCap { get; set; }
        public double? ObjVal { get; set; }
        public DateTime? ObjDt { get; set; }
        public string? ObjChar { get; set; }
        public bool? FHide { get; set; }
        public bool FAcs { get; set; }
        public bool FNew { get; set; }
        public bool FUpdt { get; set; }
        public bool FDel { get; set; }
        public bool FSp { get; set; }
        public bool? FNull { get; set; }
        public string? Status { get; set; }
        public short SKy { get; set; }
        public int? EntUsrKy { get; set; }
        public DateTime? EntDtm { get; set; }
        public byte? FApr { get; set; }
        public bool FObs { get; set; }
    }

}
