﻿using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TheWorld.Models
{
    public class WorldRepository : IWorldRepository
    {
        private readonly WorldContext _context;
        private readonly ILogger<WorldRepository> _logger;

        public WorldRepository(WorldContext context, ILogger<WorldRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void AddStop(string tripName, Stop newStop, string username)
        {
            Trip trip = GetTripByNameAndUser(tripName, username);
            if (trip != null)
            {
                trip.Stops.Add(newStop);
                //_context.Stops.Add(newStop); <- not needed
            }
        }

        public void AddTrip(Trip trip)
        {
            _context.Add(trip);
        }

        public void DeleteStop(string tripName, string stopName, string username)
        {
            Trip trip = GetTripByNameAndUser(tripName, username);
            if (trip != null)
            {
                Stop stop = trip.Stops.Where(x => x.Name == stopName).FirstOrDefault();
                if (stop != null)
                {
                    _context.Stops.Remove(stop);
                }
            }
        }

        private Stop GetStopByName(string stopName)
        {
            return _context.Stops.Where(x => x.Name == stopName).FirstOrDefault();
        }

        public void DeleteTrip(string tripName, string username)
        {
            Trip trip = GetTripByNameAndUser(tripName, username);
            if (trip != null)
            {
                _context.Remove(trip);
            }
        }

        public IEnumerable<Trip> GetAllTrips()
        {
            _logger.LogInformation("Getting All Trips from the Database");
            return _context.Trips.ToList();
        }

        public Trip GetTripByName(string tripName)
        {
            return _context.Trips
                .Include(t => t.Stops)
                .Where(t => t.Name == tripName)
                .FirstOrDefault();
        }

        public Trip GetTripByNameAndUser(string tripName, string username)
        {
            return _context.Trips
                .Include(t => t.Stops)
                .Where(t => t.Name == tripName && t.UserName == username)
                .FirstOrDefault();
        }

        public IEnumerable<Trip> GetTripsByUsername(string username)
        {
            return _context
                .Trips
                .Include(x => x.Stops)
                .Where(x => x.UserName == username)
                .ToList();
        }

        public async Task<bool> SaveChangesAsync()
        {
            int rowsAffected = await _context.SaveChangesAsync();

            return rowsAffected > 0;
        }
    }
}
