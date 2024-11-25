using DATN.Server.Service;
using DATN.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace DATN.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly TransactionService _TransactionService;

        public TransactionController(TransactionService _Transaction)
        {
            _TransactionService = _Transaction;
        }

        [HttpGet("GetTransaction")]
        public List<Transaction> GetTransaction()
        {
            return _TransactionService.GetTransaction();
        }

        [HttpPost("AddTransaction")]
        public Transaction AddTransaction(Transaction Transaction)
        {
            return _TransactionService.AddTransaction(new Transaction
            {
                ReservationId = Transaction.ReservationId,
                Amount = Transaction.Amount,
                PaymentDate = Transaction.PaymentDate,
                PaymentStatus = Transaction.PaymentStatus,
            });
        }

        [HttpGet("{id}")]
        public ActionResult<Transaction> GetIdTransaction(int id)
        {
            if (id == 0)
            {
                return BadRequest("Value must be...");

            }
            return Ok(_TransactionService.GetIdTransaction(id));
        }


        [HttpDelete("{id}")]
        public IActionResult DeleteTransaction(int id)
        {
            var deleted = _TransactionService.DeleteTransaction(id);
            if (deleted == null)
            {
                return NotFound("Transaction not found");
            }

            return Ok(deleted);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateTransaction(int id, [FromBody] Transaction updatedTransaction)
        {
            var updated = _TransactionService.UpdateTransaction(id, updatedTransaction);
            if (updated == null)
            {
                return NotFound("Transaction not found");
            }

            return Ok(updated);
        }
    }
}
