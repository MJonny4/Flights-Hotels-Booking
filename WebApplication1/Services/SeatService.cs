using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class SeatService : ISeatService
    {
        private readonly ApplicationDbContext _context;

        public SeatService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task GenerateSeatsForFlightAsync(int flightId)
        {
            // Check if seats already exist for this flight
            var existingSeats = await _context.Seats
                .Where(s => s.FlightId == flightId)
                .CountAsync();

            if (existingSeats > 0)
                return; // Seats already generated

            var seats = new List<Seat>();

            // Business Class: Rows 1-3 (2-2 configuration)
            // Economy Class: Rows 4-25 (3-3 configuration)

            // Generate Business Class seats (18 seats)
            for (int row = 1; row <= 3; row++)
            {
                var seatLetters = new[] { "A", "C", "D", "F" }; // 2-2 config, no B or E
                foreach (var letter in seatLetters)
                {
                    seats.Add(new Seat
                    {
                        FlightId = flightId,
                        SeatNumber = $"{row}{letter}",
                        SeatClass = FlightClass.Business,
                        IsAvailable = true,
                        IsWindowSeat = letter == "A" || letter == "F",
                        IsAisleSeat = letter == "C" || letter == "D"
                    });
                }
            }

            // Generate Economy Class seats (132 seats)
            for (int row = 4; row <= 25; row++)
            {
                var seatLetters = new[] { "A", "B", "C", "D", "E", "F" }; // 3-3 config
                foreach (var letter in seatLetters)
                {
                    seats.Add(new Seat
                    {
                        FlightId = flightId,
                        SeatNumber = $"{row}{letter}",
                        SeatClass = FlightClass.Economy,
                        IsAvailable = true,
                        IsWindowSeat = letter == "A" || letter == "F",
                        IsAisleSeat = letter == "C" || letter == "D"
                    });
                }
            }

            _context.Seats.AddRange(seats);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Seat>> GetAvailableSeatsAsync(int flightId, FlightClass flightClass)
        {
            return await _context.Seats
                .Where(s => s.FlightId == flightId && s.SeatClass == flightClass && s.IsAvailable)
                .OrderBy(s => s.SeatNumber)
                .ToListAsync();
        }

        public async Task<Seat?> GetSeatByIdAsync(int seatId)
        {
            return await _context.Seats
                .Include(s => s.Flight)
                .FirstOrDefaultAsync(s => s.Id == seatId);
        }

        public async Task<bool> IsSeatAvailableAsync(int seatId)
        {
            var seat = await _context.Seats.FindAsync(seatId);
            return seat?.IsAvailable ?? false;
        }

        public async Task ReserveSeatAsync(int seatId)
        {
            var seat = await _context.Seats.FindAsync(seatId);
            if (seat != null)
            {
                seat.IsAvailable = false;
                await _context.SaveChangesAsync();
            }
        }

        public async Task ReleaseSeatAsync(int seatId)
        {
            var seat = await _context.Seats.FindAsync(seatId);
            if (seat != null)
            {
                seat.IsAvailable = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Seat>> GetSeatMapAsync(int flightId)
        {
            return await _context.Seats
                .Where(s => s.FlightId == flightId)
                .OrderBy(s => s.SeatNumber)
                .ToListAsync();
        }

        public async Task<int> GetAvailableSeatCountAsync(int flightId, FlightClass flightClass)
        {
            return await _context.Seats
                .Where(s => s.FlightId == flightId && s.SeatClass == flightClass && s.IsAvailable)
                .CountAsync();
        }

        public async Task<bool> HasEnoughSeatsAsync(int flightId, FlightClass flightClass, int requiredSeats)
        {
            var availableSeats = await GetAvailableSeatCountAsync(flightId, flightClass);
            return availableSeats >= requiredSeats;
        }

        public async Task ReserveSeatsAsync(List<int> seatIds)
        {
            var seats = await _context.Seats
                .Where(s => seatIds.Contains(s.Id) && s.IsAvailable)
                .ToListAsync();

            foreach (var seat in seats)
            {
                seat.IsAvailable = false;
            }

            await _context.SaveChangesAsync();
        }

        public async Task SimulateOccupiedSeatsAsync(int flightId, int economyOccupiedCount, int businessOccupiedCount)
        {
            var random = new Random();

            // Occupy random Economy seats
            if (economyOccupiedCount > 0)
            {
                var economySeats = await _context.Seats
                    .Where(s => s.FlightId == flightId && s.SeatClass == FlightClass.Economy && s.IsAvailable)
                    .OrderBy(s => Guid.NewGuid()) // Random order
                    .Take(economyOccupiedCount)
                    .ToListAsync();

                foreach (var seat in economySeats)
                {
                    seat.IsAvailable = false;
                }
            }

            // Occupy random Business seats
            if (businessOccupiedCount > 0)
            {
                var businessSeats = await _context.Seats
                    .Where(s => s.FlightId == flightId && s.SeatClass == FlightClass.Business && s.IsAvailable)
                    .OrderBy(s => Guid.NewGuid()) // Random order
                    .Take(businessOccupiedCount)
                    .ToListAsync();

                foreach (var seat in businessSeats)
                {
                    seat.IsAvailable = false;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}