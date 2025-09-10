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
using TicketBookingApp.Dtos.MovieDtos;
using TicketBookingApp.Entities;
using TicketBookingApp.Models;
using TicketBookingApp.Repositories;
using TicketBookingApp.Services;

namespace TicketBookingTest
{
    public class MovieRepositoryTest
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;
        private readonly ILogService _logService;
        private readonly MovieRepository _repository;

        public MovieRepositoryTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("MovieDb_" + Guid.NewGuid())
                .Options;

            _context = new ApplicationDbContext(options);
            _cache = A.Fake<IDistributedCache>();
            _logService = A.Fake<ILogService>();
            _repository = new MovieRepository(_context, _cache, _logService);
        }

        [Fact]
        public async Task GetMovieByIdAsync_ShouldReturnMovieFromDb_WhenCacheIsEmpty()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);
            var movie = new Movie { Id = 1, Title = "Inception" };
            context.Movies.Add(movie);
            await context.SaveChangesAsync();

            var cache = A.Fake<IDistributedCache>(); // not used in this test
            var logService = A.Fake<ILogService>();
            var repository = new MovieRepository(context, cache, logService);

            // Act
            var result = await repository.GetMovieByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.Title.Should().Be("Inception");
        }

        [Fact]
        public async Task CreateMovieAsync_ShouldAddMovie_AndLog()
        {
            // Arrange
            var movie = new Movie { Title = "New Movie", Rating = 4, Duration = TimeSpan.FromHours(2) };

            // Act
            var result = await _repository.CreateMovieAsync(movie);

            // Assert
            result.Title.Should().Be("New Movie");
            A.CallTo(() => _logService.LogAsync("CREATE", "Movie", A<string>._))
                .MustHaveHappened();
        }

        [Fact]
        public async Task UpdateMovieAsync_ShouldUpdateValues_AndLog()
        {
            // Arrange
            var movie = new Movie { Title = "Old Movie", Description = "Old", Duration = TimeSpan.FromHours(1), Rating = 3 };
            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            var dto = new UpdateMovieDto { Description = "New", Duration = TimeSpan.FromHours(2), Rating = 5 };

            // Act
            var updated = await _repository.UpdateMovieAsync(movie.Id, dto);

            // Assert
            updated!.Description.Should().Be("New");
            updated.Rating.Should().Be(5);

            A.CallTo(() => _logService.LogAsync("UPDATE", "Movie", A<string>._))
                .MustHaveHappened();
        }

        [Fact]
        public async Task DeleteMovieAsync_ShouldRemoveMovie_AndLog()
        {
            // Arrange
            var movie = new Movie { Title = "To Delete", Duration = TimeSpan.FromHours(2), Rating = 7 };
            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            // Act
            var deleted = await _repository.DeleteMovieAsync(movie.Id);

            // Assert
            deleted.Should().NotBeNull();
            A.CallTo(() => _logService.LogAsync("DELETE", "Movie", A<string>._))
                .MustHaveHappened();
        }
    }
}
