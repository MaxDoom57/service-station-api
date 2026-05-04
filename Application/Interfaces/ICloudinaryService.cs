using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ICloudinaryService
    {
        Task<(string Url, string PublicId)> UploadImageAsync(string base64Image, string folder);
        Task<bool> DeleteImageAsync(string publicId);
    }
}
