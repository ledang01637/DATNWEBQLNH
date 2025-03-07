﻿using DATN.Shared;
using System.Collections.Generic;
using System;
using DATN.Server.Service;
using System.Linq;
using DATN.Server.Data;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;

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

        public Reservation GetReservationByTimeTableId(int tableId)
        {
            var now = DateTime.Now;
            var reservation = _context.Reservations.FirstOrDefault(a => a.TableId == tableId && a.ReservationStatus == "Đã nhận bàn");

            return reservation ?? new Reservation();
        }

        public List<Reservation> GetReservationByTableId(int tableId)
        {
            var reservations = _context.Reservations.Where(a => a.TableId == tableId).ToList();
            return reservations ?? new List<Reservation>();
        }

        public List<Reservation> GetReservationInclude()
        {
            var reservations = _context.Reservations.Where(a => !a.IsDeleted).Include(a => a.Tables).ToList();
            return reservations ?? new List<Reservation>();
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
