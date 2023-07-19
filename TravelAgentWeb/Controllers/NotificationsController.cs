using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TravelAgentWeb.Data;
using TravelAgentWeb.Dtos;

namespace TravelAgentWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly TravelAgentDbContext _context;

        public NotificationsController(TravelAgentDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public ActionResult FlightChanged(FlightDetailUpdateDto flightDetailUpdateDto)
        {
            Console.WriteLine($"Webhook recebido de {flightDetailUpdateDto.Publisher}...");

            var secretModel = _context.SubscriptionSecrets
                .FirstOrDefault(ss => ss.Publisher == flightDetailUpdateDto.Publisher
                                && ss.Secret == flightDetailUpdateDto.Secret);

            if (secretModel is null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Palavra-chave inválida, Webhook inválido!");
                Console.ResetColor();

                return Ok();
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Webhook válido!");
            Console.WriteLine($"Preço anterior: R$ {flightDetailUpdateDto.OldPrice}\n Preço novo: R$ {flightDetailUpdateDto.NewPrice}");
            Console.ResetColor();

            return Ok();
        }
    }
}