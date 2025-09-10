using FakeItEasy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketBookingApp.Entities;
using TicketBookingApp.Models;
using TicketBookingApp.Repositories;
using TicketBookingApp.Services;

namespace TicketBookingTest
{
    public class HallRepositoryTest
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;
        private readonly ILogService _logService;
        private readonly HallRepository _repository;

        public HallRepositoryTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("HallDb_" + Guid.NewGuid())
                .Options;

            _context = new ApplicationDbContext(options);
            _cache = A.Fake<IDistributedCache>();
            _logService = A.Fake<ILogService>();
            _repository = new HallRepository(_context, _cache, _logService);
        }

        [Fact]
        public async Task CreateHallAsync_ShouldAddHallAndSeats_AndLog()
        {
            // Arrange
            var hall = new Hall { Name = "New Hall" };

            // Act
            var result = await _repository.CreateHallAsync(hall);

            // Assert
            result.Seats.Should().HaveCount(40);
            result.Seats.First().SeatNumber.Should().Be("S01");

            A.CallTo(() => _logService.LogAsync("CREATE", "Hall", A<string>._))
                .MustHaveHappened();
        }
        [Fact]
        public async Task UpdateHallAsync_ShouldChangeName_AndLog()
        {
            // Arrange
            var hall = new Hall { Name = "Old Hall" };
            _context.Halls.Add(hall);
            await _context.SaveChangesAsync();

            // Act
            var updated = await _repository.UpdateHallAsync(hall.Id, new Hall { Name = "Updated Hall" });

            // Assert
            updated!.Name.Should().Be("Updated Hall");

            A.CallTo(() => _logService.LogAsync("UPDATE", "Hall", A<string>._))
                .MustHaveHappened();
        }

        [Fact]
        public async Task DeleteHallAsync_ShouldRemoveHall_AndLog()
        {
            // Arrange
            var hall = new Hall { Name = "Hall To Delete" };
            _context.Halls.Add(hall);
            await _context.SaveChangesAsync();

            // Act
            var deleted = await _repository.DeleteHallAsync(hall.Id);

            // Assert
            deleted.Should().NotBeNull();
            A.CallTo(() => _logService.LogAsync("DELETE", "Hall", A<string>._))
                .MustHaveHappened();
        }

        [Fact]
        public async Task GetHallByIdAsync_ShouldReturnHallFromDb_WhenCacheIsEmpty()
        {
            // Arrange
            var hall = new Hall { Name = "Hall A" };
            _context.Halls.Add(hall);
            await _context.SaveChangesAsync();

            // add seats manually since repo does Include(h => h.Seats)
            var seats = new List<Seat>();
            for (int i = 1; i <= 5; i++) // keep test lightweight, not all 40
            {
                seats.Add(new Seat { SeatNumber = $"S{i:D2}", HallId = hall.Id });
            }
            _context.Seats.AddRange(seats);
            await _context.SaveChangesAsync();

            // Fake cache → force miss
            A.CallTo(() => _cache.GetAsync(A<string>._, A<CancellationToken>._))
                .Returns(Task.FromResult<byte[]?>(null));

            // Act
            var result = await _repository.GetHallByIdAsync(hall.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Hall A");
            result.Seats.Should().HaveCount(5);
        }
    }
}
