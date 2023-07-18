using System;
using System.Linq;
using AirlineWeb.Data;
using AirlineWeb.Dtos;
using AirlineWeb.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace AirlineWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookSubscriptionsController : ControllerBase
    {
        private readonly AirlineDbContext _context;
        private readonly IMapper _mapper;

        public WebhookSubscriptionsController(AirlineDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }


        [HttpPost]
        public ActionResult<WebhookSubscriptionReadDto> CreateSubscription(WebhookSubscriptionCreateDto webhookSubscriptionCreateDto)
        {
             var webhookSubscription = _context.WebhookSubscriptions
                .FirstOrDefault(ws => ws.WebhookURI == webhookSubscriptionCreateDto.WebhookURI);

            if (webhookSubscription is null)
            {
                webhookSubscription = _mapper.Map<WebhookSubscription>(webhookSubscriptionCreateDto);

                webhookSubscription.Secret = Guid.NewGuid().ToString();
                webhookSubscription.WebhookPublisher = "LATAM Airlines";

                try
                {
                    _context.WebhookSubscriptions.Add(webhookSubscription);
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }

                var webhookSubscriptionReadDto = _mapper.Map<WebhookSubscriptionReadDto>(webhookSubscription);
            }

            return NoContent();
        }
    }
}
