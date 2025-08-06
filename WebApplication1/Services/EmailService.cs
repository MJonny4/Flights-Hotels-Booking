using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendBookingConfirmationAsync(Booking booking)
        {
            try
            {
                var subject = $"Booking Confirmation - {booking.BookingReference}";
                var htmlBody = GenerateBookingConfirmationHtml(booking);
                var textBody = GenerateBookingConfirmationText(booking);

                await SendEmailAsync(booking.ContactEmail, subject, htmlBody, textBody);
                _logger.LogInformation($"Booking confirmation email sent to {booking.ContactEmail} for booking {booking.BookingReference}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send booking confirmation email for booking {booking.BookingReference}");
                throw;
            }
        }

        public async Task SendBookingCancellationAsync(Booking booking)
        {
            try
            {
                var subject = $"Booking Cancellation - {booking.BookingReference}";
                var htmlBody = GenerateBookingCancellationHtml(booking);
                var textBody = GenerateBookingCancellationText(booking);

                await SendEmailAsync(booking.ContactEmail, subject, htmlBody, textBody);
                _logger.LogInformation($"Booking cancellation email sent to {booking.ContactEmail} for booking {booking.BookingReference}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send booking cancellation email for booking {booking.BookingReference}");
                throw;
            }
        }

        public async Task SendWelcomeEmailAsync(string email, string firstName)
        {
            try
            {
                var subject = "Welcome to FlightBooking Pro!";
                var htmlBody = GenerateWelcomeHtml(firstName);
                var textBody = GenerateWelcomeText(firstName);

                await SendEmailAsync(email, subject, htmlBody, textBody);
                _logger.LogInformation($"Welcome email sent to {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send welcome email to {email}");
                throw;
            }
        }

        private async Task SendEmailAsync(string toEmail, string subject, string htmlBody, string textBody)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("FlightBooking Pro", _configuration["EmailSettings:Username"]));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody,
                TextBody = textBody
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_configuration["EmailSettings:Host"], 
                int.Parse(_configuration["EmailSettings:Port"] ?? "587"), SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_configuration["EmailSettings:Username"], 
                _configuration["EmailSettings:Password"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        private string GenerateBookingConfirmationHtml(Booking booking)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #28a745; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f8f9fa; }}
        .booking-details {{ background-color: white; padding: 15px; margin: 15px 0; border-radius: 5px; }}
        .footer {{ background-color: #6c757d; color: white; padding: 15px; text-align: center; }}
        .reference {{ font-size: 24px; font-weight: bold; color: #007bff; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>✈️ Booking Confirmed!</h1>
            <p>Your flight has been successfully booked</p>
        </div>
        
        <div class=""content"">
            <h2>Hello {booking.PassengerFirstName},</h2>
            <p>Thank you for choosing FlightBooking Pro! Your booking has been confirmed.</p>
            
            <div class=""booking-details"">
                <h3>Booking Details</h3>
                <p><strong>Booking Reference:</strong> <span class=""reference"">{booking.BookingReference}</span></p>
                <p><strong>Passenger:</strong> {booking.PassengerFirstName} {booking.PassengerLastName}</p>
                <p><strong>Passport Number:</strong> {booking.PassportNumber}</p>
                <p><strong>Date of Birth:</strong> {booking.PassengerDateOfBirth:MMM dd, yyyy}</p>
                <p><strong>Total Amount:</strong> ${booking.TotalAmount}</p>
                <p><strong>Booking Date:</strong> {booking.BookingDate:MMM dd, yyyy 'at' HH:mm}</p>
            </div>
            
            <div class=""booking-details"">
                <h3>Important Information</h3>
                <ul>
                    <li><strong>Check-in:</strong> Online check-in opens 24 hours before departure</li>
                    <li><strong>Cancellation:</strong> Free cancellation up to 7 days before departure</li>
                    <li><strong>Documents:</strong> Ensure your passport is valid for at least 6 months</li>
                    <li><strong>Contact:</strong> For changes, contact our customer service</li>
                </ul>
            </div>
        </div>
        
        <div class=""footer"">
            <p>Thank you for flying with us!</p>
            <p>FlightBooking Pro - Your trusted travel partner</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateBookingConfirmationText(Booking booking)
        {
            return $@"
✈️ BOOKING CONFIRMED - FlightBooking Pro

Hello {booking.PassengerFirstName},

Thank you for choosing FlightBooking Pro! Your booking has been confirmed.

BOOKING DETAILS:
- Booking Reference: {booking.BookingReference}
- Passenger: {booking.PassengerFirstName} {booking.PassengerLastName}
- Passport Number: {booking.PassportNumber}
- Date of Birth: {booking.PassengerDateOfBirth:MMM dd, yyyy}
- Total Amount: ${booking.TotalAmount}
- Booking Date: {booking.BookingDate:MMM dd, yyyy 'at' HH:mm}

IMPORTANT INFORMATION:
- Check-in opens 24 hours before departure
- Free cancellation up to 7 days before departure
- Ensure your passport is valid for at least 6 months
- Contact customer service for any changes

Thank you for flying with us!
FlightBooking Pro - Your trusted travel partner
";
        }

        private string GenerateBookingCancellationHtml(Booking booking)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #dc3545; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f8f9fa; }}
        .booking-details {{ background-color: white; padding: 15px; margin: 15px 0; border-radius: 5px; }}
        .footer {{ background-color: #6c757d; color: white; padding: 15px; text-align: center; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>❌ Booking Cancelled</h1>
            <p>Your booking has been cancelled</p>
        </div>
        
        <div class=""content"">
            <h2>Hello {booking.PassengerFirstName},</h2>
            <p>Your booking has been successfully cancelled.</p>
            
            <div class=""booking-details"">
                <h3>Cancelled Booking Details</h3>
                <p><strong>Booking Reference:</strong> {booking.BookingReference}</p>
                <p><strong>Passenger:</strong> {booking.PassengerFirstName} {booking.PassengerLastName}</p>
                <p><strong>Amount Refunded:</strong> ${booking.TotalAmount}</p>
                <p><strong>Cancellation Date:</strong> {DateTime.Now:MMM dd, yyyy 'at' HH:mm}</p>
            </div>
            
            <p>Your refund will be processed within 5-7 business days to your original payment method.</p>
        </div>
        
        <div class=""footer"">
            <p>We hope to serve you again soon!</p>
            <p>FlightBooking Pro - Your trusted travel partner</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateBookingCancellationText(Booking booking)
        {
            return $@"
❌ BOOKING CANCELLED - FlightBooking Pro

Hello {booking.PassengerFirstName},

Your booking has been successfully cancelled.

CANCELLED BOOKING DETAILS:
- Booking Reference: {booking.BookingReference}
- Passenger: {booking.PassengerFirstName} {booking.PassengerLastName}
- Amount Refunded: ${booking.TotalAmount}
- Cancellation Date: {DateTime.Now:MMM dd, yyyy 'at' HH:mm}

Your refund will be processed within 5-7 business days to your original payment method.

We hope to serve you again soon!
FlightBooking Pro - Your trusted travel partner
";
        }

        private string GenerateWelcomeHtml(string firstName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f8f9fa; }}
        .footer {{ background-color: #6c757d; color: white; padding: 15px; text-align: center; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>🎉 Welcome to FlightBooking Pro!</h1>
            <p>Your journey begins here</p>
        </div>
        
        <div class=""content"">
            <h2>Hello {firstName},</h2>
            <p>Welcome to FlightBooking Pro! We're excited to have you join our community of travelers.</p>
            
            <p>With your new account, you can:</p>
            <ul>
                <li>Search and book flights to 7 amazing destinations</li>
                <li>Manage your bookings and travel history</li>
                <li>Select your preferred meals and seats</li>
                <li>Enjoy seasonal promotions and special offers</li>
            </ul>
            
            <p>Start your next adventure by searching for flights on our platform!</p>
        </div>
        
        <div class=""footer"">
            <p>Happy travels!</p>
            <p>FlightBooking Pro - Your trusted travel partner</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateWelcomeText(string firstName)
        {
            return $@"
🎉 WELCOME TO FLIGHTBOOKING PRO!

Hello {firstName},

Welcome to FlightBooking Pro! We're excited to have you join our community of travelers.

With your new account, you can:
- Search and book flights to 7 amazing destinations
- Manage your bookings and travel history
- Select your preferred meals and seats
- Enjoy seasonal promotions and special offers

Start your next adventure by searching for flights on our platform!

Happy travels!
FlightBooking Pro - Your trusted travel partner
";
        }
    }
}