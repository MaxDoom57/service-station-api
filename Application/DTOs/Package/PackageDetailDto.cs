namespace Application.DTOs.Package
{
    public class PackageItemDto
    {
        public int ItmKy { get; set; }
        public string ItmCd { get; set; }
        public string? ItmNm { get; set; }
        public string? Time { get; set; }
        public decimal? SlsPri { get; set; }
    }

    public class PackageDetailDto : PackageDto
    {
        public List<PackageItemDto> Items { get; set; } = new List<PackageItemDto>();
    }
}
