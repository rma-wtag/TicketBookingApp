using FakeItEasy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
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
    public class ShowRepositoryTest
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;
        private readonly ILogService _logService;
        private readonly ShowRepository _repository;

        public ShowRepositoryTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("ShowDb_" + Guid.NewGuid())
                .Options;

            _context = new ApplicationDbContext(options);
            _cache = A.Fake<IDistributedCache>();
            _logService = A.Fake<ILogService>();
            _repository = new ShowRepository(_context, _cache, _logService);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnShowFromDb_WhenCacheIsEmpty()
        {
            // Arrange: In-memory EF
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);

            // Related entities first
            var movie = new Movie { Id = 1, Title = "Inception" };
            var hall = new Hall { Id = 1, Name = "Main Hall" };
            context.Movies.Add(movie);
            context.Halls.Add(hall);
            await context.SaveChangesAsync();

            // Now the Show (pointing to existing Movie & Hall)
            var show = new Show
            {
                Id = 1,
                MovieId = movie.Id,
                HallId = hall.Id,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(2)
            };
            context.Shows.Add(show);
            await context.SaveChangesAsync();

            // Fake cache: return null for GetAsync
            var cache = A.Fake<IDistributedCache>();
            A.CallTo(() => cache.GetAsync(A<string>._, A<CancellationToken>._))
                .Returns(Task.FromResult<byte[]?>(null));

            var logService = A.Fake<ILogService>();
            var repository = new ShowRepository(context, cache, logService);

            // Act
            var result = await repository.GetByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.MovieId.Should().Be(1);
            result.HallId.Should().Be(1);
        }

        [Fact]
        public async Task CreateNewShowAsync_ShouldSetEndTime_AndLog()
        {
            // Arrange
            var movie = new Movie { Id = 1, Title = "Matrix", Duration = TimeSpan.FromHours(2) };
            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            var show = new Show { MovieId = 1, HallId = 1, StartTime = DateTime.Now, Price = 100 };

            // Act
            var result = await _repository.CreateNewShowAsync(show);

            // Assert
            result.Should().NotBeNull();
            result!.EndTime.Should().Be(show.StartTime + movie.Duration);

            A.CallTo(() => _logService.LogAsync("CREATE", "Show", A<string>._))
                .MustHaveHappened();
        }

        [Fact]
        public async Task UpdateShowAsync_ShouldChangeValues_AndLog()
        {
            // Arrange
            var show = new Show
            {
                MovieId = 1,
                HallId = 1,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(2),
                Price = 50
            };
            _context.Shows.Add(show);
            await _context.SaveChangesAsync();

            // Act
            show.Price = 75;
            var updated = await _repository.UpdateShowAsync(show.Id, show);

            // Assert
            updated!.Price.Should().Be(75);

            A.CallTo(() => _logService.LogAsync("UPDATE", "Show", A<string>._))
                .MustHaveHappened();
        }

        [Fact]
        public async Task DeleteShowAsync_ShouldRemoveShow_AndLog()
        {
            // Arrange
            var show = new Show
            {
                MovieId = 1,
                HallId = 1,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(2),
                Price = 100
            };
            _context.Shows.Add(show);
            await _context.SaveChangesAsync();

            // Act
            var deleted = await _repository.DeleteShowAsync(show.Id);

            // Assert
            deleted.Should().NotBeNull();
            A.CallTo(() => _logService.LogAsync("DELETE", "Show", A<string>._))
                .MustHaveHappened();
        }
    }
}
