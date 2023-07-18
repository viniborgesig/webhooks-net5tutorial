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


        [HttpGet("{secret}", Name = "GetSubscriptionBySecret")]
        public ActionResult<WebhookSubscriptionReadDto> GetSubscriptionBySecret(string secret)
        {
            var webhookSubscription = _context.WebhookSubscriptions
                .FirstOrDefault(ws => ws.Secret == secret);

            if (webhookSubscription is null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<WebhookSubscriptionReadDto>(webhookSubscription));
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
                return CreatedAtRoute(nameof(GetSubscriptionBySecret), new { secret = webhookSubscriptionReadDto.Secret }, webhookSubscriptionReadDto);
            }

            return NoContent();
        }
    }
}
