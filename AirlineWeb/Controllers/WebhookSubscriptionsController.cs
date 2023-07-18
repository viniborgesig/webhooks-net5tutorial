using AirlineWeb.Data;
using AirlineWeb.Dtos;
using AirlineWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace AirlineWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookSubscriptionsController : ControllerBase
    {
        private readonly AirlineDbContext _context;

        public WebhookSubscriptionsController(AirlineDbContext context)
        {
            _context = context;
        }

        // public ActionResult<WebhookSubscriptionReadDto> CreateSubscription(WebhookSubscriptionCreateDto webhookSubscription)
        // {
            
        // }
    }
}
