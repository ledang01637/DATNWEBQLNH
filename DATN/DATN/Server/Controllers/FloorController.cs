using Microsoft.AspNetCore.Mvc;
using DATN.Shared;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using DATN.Server.Service;

namespace DATN.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FloorController : ControllerBase
    {
        private readonly FloorService _FloorService;

        public FloorController(FloorService _floor)
        {
            _FloorService = _floor;
        }

        [HttpGet("GetFloor")]
        public List<Floor> GetFloor()
        {
            return _FloorService.GetFloor();
        }

        [HttpPost("AddFloor")]
        public Floor AddFloor(Floor Floor)
        {
            return _FloorService.AddFloor(new Floor
            {
                NumberFloor = Floor.NumberFloor,
                IsActive = Floor.IsActive,

            });
        }

        [HttpGet("{id}")]
        public ActionResult<Floor> GetIdFloor(int id)
        {
            if (id == 0)
            {
                return BadRequest("Value must be...");

            }
            return Ok(_FloorService.GetIdFloor(id));
        }


        [HttpDelete("{id}")]
        public IActionResult DeleteFloor(int id)
        {
            var deleted = _FloorService.DeleteFloor(id);
            if (deleted == null)
            {
                return NotFound("Floor not found");
            }

            return Ok(deleted);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateFloor(int id, [FromBody] Floor updatedFloor)
        {
            var updated = _FloorService.UpdateFloor(id, updatedFloor);
            if (updated == null)
            {
                return NotFound("Floor not found");
            }

            return Ok(updated);
        }
    }
}
