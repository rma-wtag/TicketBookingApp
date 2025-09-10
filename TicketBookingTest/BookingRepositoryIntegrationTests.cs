using FakeItEasy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketBookingApp.Dtos.BookingDtos;
using TicketBookingApp.Entities;
using TicketBookingApp.Models;
using TicketBookingApp.Services.BookingServices;

namespace TicketBookingTest
{
    public class BookingRepositoryIntegrationTests : IAsyncLifetime
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;
        private readonly BookingRepository _repository;

        public BookingRepositoryIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("BookingTestDb_" + Guid.NewGuid())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

            _context = new ApplicationDbContext(options);
            _cache = A.Fake<IDistributedCache>();

            _repository = new TestBookingRepository(_context, _cache);
        }

        [Fact]
        public async Task CreateNewBookingAsync_ShouldSucceed_WhenSeatsAreAvailable()
        {
            // Arrange
            var hall = new Hall { Name = "Integration Hall" };
            _context.Halls.Add(hall);

            var movie = new Movie { Title = "Test Movie", Duration = TimeSpan.FromHours(2) };
            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            var show = new Show
            {
                MovieId = movie.Id,
                HallId = hall.Id,
                Price = 200,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(2)
            };
            _context.Shows.Add(show);
            await _context.SaveChangesAsync();

            var seats = Enumerable.Range(1, 5)
                .Select(i => new Seat { SeatNumber = $"S{i:D2}", HallId = hall.Id })
                .ToList();

            _context.Seats.AddRange(seats);
            await _context.SaveChangesAsync();

            var dto = new CreateBookingDtos
            {
                UserId = 1,
                ShowId = show.Id,
                SelectedSeatIds = new List<int> { seats[0].Id, seats[1].Id }
            };

            // Act
            var booking = await _repository.CreateNewBookingAsync(dto);

            // Assert
            booking.Should().NotBeNull();
            booking!.BookingSeats.Should().HaveCount(2);
            booking.Payment.Should().NotBeNull();
            booking.Payment!.Amount.Should().Be(400);
        }

        [Fact]
        public async Task DeleteBookingAsync_ShouldRemoveBooking()
        {
            // Arrange
            var hall = new Hall { Name = "Delete Hall" };
            _context.Halls.Add(hall);
            var movie = new Movie { Title = "Delete Movie", Duration = TimeSpan.FromHours(2) };
            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            var show = new Show
            {
                MovieId = movie.Id,
                HallId = hall.Id,
                Price = 150,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(2)
            };
            _context.Shows.Add(show);
            await _context.SaveChangesAsync();

            var seat = new Seat { SeatNumber = "S01", HallId = hall.Id };
            _context.Seats.Add(seat);
            await _context.SaveChangesAsync();

            var booking = new Booking
            {
                UserId = 2,
                ShowId = show.Id,
                CreatedAt = DateTime.UtcNow,
                Payment = new Payment
                {
                    Amount = 150,
                    PaymentStatus = PaymentStatus.Processing,
                    DateTime = DateTime.Now
                }
            };
            booking.BookingSeats.Add(new BookingSeat { SeatId = seat.Id, ShowId = show.Id });
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // Act
            var deleted = await _repository.DeleteBookingAsync(booking.Id);

            // Assert
            deleted.Should().NotBeNull();
            var bookingInDb = await _context.Bookings.FindAsync(booking.Id);
            bookingInDb.Should().BeNull();
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            await _context.Database.EnsureDeletedAsync();
            await _context.DisposeAsync();
        }
    }
}
