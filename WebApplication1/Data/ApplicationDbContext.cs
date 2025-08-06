using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<City> Cities { get; set; }
        public DbSet<Models.Route> Routes { get; set; }
        public DbSet<Flight> Flights { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Meal> Meals { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingFlight> BookingFlights { get; set; }
        public DbSet<UserPreference> UserPreferences { get; set; }
        public DbSet<HotelBooking> HotelBookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure DateTime fields for MySQL
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    {
                        property.SetColumnType("datetime(6)");
                    }
                }
            }

            // City configuration
            modelBuilder.Entity<City>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CountryCode).IsRequired().HasMaxLength(3);
                entity.Property(e => e.AirportCode).IsRequired().HasMaxLength(3);
                entity.Property(e => e.TimeZone).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Latitude).HasPrecision(10, 7);
                entity.Property(e => e.Longitude).HasPrecision(10, 7);
                entity.HasIndex(e => e.AirportCode).IsUnique();
            });

            // Route configuration
            modelBuilder.Entity<Models.Route>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.BasePrice).HasPrecision(10, 2);
                
                entity.HasOne(e => e.OriginCity)
                    .WithMany(c => c.OriginRoutes)
                    .HasForeignKey(e => e.OriginCityId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.DestinationCity)
                    .WithMany(c => c.DestinationRoutes)
                    .HasForeignKey(e => e.DestinationCityId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.TransferCity)
                    .WithMany()
                    .HasForeignKey(e => e.TransferCityId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Flight configuration
            modelBuilder.Entity<Flight>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FlightNumber).IsRequired().HasMaxLength(10);
                entity.Property(e => e.EconomyPrice).HasPrecision(10, 2);
                entity.Property(e => e.BusinessPrice).HasPrecision(10, 2);

                entity.HasOne(e => e.Route)
                    .WithMany(r => r.Flights)
                    .HasForeignKey(e => e.RouteId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);

                entity.HasOne(e => e.OriginCity)
                    .WithMany(c => c.DepartureFlights)
                    .HasForeignKey(e => e.OriginCityId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.DestinationCity)
                    .WithMany(c => c.ArrivalFlights)
                    .HasForeignKey(e => e.DestinationCityId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Seat configuration
            modelBuilder.Entity<Seat>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SeatNumber).IsRequired().HasMaxLength(5);

                entity.HasOne(e => e.Flight)
                    .WithMany(f => f.Seats)
                    .HasForeignKey(e => e.FlightId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.FlightId, e.SeatNumber }).IsUnique();
            });

            // Meal configuration
            modelBuilder.Entity<Meal>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Price).HasPrecision(10, 2);
            });

            // Booking configuration
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.BookingReference).IsRequired().HasMaxLength(20);
                entity.Property(e => e.TotalAmount).HasPrecision(10, 2);
                entity.Property(e => e.PassengerFirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PassengerLastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PassportNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.ContactEmail).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ContactPhone).HasMaxLength(20);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Bookings)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.BookingReference).IsUnique();
            });

            // BookingFlight configuration
            modelBuilder.Entity<BookingFlight>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Price).HasPrecision(10, 2);

                entity.HasOne(e => e.Booking)
                    .WithMany(b => b.BookingFlights)
                    .HasForeignKey(e => e.BookingId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Flight)
                    .WithMany(f => f.BookingFlights)
                    .HasForeignKey(e => e.FlightId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Seat)
                    .WithMany(s => s.BookingFlights)
                    .HasForeignKey(e => e.SeatId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Meal)
                    .WithMany(m => m.BookingFlights)
                    .HasForeignKey(e => e.MealId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // UserPreference configuration
            modelBuilder.Entity<UserPreference>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PreferenceType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PreferenceValue).IsRequired().HasMaxLength(200);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.UserPreferences)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // HotelBooking configuration
            modelBuilder.Entity<HotelBooking>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.HotelName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.HotelAddress).IsRequired().HasMaxLength(500);
                entity.Property(e => e.RoomType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PricePerNight).HasPrecision(10, 2);
                entity.Property(e => e.TotalPrice).HasPrecision(10, 2);
                entity.Property(e => e.ExternalBookingReference).HasMaxLength(100);
                entity.Property(e => e.BookingProvider).IsRequired().HasMaxLength(50);

                entity.HasOne(e => e.Booking)
                    .WithMany(b => b.HotelBookings)
                    .HasForeignKey(e => e.BookingId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.City)
                    .WithMany()
                    .HasForeignKey(e => e.CityId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}