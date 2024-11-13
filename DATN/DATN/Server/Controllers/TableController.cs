using Microsoft.AspNetCore.Mvc;
using DATN.Shared;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using DATN.Server.Service;

namespace DATN.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TableController : ControllerBase
    {
        private readonly TableService _TableService;

        public TableController(TableService _table)
        {
            _TableService = _table;
        }

        [HttpGet("GetTable")]
        public List<Table> GetTable()
        {
            return _TableService.GetTable();
        }

        [HttpPost("GetTableInclude")]
        public Table GetTableInclude([FromBody] int tableId)
        {
            return _TableService.GetTableInclude(tableId);
        }

        [HttpPost("AddTable")]
        public Table AddTable(Table Table)
        {
            return _TableService.AddTable(new Table
            {
                FloorId = Table.FloorId,
                TableNumber = Table.TableNumber,
                SeatingCapacity = Table.SeatingCapacity,
                IsDeleted = Table.IsDeleted,
                Status = Table.Status,
                Position = Table.Position,

            });
        }

        [HttpGet("{id}")]
        public ActionResult<Table> GetIdTable(int id)
        {
            if (id == 0)
            {
                return BadRequest("Value must be...");

            }
            return Ok(_TableService.GetIdTable(id));
        }


        [HttpDelete("{id}")]
        public IActionResult DeleteTable(int id)
        {
            var deleted = _TableService.DeleteTable(id);
            if (deleted == null)
            {
                return NotFound("Table not found");
            }

            return Ok(deleted);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateTable(int id, [FromBody] Table updatedTable)
        {
            var updated = _TableService.UpdateTable(id, updatedTable);
            if (updated == null)
            {
                return NotFound("Table not found");
            }

            return Ok(updated);
        }
    }
}
