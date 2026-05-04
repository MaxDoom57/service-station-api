namespace Application.DTOs.Reservation
{
    public class OtpInitDto
    {
        public CreateFullReservationDto ReservationDetails { get; set; }
    }

    public class OtpConfirmDto
    {
        public string SessionId { get; set; }
        public string Otp { get; set; }
    }

    public class OtpResendDto
    {
        public string SessionId { get; set; }
    }
}
