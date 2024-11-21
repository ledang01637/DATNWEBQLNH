using Microsoft.AspNetCore.Mvc;
using DATN.Shared;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using DATN.Server.Service;

namespace DATN.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeServiceController : ControllerBase
    {
        private readonly EmployeeShifteService _EmployeeShifteService;

        public EmployeeServiceController(EmployeeShifteService _EmployeeShifte)
        {
            _EmployeeShifteService = _EmployeeShifte;
        }

        [HttpGet("GetEmployeeService")]
        public List<EmployeeShifte> GetEmployeeShifte()
        {
            return _EmployeeShifteService.GetEmployeeShifte();
        }

        [HttpPost("AddEmployeeService")]
        public EmployeeShifte AddEmployeeShifte(EmployeeShifte EmployeeShifte)
        {
            return _EmployeeShifteService.AddEmployeeShifte(new EmployeeShifte
            {
                ShifteDay = EmployeeShifte.ShifteDay,
                EmployeeId = EmployeeShifte.EmployeeId,
                ShifteId = EmployeeShifte.ShifteId,

            });
        }

        [HttpGet("{id}")]
        public ActionResult<EmployeeShifte> GetIdEmployeeShifte(int id)
        {
            if (id == 0)
            {
                return BadRequest("Value must be...");

            }
            return Ok(_EmployeeShifteService.GetIdEmployeeShifte(id));
        }


        [HttpDelete("{id}")]
        public IActionResult DeleteEmployeeShifte(int id)
        {
            var deleted = _EmployeeShifteService.DeleteEmployeeShifte(id);
            if (deleted == null)
            {
                return NotFound("EmployeeShifte not found");
            }

            return Ok(deleted);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateEmployeeShifte(int id, [FromBody] EmployeeShifte updatedEmployeeShifte)
        {
            var updated = _EmployeeShifteService.UpdateEmployeeShifte(id, updatedEmployeeShifte);
            if (updated == null)
            {
                return NotFound("EmployeeShifte not found");
            }

            return Ok(updated);
        }
    }
}
