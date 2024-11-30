using Microsoft.AspNetCore.Mvc;
using Stripe;
using TelegramBotApi.Models.DTOs.Request;
using TelegramBotApi.Models.DTOs.Response;

namespace TelegramBotApi.Controllers;

[ApiController]
[Route("api/payment")]
public class PaymentController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<PaymentResponseDTO>> Payment(PaymentRequestDTO paymentRequestDto)
    {
        var response = new PaymentResponseDTO();
        // Configura la clave secreta de Stripe
        StripeConfiguration.ApiKey = "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"; // Clave secreta
        
        // Crear un PaymentIntent sin redirección
        var chargeOptions = new ChargeCreateOptions()
        {
            Amount = 1000, // Monto en centavos (2000 = $20.00 MXN)
            Currency = "mxn", // Moneda en MXN
            Source = paymentRequestDto.PaymentId,
            Capture = false
        };

        var chargeService = new ChargeService();
        try
        {
            Charge charge = await chargeService.CreateAsync(chargeOptions);

            

            // Verificar si el pago fue exitoso
            if (charge.Status == "succeeded")
            {
                response.Status = true;
                response.PaymentDetail = "Pago procesado con exito";
                response.PaymentMessage = $"Numero transaccion {charge.Id}";
                Console.WriteLine(charge.Description);
                return Ok(response);
                
            }else if (charge.Status == "requires_action" || charge.Status == "requires_source_action")
            {
                // Si el pago requiere autenticación adicional, pero no se redirige
                response.Status = false;
                response.PaymentDetail = "El pago no pudo ser procesado debido a autenticación adicional requerida.";
                response.PaymentMessage = $"Numero transaccion {charge.Id}";

                Console.WriteLine(charge.Description);
                return StatusCode(statusCode:200, response);
            }else
            {
                response.Status = false;
                response.PaymentDetail = "El pago no pudo ser realizado";
                response.PaymentMessage = $"Numero transaccion {charge.Id}";

                return StatusCode(statusCode:200, response);
            }
        }
        catch (StripeException e)
        {
            response.Status = false;
            response.PaymentDetail = "Error al procesar el pago";
            response.PaymentMessage = e.Message;
            
            return StatusCode(statusCode:200, response);;
        }
    }
}
