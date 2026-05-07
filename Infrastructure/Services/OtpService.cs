using Application.DTOs.Reservation;
using Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class OtpService
    {
        private readonly IMemoryCache _cache;
        private readonly ISmsService _smsService;
        private readonly ReservationService _reservationService;

        private const int OtpExpiryMinutes = 5;
        private const int MaxResendCount = 3;
        private const int MaxFailedAttempts = 5;

        public OtpService(IMemoryCache cache, ISmsService smsService, ReservationService reservationService)
        {
            _cache = cache;
            _smsService = smsService;
            _reservationService = reservationService;
        }

        // STEP 1: Validate phone, generate OTP, cache session, send SMS
        public async Task<(bool success, string message, string? sessionId)> InitAsync(CreateFullReservationDto dto)
        {
            // Determine phone number
            string? phone = dto.Tp1;
            if (string.IsNullOrWhiteSpace(phone) && dto.NewVehicleDetails?.Owner != null)
            {
                phone = dto.NewVehicleDetails.Owner.TP1;
                dto.Tp1 = phone; // Ensure it's set for the database save later
            }

            // Validate Tp1 is provided
            if (string.IsNullOrWhiteSpace(phone))
                return (false, "Mobile number is required.", null);

            // Validate format: exactly 10 digits starting with 0
            var cleanPhone = Regex.Replace(phone.Trim(), @"\s", "");
            if (!Regex.IsMatch(cleanPhone, @"^\d{10}$") || !cleanPhone.StartsWith("0"))
                return (false, "Invalid mobile number. Must be 10 digits starting with 0 (e.g. 0771234567).", null);

            // Generate session and OTP
            var sessionId = Guid.NewGuid().ToString();
            var otp = GenerateOtp();

            // Cache the session
            var session = new OtpSession
            {
                PhoneNumber = cleanPhone,
                Otp = otp,
                ExpiresAt = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes),
                ResendCount = 1,
                FailedAttempts = 0,
                PendingDto = dto
            };

            _cache.Set(sessionId, session, TimeSpan.FromMinutes(OtpExpiryMinutes));

            // Send OTP via SMS
            var smsMessage = $"HAT Corporate Solutions (Pvt)Ltd.\n\nYour Reservation Code: {otp}\n\nDo not share this with anyone.";
            _ = _smsService.SendAsync(cleanPhone, smsMessage);

            // LOG OTP FOR DEVELOPMENT/TESTING
            Console.WriteLine("=========================");
            Console.WriteLine($"Your Reservation Code: {otp}");
            Console.WriteLine("=========================");

            return (true, "OTP sent successfully. Please verify within 5 minutes.", sessionId);
        }

        // STEP 2: Verify OTP and create reservation
        public async Task<(bool success, string message, int resKy)> ConfirmAsync(OtpConfirmDto dto)
        {
            if (!_cache.TryGetValue<OtpSession>(dto.SessionId, out var session))
                return (false, "Session not found or expired. Please request a new OTP.", 0);

            // Check if expired
            if (DateTime.UtcNow > session.ExpiresAt)
            {
                _cache.Remove(dto.SessionId);
                return (false, "OTP has expired. Please request a new one.", 0);
            }

            // Check too many wrong attempts
            if (session.FailedAttempts >= MaxFailedAttempts)
            {
                _cache.Remove(dto.SessionId);
                return (false, "Too many failed attempts. Session invalidated. Please start again.", 0);
            }

            // Validate OTP
            if (dto.Otp != session.Otp)
            {
                session.FailedAttempts++;
                _cache.Set(dto.SessionId, session, session.ExpiresAt - DateTime.UtcNow);
                int remaining = MaxFailedAttempts - session.FailedAttempts;
                return (false, $"Invalid OTP. {remaining} attempt(s) remaining.", 0);
            }

            // OTP valid — invalidate session immediately (prevent reuse)
            _cache.Remove(dto.SessionId);

            // Proceed to create reservation
            var result = await _reservationService.CreateReservationAsync(session.PendingDto);
            return (result.success, result.message, result.resKy);
        }

        // STEP 3: Resend OTP for an existing session
        public async Task<(bool success, string message)> ResendAsync(OtpResendDto dto)
        {
            if (!_cache.TryGetValue<OtpSession>(dto.SessionId, out var session))
                return (false, "Session not found or expired. Please request a new OTP.");

            if (session.ResendCount >= MaxResendCount)
                return (false, $"Maximum OTP resend limit ({MaxResendCount}) reached. Please start a new reservation request.");

            // Generate new OTP and reset expiry
            session.Otp = GenerateOtp();
            session.ExpiresAt = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes);
            session.ResendCount++;
            session.FailedAttempts = 0; // Reset failed attempts on resend

            _cache.Set(dto.SessionId, session, TimeSpan.FromMinutes(OtpExpiryMinutes));

            var smsMessage = $"HAT Corporate Solutions (Pvt)Ltd.\n\nYour Reservation Code: {session.Otp}\n\nDo not share this with anyone.";
            _ = _smsService.SendAsync(session.PhoneNumber, smsMessage);

            // LOG OTP FOR DEVELOPMENT/TESTING
            Console.WriteLine("=========================");
            Console.WriteLine($"Your Reservation Code: {session.Otp}");
            Console.WriteLine("=========================");

            return (true, $"OTP resent successfully. Attempt {session.ResendCount} of {MaxResendCount}.");
        }

        private static string GenerateOtp()
        {
            // Cryptographically random 6-digit OTP
            var bytes = new byte[4];
            System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
            var value = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 900000 + 100000; // 100000–999999
            return value.ToString();
        }
    }

    // Internal session model (only lives in memory cache, never persisted)
    public class OtpSession
    {
        public string PhoneNumber { get; set; }
        public string Otp { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int ResendCount { get; set; }
        public int FailedAttempts { get; set; }
        public CreateFullReservationDto PendingDto { get; set; }
    }
}
