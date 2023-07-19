using System;
using System.Linq;
using AirlineWeb.Data;
using AirlineWeb.Dtos;
using AirlineWeb.MessageBus;
using AirlineWeb.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace AirlineWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightsController : ControllerBase
    {
        private readonly AirlineDbContext _context;
        private readonly IMapper _mapper;
        private readonly IMessageBusClient _messageBusClient;

        public FlightsController(AirlineDbContext context, IMapper mapper, IMessageBusClient messageBusClient)
        {
            _context = context;
            _mapper = mapper;
            _messageBusClient = messageBusClient;
        }

        [HttpGet("{flightCode}", Name = "GetFlightDetailsByCode")]
        public ActionResult<FlightDetailReadDto> GetFlightDetailsByCode(string flightCode)
        {
            var flightDetail = _context.FlightDetails
                .FirstOrDefault(fd => fd.FlightCode == flightCode);

            if (flightDetail is null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<FlightDetailReadDto>(flightDetail));
        }

        [HttpPost]
        public ActionResult<FlightDetailReadDto> CreateFlight(FlightDetailCreateDto flightDetailCreateDto)
        {
            var flightDetail = _context.FlightDetails
                .FirstOrDefault(fd => fd.FlightCode == flightDetailCreateDto.FlightCode);

            if (flightDetail is null)
            {
                var flightDetailModel = _mapper.Map<FlightDetail>(flightDetailCreateDto);

                try
                {
                    _context.FlightDetails.Add(flightDetailModel);
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }

                var flightDetailReadDto = _mapper.Map<FlightDetailReadDto>(flightDetailModel);
                return CreatedAtRoute(nameof(GetFlightDetailsByCode), new { flightCode = flightDetailReadDto.FlightCode }, flightDetailReadDto);
            }

            return NoContent();
        }

        [HttpPut("{id}")]
        public ActionResult UpdateFlightDetail(int id, FlightDetailUpdateDto flightDetailUpdateDto)
        {
            var flightDetail = _context.FlightDetails
                .FirstOrDefault(fd => fd.Id == id);

            if (flightDetail is null)
            {
                return NotFound();
            }

            decimal oldPrice = flightDetail.Price;

            _mapper.Map(flightDetailUpdateDto, flightDetail);

            try
            {
                _context.SaveChanges();

                if (oldPrice == flightDetail.Price)
                {
                    Console.WriteLine("Sem alteração de preço.");
                    return NoContent();
                }

                Console.WriteLine("Alteração de preço, mensagem será colocada no RabbitmQ.");

                var notificationMessageDto = new NotificationMessageDto
                {
                    FlightCode = flightDetail.FlightCode,
                    WebhookType = "Alteração de preço",
                    OldPrice = oldPrice,
                    NewPrice = flightDetail.Price
                };

                _messageBusClient.SendMessage(notificationMessageDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return NoContent();
        }
    }
}