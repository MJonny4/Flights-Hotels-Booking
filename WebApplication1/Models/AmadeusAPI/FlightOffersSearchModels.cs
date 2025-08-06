using System.Text.Json.Serialization;

namespace WebApplication1.Models
{
    // Root response model
    public class FlightOffersSearchResponse
    {
        [JsonPropertyName("meta")]
        public FlightSearchMeta Meta { get; set; } = new();

        [JsonPropertyName("data")]
        public List<FlightOffer> Data { get; set; } = new();

        [JsonPropertyName("dictionaries")]
        public FlightDictionaries? Dictionaries { get; set; }
    }

    // Meta information
    public class FlightSearchMeta
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("links")]
        public FlightSearchLinks? Links { get; set; }
    }

    public class FlightSearchLinks
    {
        [JsonPropertyName("self")]
        public string? Self { get; set; }

        [JsonPropertyName("next")]
        public string? Next { get; set; }

        [JsonPropertyName("previous")]
        public string? Previous { get; set; }

        [JsonPropertyName("last")]
        public string? Last { get; set; }

        [JsonPropertyName("first")]
        public string? First { get; set; }
    }

    // Main flight offer model
    public class FlightOffer
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "flight-offer";

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("source")]
        public string Source { get; set; } = string.Empty;

        [JsonPropertyName("instantTicketingRequired")]
        public bool InstantTicketingRequired { get; set; }

        [JsonPropertyName("nonHomogeneous")]
        public bool NonHomogeneous { get; set; }

        [JsonPropertyName("oneWay")]
        public bool OneWay { get; set; }

        [JsonPropertyName("isUpsellOffer")]
        public bool IsUpsellOffer { get; set; }

        [JsonPropertyName("lastTicketingDate")]
        public string? LastTicketingDate { get; set; }

        [JsonPropertyName("lastTicketingDateTime")]
        public string? LastTicketingDateTime { get; set; }

        [JsonPropertyName("numberOfBookableSeats")]
        public int NumberOfBookableSeats { get; set; }

        [JsonPropertyName("itineraries")]
        public List<FlightItinerary> Itineraries { get; set; } = new();

        [JsonPropertyName("price")]
        public FlightPrice Price { get; set; } = new();

        [JsonPropertyName("pricingOptions")]
        public FlightPricingOptions? PricingOptions { get; set; }

        [JsonPropertyName("validatingAirlineCodes")]
        public List<string> ValidatingAirlineCodes { get; set; } = new();

        [JsonPropertyName("travelerPricings")]
        public List<TravelerPricing> TravelerPricings { get; set; } = new();
    }

    // Itinerary and segments
    public class FlightItinerary
    {
        [JsonPropertyName("duration")]
        public string Duration { get; set; } = string.Empty;

        [JsonPropertyName("segments")]
        public List<FlightSegment> Segments { get; set; } = new();
    }

    public class FlightSegment
    {
        [JsonPropertyName("departure")]
        public FlightEndpoint Departure { get; set; } = new();

        [JsonPropertyName("arrival")]
        public FlightEndpoint Arrival { get; set; } = new();

        [JsonPropertyName("carrierCode")]
        public string CarrierCode { get; set; } = string.Empty;

        [JsonPropertyName("number")]
        public string Number { get; set; } = string.Empty;

        [JsonPropertyName("aircraft")]
        public Aircraft Aircraft { get; set; } = new();

        [JsonPropertyName("operating")]
        public OperatingCarrier? Operating { get; set; }

        [JsonPropertyName("duration")]
        public string Duration { get; set; } = string.Empty;

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("numberOfStops")]
        public int NumberOfStops { get; set; }

        [JsonPropertyName("blacklistedInEU")]
        public bool BlacklistedInEU { get; set; }
    }

    public class FlightEndpoint
    {
        [JsonPropertyName("iataCode")]
        public string IataCode { get; set; } = string.Empty;

        [JsonPropertyName("terminal")]
        public string? Terminal { get; set; }

        [JsonPropertyName("at")]
        public string At { get; set; } = string.Empty;
    }

    public class Aircraft
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;
    }

    public class OperatingCarrier
    {
        [JsonPropertyName("carrierCode")]
        public string CarrierCode { get; set; } = string.Empty;
    }

    // Price models
    public class FlightPrice
    {
        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonPropertyName("total")]
        public string Total { get; set; } = string.Empty;

        [JsonPropertyName("base")]
        public string Base { get; set; } = string.Empty;

        [JsonPropertyName("fees")]
        public List<PriceFee>? Fees { get; set; }

        [JsonPropertyName("grandTotal")]
        public string? GrandTotal { get; set; }

        [JsonPropertyName("additionalServices")]
        public List<AdditionalService>? AdditionalServices { get; set; }

        [JsonPropertyName("taxes")]
        public List<PriceTax>? Taxes { get; set; }

        [JsonPropertyName("refundableTaxes")]
        public string? RefundableTaxes { get; set; }
    }

    public class PriceFee
    {
        [JsonPropertyName("amount")]
        public string Amount { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
    }

    public class AdditionalService
    {
        [JsonPropertyName("amount")]
        public string Amount { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
    }

    public class PriceTax
    {
        [JsonPropertyName("amount")]
        public string Amount { get; set; } = string.Empty;

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;
    }

    // Pricing options
    public class FlightPricingOptions
    {
        [JsonPropertyName("fareType")]
        public List<string>? FareType { get; set; }

        [JsonPropertyName("includedCheckedBagsOnly")]
        public bool IncludedCheckedBagsOnly { get; set; }

        [JsonPropertyName("refundableFare")]
        public bool? RefundableFare { get; set; }

        [JsonPropertyName("noRestrictionFare")]
        public bool? NoRestrictionFare { get; set; }

        [JsonPropertyName("noPenaltyFare")]
        public bool? NoPenaltyFare { get; set; }
    }

    // Traveler pricing
    public class TravelerPricing
    {
        [JsonPropertyName("travelerId")]
        public string TravelerId { get; set; } = string.Empty;

        [JsonPropertyName("fareOption")]
        public string FareOption { get; set; } = string.Empty;

        [JsonPropertyName("travelerType")]
        public string TravelerType { get; set; } = string.Empty;

        [JsonPropertyName("price")]
        public FlightPrice Price { get; set; } = new();

        [JsonPropertyName("fareDetailsBySegment")]
        public List<FareDetails> FareDetailsBySegment { get; set; } = new();
    }

    public class FareDetails
    {
        [JsonPropertyName("segmentId")]
        public string SegmentId { get; set; } = string.Empty;

        [JsonPropertyName("cabin")]
        public string Cabin { get; set; } = string.Empty;

        [JsonPropertyName("fareBasis")]
        public string FareBasis { get; set; } = string.Empty;

        [JsonPropertyName("brandedFare")]
        public string? BrandedFare { get; set; }

        [JsonPropertyName("brandedFareLabel")]
        public string? BrandedFareLabel { get; set; }

        [JsonPropertyName("class")]
        public string Class { get; set; } = string.Empty;

        [JsonPropertyName("includedCheckedBags")]
        public BaggageAllowance? IncludedCheckedBags { get; set; }

        [JsonPropertyName("includedCabinBags")]
        public BaggageAllowance? IncludedCabinBags { get; set; }

        [JsonPropertyName("amenities")]
        public List<FlightAmenity>? Amenities { get; set; }
    }

    public class BaggageAllowance
    {
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("weight")]
        public int? Weight { get; set; }

        [JsonPropertyName("weightUnit")]
        public string? WeightUnit { get; set; }
    }

    public class FlightAmenity
    {
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("isChargeable")]
        public bool IsChargeable { get; set; }

        [JsonPropertyName("amenityType")]
        public string AmenityType { get; set; } = string.Empty;

        [JsonPropertyName("amenityProvider")]
        public AmenityProvider? AmenityProvider { get; set; }
    }

    public class AmenityProvider
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

    // Dictionaries
    public class FlightDictionaries
    {
        [JsonPropertyName("locations")]
        public Dictionary<string, LocationInfo>? Locations { get; set; }

        [JsonPropertyName("aircraft")]
        public Dictionary<string, string>? Aircraft { get; set; }

        [JsonPropertyName("currencies")]
        public Dictionary<string, string>? Currencies { get; set; }

        [JsonPropertyName("carriers")]
        public Dictionary<string, string>? Carriers { get; set; }
    }

    public class LocationInfo
    {
        [JsonPropertyName("cityCode")]
        public string CityCode { get; set; } = string.Empty;

        [JsonPropertyName("countryCode")]
        public string CountryCode { get; set; } = string.Empty;
    }

    // Helper enums for type safety
    public static class FlightOfferConstants
    {
        public static class TravelerTypes
        {
            public const string Adult = "ADULT";
            public const string Child = "CHILD";
            public const string InfantWithSeat = "SEATED_INFANT";
            public const string InfantWithoutSeat = "HELD_INFANT";
            public const string Senior = "SENIOR";
            public const string Young = "YOUNG";
            public const string Student = "STUDENT";
        }

        public static class CabinClasses
        {
            public const string Economy = "ECONOMY";
            public const string PremiumEconomy = "PREMIUM_ECONOMY";
            public const string Business = "BUSINESS";
            public const string First = "FIRST";
        }

        public static class FareOptions
        {
            public const string Standard = "STANDARD";
            public const string IncludedCheckedBagsOnly = "INCLUDED_CHECKED_BAGS_ONLY";
        }

        public static class AmenityTypes
        {
            public const string Baggage = "BAGGAGE";
            public const string PreReservedSeat = "PRE_RESERVED_SEAT";
            public const string Meal = "MEAL";
            public const string BrandedFares = "BRANDED_FARES";
            public const string WiFi = "WIFI";
            public const string Entertainment = "ENTERTAINMENT";
        }

        public static class FeeTypes
        {
            public const string Supplier = "SUPPLIER";
            public const string Ticketing = "TICKETING";
            public const string FormOfPayment = "FORM_OF_PAYMENT";
        }
    }

    // Extension methods for easier data access
    public static class FlightOfferExtensions
    {
        public static decimal GetTotalPriceDecimal(this FlightPrice price)
        {
            return decimal.TryParse(price.Total, out var result) ? result : 0;
        }

        public static decimal GetBasePriceDecimal(this FlightPrice price)
        {
            return decimal.TryParse(price.Base, out var result) ? result : 0;
        }

        public static TimeSpan GetDurationTimeSpan(this FlightItinerary itinerary)
        {
            return TimeSpan.TryParse(itinerary.Duration.Replace("PT", "").Replace("H", ":").Replace("M", ""), out var result) ? result : TimeSpan.Zero;
        }

        public static DateTime GetDepartureDateTime(this FlightSegment segment)
        {
            return DateTime.TryParse(segment.Departure.At, out var result) ? result : DateTime.MinValue;
        }

        public static DateTime GetArrivalDateTime(this FlightSegment segment)
        {
            return DateTime.TryParse(segment.Arrival.At, out var result) ? result : DateTime.MinValue;
        }

        public static bool IsDirectFlight(this FlightItinerary itinerary)
        {
            return itinerary.Segments.Count == 1 && itinerary.Segments[0].NumberOfStops == 0;
        }

        public static int GetTotalStops(this FlightItinerary itinerary)
        {
            return itinerary.Segments.Sum(s => s.NumberOfStops) + Math.Max(0, itinerary.Segments.Count - 1);
        }

        public static string GetAirlineNames(this FlightOffer offer, FlightDictionaries? dictionaries)
        {
            if (dictionaries?.Carriers == null) return string.Join(", ", offer.ValidatingAirlineCodes);
            
            var airlineNames = offer.ValidatingAirlineCodes
                .Select(code => dictionaries.Carriers.TryGetValue(code, out var name) ? name : code)
                .Distinct();
            
            return string.Join(", ", airlineNames);
        }

        public static string GetRouteDescription(this FlightItinerary itinerary)
        {
            if (!itinerary.Segments.Any()) return "No segments";
            
            var route = new List<string> { itinerary.Segments.First().Departure.IataCode };
            route.AddRange(itinerary.Segments.Select(s => s.Arrival.IataCode));
            
            return string.Join(" → ", route);
        }
    }
}