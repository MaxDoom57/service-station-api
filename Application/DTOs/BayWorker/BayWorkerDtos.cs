namespace Application.DTOs.BayWorker
{
    public class CreateBayWorkerDto
    {
        public int BayKy { get; set; }
        public int UsrKy { get; set; }
        public string? Remarks { get; set; }
    }

    public class UpdateBayWorkerDto
    {
        public int BayWorkerKy { get; set; }
        public int BayKy { get; set; }
        public int UsrKy { get; set; }
        public string? Remarks { get; set; }
    }

    public class BayWorkerDto
    {
        public int BayWorkerKy { get; set; }
        public int BayKy { get; set; }
        public string BayNm { get; set; }
        public int UsrKy { get; set; }
        public string UsrNm { get; set; }
        public string? Remarks { get; set; }
        public bool IsActive { get; set; }
    }
}
