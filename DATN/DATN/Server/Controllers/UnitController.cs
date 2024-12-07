using Microsoft.AspNetCore.Mvc;
using DATN.Shared;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using DATN.Server.Service;

namespace DATN.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnitController : ControllerBase
    {
        private readonly UnitService _UnitService;

        public UnitController(UnitService _unit)
        {
            _UnitService = _unit;
        }

        [HttpGet("GetUnit")]
        public List<Unit> GetUnits()
        {
            return _UnitService.GetUnit();
        }

        [HttpPost("AddUnit")]
        public Unit AddUnit([FromBody] Unit Unit)
        {
            return _UnitService.AddUnit(new Unit
            {
                UnitName = Unit.UnitName,
                UnitDescription = Unit.UnitDescription,
            });
        }

        [HttpGet("{id}")]
        public ActionResult<Unit> GetIdUnit(int id)
        {
            if (id == 0)
            {
                return BadRequest("Value must be...");

            }
            return Ok(_UnitService.GetIdUnit(id));
        }


        [HttpDelete("{id}")]
        public IActionResult DeleteUnit(int id)
        {
            var deleted = _UnitService.DeleteUnit(id);
            if (deleted == null)
            {
                return NotFound("Unit not found");
            }

            return Ok(deleted);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateUnit(int id, [FromBody] Unit updatedUnit)
        {
            var updated = _UnitService.UpdateUnit(id, updatedUnit);
            if (updated == null)
            {
                return NotFound("Unit not found");
            }

            return Ok(updated);
        }
    }
}
