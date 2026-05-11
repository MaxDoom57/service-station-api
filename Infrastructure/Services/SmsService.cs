using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class SmsService : ISmsService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _baseUrl;
        private readonly string _apiToken;
        private readonly string _senderId;

        public string SmsCompanyName { get; }

        public SmsService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _baseUrl = configuration["SmsGateway:BaseUrl"] ?? "";
            _apiToken = configuration["SmsGateway:ApiToken"] ?? "";
            _senderId = configuration["SmsGateway:SenderId"] ?? "";
            SmsCompanyName = configuration["SmsGateway:SmsCompanyName"] ?? "HATCS";
        }

        public Task SendAsync(string phoneNumber, string message)
        {
            // Run entirely on a background thread to prevent any synchronous blocking
            return Task.Run(async () =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(phoneNumber))
                    {
                        return;
                    }

                    // Normalize phone number to Text.lk format (94XXXXXXXXX)
                    var normalized = NormalizePhone(phoneNumber);
                    if (normalized == null)
                    {
                        return;
                    }

                    var client = _httpClientFactory.CreateClient("SmsClient");

                    var payload = new
                    {
                        api_token = _apiToken,
                        recipient = normalized,
                        sender_id = _senderId,
                        type = "plain",
                        message = message
                    };

                    var jsonOptions = new JsonSerializerOptions
                    {
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    };
                    var json = JsonSerializer.Serialize(payload, jsonOptions);

                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = await client.PostAsync(_baseUrl, content);

                    if (!response.IsSuccessStatusCode)
                    {
                        // Handle failure silently
                    }
                }
                catch (Exception)
                {
                    // Fail silently - SMS errors should never interrupt the main flow
                }
            });
        }

        private string? NormalizePhone(string phone)
        {
            // Strip ALL whitespace characters
            phone = System.Text.RegularExpressions.Regex.Replace(phone, @"\s", "");

            // Remove any + prefix
            phone = phone.TrimStart('+');

            // Must contain digits only
            if (!System.Text.RegularExpressions.Regex.IsMatch(phone, @"^\d+$"))
            {
                return null;
            }

            // If starts with 0, convert to 94 prefix (Sri Lanka)
            // 0XXXXXXXXX -> 94XXXXXXXXX
            if (phone.Length == 10 && phone.StartsWith("0"))
            {
                return "94" + phone.Substring(1);
            }

            // If already in international format (94XXXXXXXXX)
            if (phone.Length == 11 && phone.StartsWith("94"))
            {
                return phone;
            }

            return null;
        }
    }
}
