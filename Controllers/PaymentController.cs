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
        var paymentIntent = new PaymentIntentCreateOptions()
        {
            Amount = 1000, // Monto en centavos (2000 = $20.00 MXN)
            Currency = "mxn", // Moneda en MXN
            PaymentMethod = paymentRequestDto.PaymentId,
            ConfirmationMethod = "manual",
            Confirm = true,
            CaptureMethod = "manual",
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true,
                AllowRedirects = "never" // Deshabilitar métodos que requieran redirección
            }
        };

        var paymetIntentService = new PaymentIntentService();
        try
        {
            PaymentIntent payment = await paymetIntentService.CreateAsync(paymentIntent);

            

            // Verificar si el pago fue exitoso
            if (payment.Status == "succeeded")
            {
                response.Status = true;
                response.PaymentDetail = "Pago procesado con exito";
                response.PaymentMessage = $"Numero transaccion {payment.Id}";
                Console.WriteLine(payment.Description);
                return Ok(response);
                
            }else if (payment.Status == "requires_action" || payment.Status == "requires_source_action")
            {
                // Si el pago requiere autenticación adicional, pero no se redirige
                response.Status = false;
                response.PaymentDetail = "El pago no pudo ser procesado debido a autenticación adicional requerida.";
                response.PaymentMessage = $"Numero transaccion {payment.Id}";

                Console.WriteLine(payment.Description);
                return StatusCode(statusCode:200, response);
            }else
            {
                response.Status = false;
                response.PaymentDetail = "El pago no pudo ser realizado";
                response.PaymentMessage = $"Numero transaccion {payment.Id}";

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
    
    [HttpGet]
    public async Task<bool> AuthorizeCardAsync(string cardNumber, string expMonth, string expYear, string cvc, decimal amount)
    {
        StripeConfiguration.ApiKey = "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"; // Clave secreta
        try
        {
            // Crear un token para la tarjeta
            var tokenOptions = new TokenCreateOptions
            {
                Card = new TokenCardOptions
                {
                    Number = cardNumber,
                    ExpMonth = expMonth,
                    ExpYear = expYear,
                    Cvc = cvc
                }
            };

            var tokenService = new TokenService();
            Token token = await tokenService.CreateAsync(tokenOptions);

            // Crear un cargo de autorización
            var chargeOptions = new ChargeCreateOptions
            {
                Amount = (long)(amount * 100), // Stripe usa centavos
                Currency = "usd",
                Source = token.Id,
                Capture = false // Sólo autoriza, no captura el dinero
            };

            var chargeService = new ChargeService();
            Charge charge = await chargeService.CreateAsync(chargeOptions);

            return charge.Status == "succeeded"; // La transacción fue exitosa
        }
        catch (StripeException ex)
        {
            Console.WriteLine($"Stripe Error: {ex.Message}");
            return false;
        }
    }
}
