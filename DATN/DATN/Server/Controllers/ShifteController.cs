using Microsoft.AspNetCore.Mvc;
using DATN.Shared;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using DATN.Server.Service;

namespace DATN.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShifteController : ControllerBase
    {
        private readonly ShifteService _ShifteService;

        public ShifteController(ShifteService _shifte)
        {
            _ShifteService = _shifte;
        }

        [HttpGet("GetShifte")]
        public List<Shifte> GetShiftes()
        {
            return _ShifteService.GetShifte();
        }

        [HttpPost("AddShifte")]
        public Shifte AddShifte(Shifte Shifte)
        {
            return _ShifteService.AddShifte(new Shifte
            {
                Shifte_Name = Shifte.Shifte_Name,
                StartTime = Shifte.StartTime,
                EndTime = Shifte.EndTime,
                IsDeleted = Shifte.IsDeleted,

            });
        }

        [HttpGet("{id}")]
        public ActionResult<Shifte> GetIdShifte(int id)
        {
            if (id == 0)
            {
                return BadRequest("Value must be...");

            }
            return Ok(_ShifteService.GetIdShifte(id));
        }


        [HttpDelete("{id}")]
        public IActionResult DeleteShifte(int id)
        {
            var deleted = _ShifteService.DeleteShifte(id);
            if (deleted == null)
            {
                return NotFound("Shifte not found");
            }

            return Ok(deleted);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateShifte(int id, [FromBody] Shifte updatedShifte)
        {
            var updated = _ShifteService.UpdateShifte(id, updatedShifte);
            if (updated == null)
            {
                return NotFound("Shifte not found");
            }

            return Ok(updated);
        }
    }
}
