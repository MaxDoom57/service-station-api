using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class UsrObj
    {
        [Key]
        public int UsrObjKy { get; set; }
        public int UsrKy { get; set; }
        public int ObjKy { get; set; }
        public byte fApr { get; set; }
        public short CKy { get; set; }
        public bool fAcs { get; set; }
        public bool fNew { get; set; }
        public bool fUpdt { get; set; }
        public bool fDel { get; set; }
        public bool fSp { get; set; }
        public int? EntUsrKy { get; set; }
        public DateTime? EntDtm { get; set; }
    }

}
