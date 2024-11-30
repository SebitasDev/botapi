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
        var options = new PaymentIntentCreateOptions
        {
            Amount = 1000, // Monto en centavos (2000 = $20.00 MXN)
            Currency = "mxn", // Moneda en MXN
            PaymentMethod = paymentRequestDto.PaymentId,
            Confirm = true, // Confirmamos el pago inmediatamente
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions()
            {
                Enabled = true, // Habilitar métodos de pago automáticos
                AllowRedirects = "never",  // No permitir redirección
            }
        };

        var service = new PaymentIntentService();
        try
        {
            PaymentIntent paymentIntent = await service.CreateAsync(options);

            

            // Verificar si el pago fue exitoso
            if (paymentIntent.Status == "succeeded")
            {
                response.Status = true;
                response.PaymentDetail = "Pago procesado con exito";
                response.PaymentMessage = $"Numero transaccion {paymentIntent.Id}";
                Console.WriteLine(paymentIntent.Description);
                return Ok(response);
                
            }else if (paymentIntent.Status == "requires_action" || paymentIntent.Status == "requires_source_action")
            {
                // Si el pago requiere autenticación adicional, pero no se redirige
                response.Status = false;
                response.PaymentDetail = "El pago no pudo ser procesado debido a autenticación adicional requerida.";
                response.PaymentMessage = $"Numero transaccion {paymentIntent.Id}";

                Console.WriteLine(paymentIntent.Description);
                return StatusCode(statusCode:200, response);
            }else
            {
                response.Status = false;
                response.PaymentDetail = "El pago no pudo ser realizado";
                response.PaymentMessage = $"Numero transaccion {paymentIntent.Id}";

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