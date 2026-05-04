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
        private readonly string _username;
        private readonly string _password;

        public SmsService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _baseUrl = configuration["SmsGateway:BaseUrl"];
            _username = configuration["SmsGateway:Username"];
            _password = configuration["SmsGateway:Password"];
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

                    // Normalize phone number to E.164 format (+94XXXXXXXXX)
                    var normalized = NormalizePhone(phoneNumber);
                    if (normalized == null)
                    {
                        return;
                    }

                    var client = _httpClientFactory.CreateClient("SmsClient");

                    // Basic Auth
                    var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_username}:{_password}"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

                    var payload = new
                    {
                        textMessage = new { text = message },
                        phoneNumbers = new[] { normalized }
                    };

                    var jsonOptions = new JsonSerializerOptions
                    {
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    };
                    var json = JsonSerializer.Serialize(payload, jsonOptions);
                    
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

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

            // Must contain digits only
            if (!System.Text.RegularExpressions.Regex.IsMatch(phone, @"^\d+$"))
            {
                return null;
            }

            // Must be exactly 10 digits starting with 0 (Sri Lanka local format)
            if (phone.Length != 10 || !phone.StartsWith("0"))
            {
                return null;
            }

            // Convert: 0XXXXXXXXX -> +94XXXXXXXXX
            return "+94" + phone.Substring(1);
        }
    }
}
