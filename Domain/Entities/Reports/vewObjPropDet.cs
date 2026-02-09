using System;

namespace Infrastructure.Context.Entities
{
    public class vewObjPropDet
    {
        public short CKy { get; set; }

        public string ObjTyp { get; set; } = string.Empty;

        public string PrntObj { get; set; } = string.Empty;

        public string ChildObj { get; set; } = string.Empty;

        public int ObjPropKy { get; set; }

        public int UsrKy { get; set; }

        public int ObjKy { get; set; }

        public bool? fHide { get; set; }

        public bool? fUpdt { get; set; }

        public bool? fNull { get; set; }

        public double? ObjVal { get; set; }

        public DateTime? ObjDt { get; set; }

        public string? ObjChar { get; set; }

        public string? ObjRem { get; set; }
    }
}
