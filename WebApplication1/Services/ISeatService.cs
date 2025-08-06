using WebApplication1.Models;

namespace WebApplication1.Services
{
    public interface ISeatService
    {
        Task GenerateSeatsForFlightAsync(int flightId);
        Task<List<Seat>> GetAvailableSeatsAsync(int flightId, FlightClass flightClass);
        Task<Seat?> GetSeatByIdAsync(int seatId);
        Task<bool> IsSeatAvailableAsync(int seatId);
        Task ReserveSeatAsync(int seatId);
        Task ReleaseSeatAsync(int seatId);
        Task<List<Seat>> GetSeatMapAsync(int flightId);
        Task<int> GetAvailableSeatCountAsync(int flightId, FlightClass flightClass);
        Task<bool> HasEnoughSeatsAsync(int flightId, FlightClass flightClass, int requiredSeats);
        Task ReserveSeatsAsync(List<int> seatIds);
        Task SimulateOccupiedSeatsAsync(int flightId, int economyOccupiedCount, int businessOccupiedCount);
    }
}