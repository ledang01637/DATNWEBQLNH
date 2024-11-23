using DATN.Shared;
using System.Collections.Generic;
using System;
using DATN.Server.Service;
using System.Linq;
using DATN.Server.Data;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace DATN.Server.Service
{
    public class ReservationService
    {
        private AppDBContext _context;
        public ReservationService(AppDBContext context)
        {
            _context = context;
        }
        public List<Reservation> GetReservation()
        {
            return _context.Reservations.ToList();
        }
        public Reservation AddReservation(Reservation Reservation)
        {
            _context.Add(Reservation);
            _context.SaveChanges();
            return Reservation;
        }
        public Reservation DeleteReservation(int id)
        {
            var existing = _context.Reservations.Find(id);
            if (existing == null)
            {
                return null;
            }
            else
            {
                _context.Remove(existing);
                _context.SaveChanges();
                return existing;
            }
        }
        public Reservation GetIdReservation(int id)
        {
            var reservation = _context.Reservations.Find(id);
            if (reservation == null)
            {
                return null;
            }
            return reservation;
        }
        public Reservation UpdateReservation(int id, Reservation update)
        {
            var existing = _context.Reservations.Find(id);
            if (existing == null)
            {
                return null;
            }
            existing.CustomerName = update.CustomerName;
            existing.CustomerPhone = update.CustomerPhone;
            existing.CustomerEmail = update.CustomerEmail;
            existing.ReservationTime = update.ReservationTime;
            existing.Adults = update.Adults;
            existing.Children = update.Children;
            existing.Tables = update.Tables;
            existing.IsPayment = update.IsPayment;
            existing.DepositPayment = update.DepositPayment;
            existing.PaymentMethod = update.PaymentMethod;
            existing.CreatedDate = update.CreatedDate;
            existing.UpdatedDate = update.UpdatedDate;
            existing.ReservationStatus = update.ReservationStatus;
            existing.CustomerNote = update.CustomerNote;
            existing.TableId = update.TableId;
            existing.IsDeleted = update.IsDeleted;


            _context.Update(existing);
            _context.SaveChanges();
            return existing;
        }
    }
}
