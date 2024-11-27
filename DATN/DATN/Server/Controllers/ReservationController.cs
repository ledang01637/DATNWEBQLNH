using Microsoft.AspNetCore.Mvc;
using DATN.Shared;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using DATN.Server.Service;
using System;

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

        [HttpGet("GetReservationByTimeTableId")]
        public ActionResult<Reservation> GetReservationByTimeTableId(int tableId)
        {
            if(tableId <= 0)
            {
                return BadRequest();
            }
            return Ok(_ReservationService.GetReservationByTimeTableId(tableId));
        }

        [HttpGet("GetReservationByTableId")]

        public ActionResult<List<Reservation>> GetReservationByTableId([FromQuery] int tableId)
        {
            if(tableId <= 0) { return BadRequest(); }

            return Ok(_ReservationService.GetReservationByTableId(tableId));
        }

        [HttpGet("GetReservationInclude")]
        public ActionResult<List<Reservation>> GetReservationInclude()
        {
            return _ReservationService.GetReservationInclude();
        }

        [HttpPost("AddReservation")]
        public Reservation AddReservation(Reservation Reservation)
        {
            return _ReservationService.AddReservation(new Reservation
            {
                CustomerName = Reservation.CustomerName,
                CustomerPhone = Reservation.CustomerPhone,
                CustomerEmail = Reservation.CustomerEmail,
                ReservationTime = Reservation.ReservationTime,
                Adults = Reservation.Adults,
                Children = Reservation.Children,
                Tables = Reservation.Tables,
                IsPayment = Reservation.IsPayment,
                DepositPayment = Reservation.DepositPayment,
                PaymentMethod = Reservation.PaymentMethod,
                CreatedDate = Reservation.CreatedDate,
                UpdatedDate = Reservation.UpdatedDate,
                ReservationStatus = Reservation.ReservationStatus,
                CustomerNote = Reservation.CustomerNote,
                TableId = Reservation.TableId,
                IsDeleted = Reservation.IsDeleted
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
