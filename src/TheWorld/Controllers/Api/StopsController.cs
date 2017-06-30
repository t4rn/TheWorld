using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheWorld.Models;
using TheWorld.Services;
using TheWorld.ViewModels;

namespace TheWorld.Controllers.Api
{
    [Authorize]
    [Route("/api/trips/{tripName}/stops")]
    public class StopsController : Controller
    {
        private readonly IGeoCoordsService _coordsService;
        private readonly ILogger<StopsController> _logger;
        private readonly IWorldRepository _repository;

        public StopsController(IWorldRepository repository, 
            ILogger<StopsController> logger,
            IGeoCoordsService coordsService)
        {
            _repository = repository;
            _logger = logger;
            _coordsService = coordsService;
        }

        [HttpGet("")]
        public IActionResult Get(string tripName)
        {
            try
            {
                System.Threading.Thread.Sleep(200);
                Trip trip = _repository.GetTripByNameAndUser(tripName, User.Identity.Name);
                if (trip != null)
                {
                    return Ok(Mapper.Map<IEnumerable<StopViewModel>>(trip.Stops.OrderBy(x => x.Order).ToList()));
                }
                else
                {
                    return BadRequest($"Failed to get trip '{tripName}'");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get stops: {0}", ex);
            }

            return BadRequest("Failed to get stops");
        }

        [HttpPost("")]
        public async Task<IActionResult> Post(string tripName, [FromBody]StopViewModel vm)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // map
                    Stop newStop = Mapper.Map<Stop>(vm);

                    // lookup the geocodes
                    GeoCoordsResult coordsResult = await _coordsService.GetCoordsAsync(newStop.Name);

                    if (coordsResult.Success == false)
                    {
                        _logger.LogError(coordsResult.Message);
                    }
                    else
                    {
                        newStop.Latitude = coordsResult.Latitude;
                        newStop.Longitude = coordsResult.Longitude;

                        // save to db
                        _repository.AddStop(tripName, newStop, User.Identity.Name);

                        if (await _repository.SaveChangesAsync())
                        {
                            return Created($"/api/trips/{tripName}/stops/{newStop.Name}",
                                Mapper.Map<StopViewModel>(newStop));
                        }
                    }
                }
                else
                {
                    return BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to save Stop: {0}", ex);
            }

            return BadRequest("Failed to save Stop");
        }
    }
}
