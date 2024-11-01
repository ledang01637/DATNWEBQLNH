using Microsoft.AspNetCore.Mvc;
using DATN.Shared;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using DATN.Server.Service;

namespace DATN.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly ReservationService _ReservationService;

        public ReservationController(ReservationService _reservation)
        {
            _ReservationService = _reservation;
        }

        [HttpGet("GetReservation")]
        public List<Reservation> GetReservation()
        {
            return _ReservationService.GetReservation();
        }

        [HttpPost("AddReservation")]
        public Reservation AddReservation(Reservation Reservation)
        {
            return _ReservationService.AddReservation(new Reservation
            {
                CustomerName = Reservation.CustomerName,
                CustomerPhone = Reservation.CustomerPhone,
                ReservationDate = Reservation.ReservationDate,
                NumberGuest = Reservation.NumberGuest,
                Tables = Reservation.Tables,
                IsPayment = Reservation.IsPayment,
                DepositPayment = Reservation.DepositPayment,
                PaymentMethod = Reservation.PaymentMethod
            });
        }

        [HttpGet("{id}")]
        public ActionResult<Reservation> GetIdReservation(int id)
        {
            if (id == 0)
            {
                return BadRequest("Value must be...");

            }
            return Ok(_ReservationService.GetIdReservation(id));
        }


        [HttpDelete("{id}")]
        public IActionResult DeleteReservation(int id)
        {
            var deleted = _ReservationService.DeleteReservation(id);
            if (deleted == null)
            {
                return NotFound("Reservation not found");
            }

            return Ok(deleted);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateReservation(int id, [FromBody] Reservation updatedReservation)
        {
            var updated = _ReservationService.UpdateReservation(id, updatedReservation);
            if (updated == null)
            {
                return NotFound("Reservation not found");
            }

            return Ok(updated);
        }
    }
}
