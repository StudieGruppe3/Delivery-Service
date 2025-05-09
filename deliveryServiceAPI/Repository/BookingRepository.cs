﻿using System;
using System.Collections.Generic;
using System.Linq;
using Models;

namespace deliveryServiceAPI.Repository
{
    public class BookingRepository
    {
        private readonly List<BookingDTO> _bookings = new();
        private readonly object _lock = new(); // Tråd-sikkerhed

        public void Put(BookingDTO booking)
        {
            lock (_lock)
            {
                _bookings.Add(booking);
            }
        }

        public List<BookingDTO> GetAll()
        {
            lock (_lock)
            {
                return _bookings.OrderBy(b => b.Deadline).ToList();
            }
        }
    }

    // 🔹 Sørg for, at BookingDTO-klassen findes i det samme namespace
    
}
