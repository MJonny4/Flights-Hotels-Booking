using System.Text.Json.Serialization;

namespace WebApplication1.DTOs
{
    // Common DTOs
    public class GeoCode
    {
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }
        
        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }
    }

    public class Address
    {
        [JsonPropertyName("countryCode")]
        public string CountryCode { get; set; } = string.Empty;
        
        [JsonPropertyName("postalCode")]
        public string PostalCode { get; set; } = string.Empty;
        
        [JsonPropertyName("stateCode")]
        public string StateCode { get; set; } = string.Empty;
        
        [JsonPropertyName("cityName")]
        public string CityName { get; set; } = string.Empty;
        
        [JsonPropertyName("lines")]
        public List<string> Lines { get; set; } = new List<string>();
    }

    // Hotel Search DTOs
    public class HotelsByCityResponse
    {
        [JsonPropertyName("data")]
        public List<Hotel> Data { get; set; } = new List<Hotel>();
    }

    public class Hotel
    {
        [JsonPropertyName("chainCode")]
        public string ChainCode { get; set; } = string.Empty;
        
        [JsonPropertyName("iataCode")]
        public string IataCode { get; set; } = string.Empty;
        
        [JsonPropertyName("dupeId")]
        public long DupeId { get; set; }
        
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("hotelId")]
        public string HotelId { get; set; } = string.Empty;
        
        [JsonPropertyName("geoCode")]
        public GeoCode GeoCode { get; set; } = new GeoCode();
        
        [JsonPropertyName("address")]
        public Address Address { get; set; } = new Address();
        
        [JsonPropertyName("lastUpdate")]
        public DateTime LastUpdate { get; set; }
        
        [JsonPropertyName("distance")]
        public Distance? Distance { get; set; }
    }

    public class Distance
    {
        [JsonPropertyName("value")]
        public double Value { get; set; }
        
        [JsonPropertyName("unit")]
        public string Unit { get; set; } = string.Empty;
    }

    // Hotel Offers DTOs
    public class HotelOffersResponse
    {
        [JsonPropertyName("data")]
        public List<HotelOffer> Data { get; set; } = new List<HotelOffer>();
    }

    public class HotelOffer
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
        
        [JsonPropertyName("hotel")]
        public HotelInfo Hotel { get; set; } = new HotelInfo();
        
        [JsonPropertyName("available")]
        public bool Available { get; set; }
        
        [JsonPropertyName("offers")]
        public List<Offer> Offers { get; set; } = new List<Offer>();
    }

    public class HotelInfo
    {
        [JsonPropertyName("hotelId")]
        public string HotelId { get; set; } = string.Empty;
        
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("chainCode")]
        public string ChainCode { get; set; } = string.Empty;
    }

    public class Offer
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        
        [JsonPropertyName("checkInDate")]
        public string CheckInDate { get; set; } = string.Empty;
        
        [JsonPropertyName("checkOutDate")]
        public string CheckOutDate { get; set; } = string.Empty;
        
        [JsonPropertyName("roomQuantity")]
        public int RoomQuantity { get; set; }
        
        [JsonPropertyName("room")]
        public Room Room { get; set; } = new Room();
        
        [JsonPropertyName("price")]
        public HotelPrice Price { get; set; } = new HotelPrice();

        [JsonPropertyName("policies")]
        public HotelPolicies? Policies { get; set; }

        [JsonPropertyName("rateFamilyEstimated")]
        public RateFamilyEstimated? RateFamilyEstimated { get; set; }
    }

    public class Room
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
        
        [JsonPropertyName("description")]
        public RoomDescription Description { get; set; } = new RoomDescription();
    }

    public class RoomDescription
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    public class HotelPrice
    {
        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;
        
        [JsonPropertyName("total")]
        public string Total { get; set; } = string.Empty;
        
        [JsonPropertyName("base")]
        public string Base { get; set; } = string.Empty;
        
        [JsonPropertyName("taxes")]
        public List<Tax> Taxes { get; set; } = new List<Tax>();
    }

    public class Tax
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;
        
        [JsonPropertyName("amount")]
        public string Amount { get; set; } = string.Empty;
    }

    // Hotel Sentiments DTOs
    public class HotelSentimentsResponse
    {
        [JsonPropertyName("meta")]
        public HotelMeta Meta { get; set; } = new HotelMeta();
        
        [JsonPropertyName("data")]
        public List<HotelSentiment> Data { get; set; } = new List<HotelSentiment>();
        
        [JsonPropertyName("warnings")]
        public List<Warning> Warnings { get; set; } = new List<Warning>();
    }

    public class HotelSentiment
    {
        [JsonPropertyName("hotelId")]
        public string HotelId { get; set; } = string.Empty;
        
        [JsonPropertyName("overallRating")]
        public int OverallRating { get; set; }
        
        [JsonPropertyName("numberOfReviews")]
        public int NumberOfReviews { get; set; }
        
        [JsonPropertyName("numberOfRatings")]
        public int NumberOfRatings { get; set; }
        
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
        
        [JsonPropertyName("sentiments")]
        public Sentiments Sentiments { get; set; } = new Sentiments();
    }

    public class Sentiments
    {
        [JsonPropertyName("staff")]
        public int Staff { get; set; }
        
        [JsonPropertyName("location")]
        public int Location { get; set; }
        
        [JsonPropertyName("service")]
        public int Service { get; set; }
        
        [JsonPropertyName("roomComforts")]
        public int RoomComforts { get; set; }
        
        [JsonPropertyName("sleepQuality")]
        public int SleepQuality { get; set; }
        
        [JsonPropertyName("swimmingPool")]
        public int SwimmingPool { get; set; }
        
        [JsonPropertyName("valueForMoney")]
        public int ValueForMoney { get; set; }
        
        [JsonPropertyName("facilities")]
        public int Facilities { get; set; }
        
        [JsonPropertyName("catering")]
        public int Catering { get; set; }
        
        [JsonPropertyName("pointsOfInterest")]
        public int PointsOfInterest { get; set; }
    }

    public class HotelMeta
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }
        
        [JsonPropertyName("links")]
        public HotelLinks Links { get; set; } = new HotelLinks();
    }

    public class HotelLinks
    {
        [JsonPropertyName("self")]
        public string Self { get; set; } = string.Empty;
    }

    public class Warning
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }
        
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
        
        [JsonPropertyName("source")]
        public Source Source { get; set; } = new Source();
        
        [JsonPropertyName("detail")]
        public string Detail { get; set; } = string.Empty;
    }

    public class Source
    {
        [JsonPropertyName("parameter")]
        public string Parameter { get; set; } = string.Empty;
        
        [JsonPropertyName("pointer")]
        public string Pointer { get; set; } = string.Empty;
    }

    // Error Response DTOs
    public class AmadeusErrorResponse
    {
        [JsonPropertyName("errors")]
        public List<AmadeusError> Errors { get; set; } = new List<AmadeusError>();
    }

    public class AmadeusError
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }
        
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
        
        [JsonPropertyName("status")]
        public int Status { get; set; }
        
        [JsonPropertyName("detail")]
        public string Detail { get; set; } = string.Empty;
        
        [JsonPropertyName("source")]
        public Source? Source { get; set; }
    }

    // Hotel Search Request DTO - Enhanced to match amadeus.md specifications
    public class HotelSearchRequest
    {
        public string CityCode { get; set; } = string.Empty;
        public string CheckInDate { get; set; } = string.Empty;
        public string CheckOutDate { get; set; } = string.Empty;
        public int Adults { get; set; } = 1;
        public int Rooms { get; set; } = 1;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? Keyword { get; set; }
        public string CurrencyCode { get; set; } = "USD";
        public string? PaymentPolicy { get; set; } // GUARANTEE, DEPOSIT, NONE
        public string? BoardType { get; set; } // ROOM_ONLY, BREAKFAST, HALF_BOARD, FULL_BOARD, ALL_INCLUSIVE
        public bool IncludeClosed { get; set; } = false;
        public bool BestRateOnly { get; set; } = true;
        public string? PriceRange { get; set; }
        public string? ChainCodes { get; set; } // Comma-separated hotel chain codes
        public int? Radius { get; set; } // Search radius for geocode searches
        public string RadiusUnit { get; set; } = "KM"; // KM or MILE
    }

    // Hotel Booking DTOs as per amadeus.md specifications
    public class HotelBookingRequest
    {
        [JsonPropertyName("data")]
        public HotelBookingData Data { get; set; } = new HotelBookingData();
    }

    public class HotelBookingData
    {
        [JsonPropertyName("offerId")]
        public string OfferId { get; set; } = string.Empty;

        [JsonPropertyName("guests")]
        public List<HotelGuest> Guests { get; set; } = new List<HotelGuest>();

        [JsonPropertyName("payments")]
        public List<HotelPayment> Payments { get; set; } = new List<HotelPayment>();

        [JsonPropertyName("rooms")]
        public List<RoomAssociation>? Rooms { get; set; }
    }

    public class HotelGuest
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public HotelGuestName Name { get; set; } = new HotelGuestName();

        [JsonPropertyName("contact")]
        public HotelGuestContact Contact { get; set; } = new HotelGuestContact();
    }

    public class HotelGuestName
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; } = string.Empty;

        [JsonPropertyName("lastName")]
        public string LastName { get; set; } = string.Empty;
    }

    public class HotelGuestContact
    {
        [JsonPropertyName("phone")]
        public string Phone { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
    }

    public class HotelPayment
    {
        [JsonPropertyName("method")]
        public string Method { get; set; } = "creditCard";

        [JsonPropertyName("paymentCard")]
        public HotelPaymentCard PaymentCard { get; set; } = new HotelPaymentCard();
    }

    public class HotelPaymentCard
    {
        [JsonPropertyName("paymentCardInfo")]
        public PaymentCardInfo PaymentCardInfo { get; set; } = new PaymentCardInfo();
    }

    public class PaymentCardInfo
    {
        [JsonPropertyName("vendorCode")]
        public string VendorCode { get; set; } = string.Empty;

        [JsonPropertyName("cardNumber")]
        public string CardNumber { get; set; } = string.Empty;

        [JsonPropertyName("expiryDate")]
        public string ExpiryDate { get; set; } = string.Empty;

        [JsonPropertyName("holderName")]
        public string HolderName { get; set; } = string.Empty;
    }

    public class RoomAssociation
    {
        [JsonPropertyName("guestIds")]
        public List<int> GuestIds { get; set; } = new List<int>();
    }

    // Hotel Booking Response
    public class HotelBookingResponse
    {
        [JsonPropertyName("data")]
        public List<HotelBookingResult> Data { get; set; } = new List<HotelBookingResult>();

        [JsonPropertyName("warnings")]
        public List<Warning> Warnings { get; set; } = new List<Warning>();
    }

    public class HotelBookingResult
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("providerConfirmationId")]
        public string ProviderConfirmationId { get; set; } = string.Empty;

        [JsonPropertyName("associatedRecords")]
        public List<AssociatedRecord> AssociatedRecords { get; set; } = new List<AssociatedRecord>();
    }

    public class AssociatedRecord
    {
        [JsonPropertyName("reference")]
        public string Reference { get; set; } = string.Empty;

        [JsonPropertyName("originSystemCode")]
        public string OriginSystemCode { get; set; } = string.Empty;
    }

    // Additional DTOs for enhanced hotel functionality
    public class HotelPolicies
    {
        [JsonPropertyName("cancellation")]
        public CancellationPolicy? Cancellation { get; set; }

        [JsonPropertyName("guarantee")]
        public GuaranteePolicy? Guarantee { get; set; }

        [JsonPropertyName("paymentType")]
        public string PaymentType { get; set; } = string.Empty;
    }

    public class CancellationPolicy
    {
        [JsonPropertyName("deadline")]
        public string Deadline { get; set; } = string.Empty;

        [JsonPropertyName("amount")]
        public string Amount { get; set; } = string.Empty;
    }

    public class GuaranteePolicy
    {
        [JsonPropertyName("acceptedPayments")]
        public AcceptedPayments AcceptedPayments { get; set; } = new AcceptedPayments();
    }

    public class AcceptedPayments
    {
        [JsonPropertyName("creditCards")]
        public List<string> CreditCards { get; set; } = new List<string>();

        [JsonPropertyName("methods")]
        public List<string> Methods { get; set; } = new List<string>();
    }

    public class RateFamilyEstimated
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
    }
}