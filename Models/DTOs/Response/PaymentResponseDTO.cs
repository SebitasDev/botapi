namespace TelegramBotApi.Models.DTOs.Response;

public class PaymentResponseDTO
{
    public bool Status { get; set; }
    public string PaymentDetail { get; set; }
    public string PaymentMessage { get; set; }
}