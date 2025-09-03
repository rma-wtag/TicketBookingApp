using ClosedXML.Excel;
using CsvHelper;
using System.Globalization;
using TicketBookingApp.Dtos.ShowDtos;

namespace TicketBookingApp.Helpers
{
    public class ShowSheetParser : ISheetParser<CreateShowDto>
    {
        public async Task<List<CreateShowDto>> ParseCsvAsync(IFormFile file)
        {
            using var reader = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture));
            var records = csv.GetRecords<CreateShowDto>().ToList();
            return await Task.FromResult(records);
        }

        public async Task<List<CreateShowDto>> ParseExcelAsync(IFormFile file)
        {
            var shows = new List<CreateShowDto>();

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RangeUsed().RowsUsed();

            foreach (var row in rows.Skip(1)) // Skip header row
            {
                shows.Add(new CreateShowDto
                {
                    MovieId = int.Parse(row.Cell(1).GetString()),
                    HallId = int.Parse(row.Cell(2).GetString()),
                    StartTime = DateTime.Parse(row.Cell(3).GetString()),
                    EndTime = DateTime.Parse(row.Cell(4).GetString())
                });
            }

            return shows;
        }
    }
}
