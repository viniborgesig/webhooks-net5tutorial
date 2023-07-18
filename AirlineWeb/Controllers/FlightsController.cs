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
    public class FlightsController : ControllerBase
    {
        private readonly AirlineDbContext _context;
        private readonly IMapper _mapper;

        public FlightsController(AirlineDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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

                var flightDetailReadDto = _mapper.Map<FlightDetailReadDto>(flightDetailCreateDto);
                return CreatedAtRoute(nameof(GetFlightDetailsByCode), new { flightCode = flightDetailReadDto.FlightCode }, flightDetailReadDto);
            }

            return NoContent();
        }
    }
}