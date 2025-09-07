using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout.Borders;
using iText.Layout.Properties;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Geom;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketBookingApp.Dtos.BookingDtos;
using TicketBookingApp.Entities;
using TicketBookingApp.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace TicketBookingApp.Services.BookingServices
{
    public class BookingRepository : IBookingRepository
    {
        public readonly ApplicationDbContext _context;

        public BookingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Booking>> GetAllBookingAsync()
        {
            return await _context.Bookings.Include(b => b.Payment)
                                            .ToListAsync();
        }

        public async Task<Booking?> GetBookingByIdAsync(int id){
            var booking = await _context.Bookings.Include(b => b.Payment)
                                                 .FirstOrDefaultAsync(x => x.Id == id);

            if (booking == null) return null;
            return booking;
        }

        // Repository Method

        // Add these using statements at the top of your file:
        // using iText.IO.Font.Constants;
        // using iText.Kernel.Colors;
        // using iText.Kernel.Font;
        // using iText.Kernel.Pdf;
        // using iText.Layout;
        // using iText.Layout.Borders;
        // using iText.Layout.Element;
        // using iText.Layout.Properties;

        public async Task<(byte[] pdfBytes, string fileName)?> GenerateTicketByBookingIdAsync(int id)
        {
            var booking = await _context.Bookings
                            .Include(b => b.User)
                            .Include(b => b.Show)
                                .ThenInclude(s => s.Movie)
                            .Include(b => b.Show)
                                .ThenInclude(s => s.Hall)
                            .Include(b => b.BookingSeats)
                                .ThenInclude(bs => bs.Seat)
                            .Include(b => b.Payment)
                            .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null || !booking.IsCompleted)
            {
                return null;
            }

            byte[] pdfBytes;
            using (var memoryStream = new MemoryStream())
            {
                var writer = new PdfWriter(memoryStream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf, PageSize.A4);

                // Reduce margins to maximize space
                document.SetMargins(20, 30, 20, 30);

                try
                {
                    // Define modern color palette
                    var primaryColor = new DeviceRgb(26, 35, 126);      // Deep blue
                    var accentColor = new DeviceRgb(255, 107, 107);     // Coral red
                    var darkGray = new DeviceRgb(45, 55, 72);           // Dark gray
                    var lightGray = new DeviceRgb(247, 250, 252);       // Very light gray
                    var mediumGray = new DeviceRgb(160, 174, 192);      // Medium gray

                    // Set up fonts
                    var titleFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                    var headerFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                    var regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                    // Create main ticket container - compact version
                    var mainContainer = new Table(1)
                        .SetWidth(UnitValue.CreatePercentValue(100))
                        .SetBackgroundColor(ColorConstants.WHITE)
                        .SetBorder(new SolidBorder(ColorConstants.BLACK, 2));

                    // Header section - more compact
                    var headerTable = new Table(1)
                        .SetWidth(UnitValue.CreatePercentValue(100))
                        .SetBackgroundColor(primaryColor)
                        .SetBorder(Border.NO_BORDER);

                    var headerCell = new Cell()
                        .SetBorder(Border.NO_BORDER)
                        .SetPadding(15) // Reduced from 25
                        .SetTextAlignment(TextAlignment.CENTER);

                    // Cinema logo/title - smaller
                    var cinemaTitle = new Paragraph("🎬 CINEMAX")
                        .SetFont(titleFont)
                        .SetFontSize(22) // Reduced from 28
                        .SetFontColor(ColorConstants.WHITE)
                        .SetMarginBottom(3); // Reduced from 5

                    var ticketSubtitle = new Paragraph("MOVIE TICKET")
                        .SetFont(regularFont)
                        .SetFontSize(12) // Reduced from 14
                        .SetFontColor(ColorConstants.WHITE);

                    headerCell.Add(cinemaTitle);
                    headerCell.Add(ticketSubtitle);
                    headerTable.AddCell(headerCell);

                    // Booking reference bar - more compact
                    var refTable = new Table(new float[] { 1, 1 })
                        .SetWidth(UnitValue.CreatePercentValue(100))
                        .SetBackgroundColor(accentColor)
                        .SetBorder(Border.NO_BORDER);

                    var bookingRef = new Cell()
                        .Add(new Paragraph($"#{booking.Id:D6}")
                            .SetFont(headerFont)
                            .SetFontSize(11) // Reduced from 12
                            .SetFontColor(ColorConstants.WHITE))
                        .SetBorder(Border.NO_BORDER)
                        .SetPadding(8) // Reduced from 12
                        .SetTextAlignment(TextAlignment.LEFT);

                    var bookingDate = new Cell()
                        .Add(new Paragraph($"{booking.CreatedAt:MMM dd, yyyy}")
                            .SetFont(regularFont)
                            .SetFontSize(11) // Reduced from 12
                            .SetFontColor(ColorConstants.WHITE))
                        .SetBorder(Border.NO_BORDER)
                        .SetPadding(8) // Reduced from 12
                        .SetTextAlignment(TextAlignment.RIGHT);

                    refTable.AddCell(bookingRef);
                    refTable.AddCell(bookingDate);

                    // Movie title - more compact
                    var movieTitle = booking.Show.Movie?.Title ?? "Movie Title Not Available";
                    var movieTitlePara = new Paragraph(movieTitle)
                        .SetFont(titleFont)
                        .SetFontSize(18) // Reduced from 24
                        .SetFontColor(darkGray)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMargin(8); // Reduced margins

                    // Create a 3x2 grid for all details to save space
                    var detailsGrid = new Table(new float[] { 1, 1, 1 })
                        .SetWidth(UnitValue.CreatePercentValue(100))
                        .SetBorder(Border.NO_BORDER)
                        .SetMargin(5);

                    // Show details
                    var showInfo = $"{booking.Show.StartTime:MMM dd}\n{booking.Show.StartTime:HH:mm}-{booking.Show.EndTime:HH:mm}";
                    var showCard = CreateCompactInfoCard("🎭 SHOW", showInfo, headerFont, regularFont, lightGray, darkGray);

                    // Hall details
                    var hallName = booking.Show.Hall?.Name ?? $"Hall {booking.Show.HallId}";
                    var hallCard = CreateCompactInfoCard("🏛️ HALL", hallName, headerFont, regularFont, lightGray, darkGray);

                    // Seat details
                    var seatNumbers = string.Join(", ", booking.BookingSeats.Select(bs =>
                        bs.Seat?.SeatNumber ?? $"Seat {bs.SeatId}"));
                    var seatsCard = CreateCompactInfoCard("🪑 SEATS", seatNumbers, headerFont, regularFont, lightGray, darkGray);

                    detailsGrid.AddCell(showCard);
                    detailsGrid.AddCell(hallCard);
                    detailsGrid.AddCell(seatsCard);

                    // Second row for customer and payment
                    var customerCard = CreateCompactInfoCard("👤 CUSTOMER", booking.User.Username, headerFont, regularFont, lightGray, darkGray);
                    var paymentCard = CreateCompactInfoCard("💳 AMOUNT", $"${booking.Payment.Amount:F2}", headerFont, regularFont, lightGray, accentColor);

                    // QR Code info - compact
                    var qrCard = CreateCompactInfoCard("📱 QR", $"#{booking.Id}", headerFont, regularFont, lightGray, darkGray);

                    detailsGrid.AddCell(customerCard);
                    detailsGrid.AddCell(paymentCard);
                    detailsGrid.AddCell(qrCard);

                    // Footer - very compact
                    var footerSection = new Table(1)
                        .SetWidth(UnitValue.CreatePercentValue(100))
                        .SetBorder(Border.NO_BORDER)
                        .SetMargin(5);

                    var footerCell = new Cell()
                        .SetBorder(Border.NO_BORDER)
                        .SetPadding(10) // Reduced from 20
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetBackgroundColor(new DeviceRgb(249, 250, 251));

                    var terms = new Paragraph("Arrive 15 min early • No refunds • Valid for single use")
                        .SetFont(regularFont)
                        .SetFontSize(8) // Reduced from 9
                        .SetFontColor(mediumGray)
                        .SetMarginBottom(5); // Reduced from 10

                    var contact = new Paragraph("www.cinemax.com • +1-800-CINEMA")
                        .SetFont(regularFont)
                        .SetFontSize(7) // Reduced from 8
                        .SetFontColor(mediumGray);

                    footerCell.Add(terms);
                    footerCell.Add(contact);
                    footerSection.AddCell(footerCell);

                    // Assemble everything in the main container
                    var mainCell = new Cell()
                        .SetBorder(Border.NO_BORDER)
                        .SetPadding(10); // Reduced from default

                    mainCell.Add(headerTable);
                    mainCell.Add(refTable);
                    mainCell.Add(movieTitlePara);
                    mainCell.Add(detailsGrid);
                    mainCell.Add(footerSection);

                    mainContainer.AddCell(mainCell);
                    document.Add(mainContainer);
                }
                finally
                {
                    document.Close();
                    pdf.Close();
                    writer.Close();
                }

                pdfBytes = memoryStream.ToArray();
            }

            var fileName = $"MovieTicket_{booking.Id}_{booking.CreatedAt:yyyyMMdd}.pdf";
            return (pdfBytes, fileName);
        }

        // Helper method to create compact styled info cards
        private Cell CreateCompactInfoCard(string title, string content, PdfFont headerFont, PdfFont regularFont,
            DeviceRgb backgroundColor, DeviceRgb textColor)
        {
            var card = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(6) // Reduced from 8
                .SetMargin(1) // Reduced from 2
                .SetBackgroundColor(backgroundColor)
                .SetTextAlignment(TextAlignment.CENTER);

            card.Add(new Paragraph(title)
                .SetFont(headerFont)
                .SetFontSize(8) // Reduced from 9
                .SetFontColor(new DeviceRgb(107, 114, 128))
                .SetMarginBottom(2)); // Reduced from 4

            card.Add(new Paragraph(content)
                .SetFont(regularFont)
                .SetFontSize(10) // Reduced from 11
                .SetFontColor(textColor));

            return card;
        }


        public async Task<IEnumerable<Seat>?> GetAvailableSeatsAsync(int showId)
        {
            var hallId = await _context.Shows.Where(sh => sh.Id == showId)
                                        .Select(sh => sh.HallId)
                                        .FirstOrDefaultAsync();
            if (hallId == 0) return null;

            var availableSeats = await _context.Seats
                                        .Where(s=> s.HallId == hallId &&  !_context.BookingSeats
                                        .Any(bs => bs.SeatId == s.Id && bs.Booking!.ShowId == showId))
                                        .ToListAsync();
            return availableSeats;
        }

        public async Task<Booking?> CreateNewBookingAsync(CreateBookingDtos createBookingDtos)
        {
            var selectedIds = createBookingDtos.SelectedSeatIds.Distinct().ToList();
            if (selectedIds.Count == 0) return null;
            //if (selectedIds.Count > 4) return null; // need to handle, from user pov

            var availableSeats = await GetAvailableSeatsAsync(createBookingDtos.ShowId);
            var availableSeatIds = availableSeats!.Select(s => s.Id);

            if (!selectedIds.All(id => availableSeatIds.Contains(id)))
                return null;

            using var tx = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

            try
            {
                var takenNow = await _context.BookingSeats
                    .Where(bs => bs.ShowId == createBookingDtos.ShowId && selectedIds.Contains(bs.SeatId))
                    .Select(bs => bs.SeatId)
                    .ToListAsync();

                if (takenNow.Any())
                {
                    await tx.RollbackAsync();
                    return null;
                }

                var show = await _context.Shows.FirstOrDefaultAsync(s => s.Id == createBookingDtos.ShowId);

                var booking = new Booking
                {
                    UserId = createBookingDtos.UserId,
                    ShowId = createBookingDtos.ShowId,
                    CreatedAt = DateTime.UtcNow,
                    IsCompleted = false,
                    Payment = new Payment
                    {
                        Amount = (show!.Price * selectedIds.Count),
                        PaymentStatus = PaymentStatus.Processing,
                        DateTime = DateTime.UtcNow
                    }
                };

                foreach (var seatId in selectedIds)
                {
                    booking.BookingSeats.Add(new BookingSeat
                    {
                        SeatId = seatId,
                        ShowId = createBookingDtos.ShowId
                    });
                }

                _context.Bookings.Add(booking);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return booking;
            }
            catch (DbUpdateException)
            {
                await tx.RollbackAsync();
                return null;
            }
            catch (Exception)
            {
                await tx.RollbackAsync();
                throw;
            }
        }
        public async Task<Booking?> DeleteBookingAsync(int id) { 
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id);
            if (booking == null) { return null; }

            _context.Remove(booking);
            await _context.SaveChangesAsync();
            return booking;
        }
    }
}
