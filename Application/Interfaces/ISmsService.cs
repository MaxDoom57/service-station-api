namespace Application.Interfaces
{
    public interface ISmsService
    {
        string SmsCompanyName { get; }
        Task SendAsync(string phoneNumber, string message);
    }
}
