# Fly - Flight Booking Platform

A comprehensive flight booking and travel management platform built with ASP.NET Core MVC, featuring real-time flight search, hotel reservations, and personalized user experiences.

## 🚀 Features

### Flight Management
- **Multi-city Flight Search**: Book flights between 7 major world cities (New York, London, Tokyo, Dubai, Sydney, Cape Town, Singapore)
- **Real-time Availability**: Integration with Amadeus API for live flight data
- **Smart Routing**: Automatic stopover calculations and connection management
- **Class Selection**: Economy and Business class options
- **Seat Assignment**: Automatic seat allocation with unique numbering
- **Flexible Booking Window**: 6 months to 3 days before departure

### Passenger Services
- **Meal Preferences**: 7 dietary options including standard, vegetarian, vegan, halal, kosher, gluten-free, and destination-inspired meals
- **Dynamic Pricing**: Seasonal 30% price increases during peak periods (Christmas season for London/New York routes, summer for Tokyo/Singapore/Dubai)
- **Cancellation Policy**: Free cancellation up to 7 days before departure

### User Management
- **Secure Authentication**: Required user registration with password management
- **Booking History**: Complete tracking of past and current bookings with status updates
- **User Profiles**: Personalized experience with targeted promotions
- **Password Recovery**: Email-based password reset functionality

### Hotel Integration
- **Destination Hotels**: Minimum 3 hotel options per destination
- **Third-party APIs**: Integration with booking platforms for accommodation

### Communication
- **Email Notifications**: Automated booking confirmations and updates
- **Legal Compliance**: Email templates following current regulations

## 🛠️ Technical Stack

- **Framework**: ASP.NET Core MVC 8+
- **Frontend**: HTML5, CSS3, jQuery, Ajax
- **Database**: PostgreSQL
- **Architecture**: Layered architecture with Dependency Injection
- **APIs**: Amadeus API for flight data, hotel booking APIs
- **Email**: SMTP integration for notifications

## 📋 Prerequisites

- .NET 8 SDK or higher
- PostgreSQL database
- Amadeus API credentials
- Email service credentials (SMTP)

## ⚙️ Configuration

Create your configuration files with the following structure:

### Development Configuration
Create `appsettings.Development.json` with your development credentials.

### Production Configuration  
Update `appsettings.Production.json` with production values.

See `CONFIGURATION.md` for detailed setup instructions.

## 🚦 Getting Started

1. Clone the repository
2. Set up your configuration files (see Configuration section)
3. Start your PostgreSQL database
4. Run the application:
   ```bash
   dotnet run
   ```

## 🏗️ Project Structure

```
WebApplication1/
├── Controllers/          # MVC Controllers
├── Models/              # Data models
├── Views/               # Razor views
├── Services/            # Business logic services
├── DTOs/                # Data transfer objects
├── ViewModels/          # View model classes
├── Data/                # Database context
├── Migrations/          # EF Core migrations
└── wwwroot/            # Static files
```

## 🌟 Key Routes

- **Home**: Flight search and booking interface
- **Account**: User registration, login, profile management
- **Flights**: Flight search results and selection
- **Hotels**: Hotel booking interface
- **Booking**: Booking management and history

## 📧 Contact & Support

For questions about setup or usage, refer to the configuration documentation or check the application logs for troubleshooting information.