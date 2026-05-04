using System;

namespace Application.DTOs.Checklist
{
    public class ChecklistItemDto
    {
        public short CdKy { get; set; }
        public short CKy { get; set; }
        public string Code { get; set; }
        public string CdNm { get; set; }
    }

    public class SaveChecklistDto
    {
        public int OrdKy { get; set; }
        public List<ChecklistValueDto> ChecklistItems { get; set; }
    }

    public class ChecklistValueDto
    {
        public int CdKy { get; set; }
        public bool BitValue1 { get; set; }
        public float? Value { get; set; }
        public string? Remarks { get; set; }
    }

    public class CreateChecklistMasterDto
    {
        public string CdNm { get; set; }
    }
}
