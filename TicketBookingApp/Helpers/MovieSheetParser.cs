using ClosedXML.Excel;
using CsvHelper;
using System.Globalization;
using TicketBookingApp.Dtos.MovieDtos;

namespace TicketBookingApp.Helpers
{
    public class MovieSheetParser : ISheetParser<CreateMovieDto>
    {
        public async Task<List<CreateMovieDto>> ParseCsvAsync(IFormFile file)
        {
            using var reader = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture));
            var records = csv.GetRecords<CreateMovieDto>().ToList();
            return await Task.FromResult(records);
        }

        public async Task<List<CreateMovieDto>> ParseExcelAsync(IFormFile file)
        {
            var movies = new List<CreateMovieDto>();

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RangeUsed().RowsUsed();

            foreach (var row in rows.Skip(1)) // Skip header row
            {
                movies.Add(new CreateMovieDto
                {
                    Title = row.Cell(1).GetString(),
                    Description = row.Cell(2).GetString(),
                    Rating = decimal.Parse(row.Cell(3).GetString()),
                    Duration = TimeSpan.Parse(row.Cell(4).GetString()),
                    PosterUrl = row.Cell(5).GetString()
                });
            }

            return movies;
        }
    }
}
