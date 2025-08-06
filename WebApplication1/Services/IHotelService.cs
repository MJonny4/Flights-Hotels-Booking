using WebApplication1.DTOs;

namespace WebApplication1.Services
{
    public interface IHotelService
    {
        Task<List<Hotel>> SearchHotelsByCityAsync(string cityCode);
        Task<List<Hotel>> SearchHotelsByGeoCodeAsync(double latitude, double longitude, int radius = 5, string radiusUnit = "KM", string? chainCodes = null);
        Task<List<Hotel>> SearchHotelsByKeywordAsync(string keyword, string? subType = null);
        Task<List<HotelOffer>> GetHotelOffersAsync(List<string> hotelIds, string checkInDate, string checkOutDate, int adults = 1, int rooms = 1, string currency = "USD", string? paymentPolicy = null, string? boardType = null);
        Task<List<HotelSentiment>> GetHotelSentimentsAsync(List<string> hotelIds);
        Task<List<Hotel>> SearchHotelsForDestinationAsync(HotelSearchRequest request);
        Task<HotelBookingResponse?> CreateHotelBookingAsync(HotelBookingRequest request);
    }
}