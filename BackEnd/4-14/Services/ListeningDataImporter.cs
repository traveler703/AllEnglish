using System.Text.Json;
using AllEnBackend.Data;
using AllEnBackend.Models;
using AllEnBackend.Dtos;

public class ListeningDataImporter
{
    private readonly AppDbContext _db;
    public ListeningDataImporter(AppDbContext db) => _db = db;

    public async Task ImportFromJsonAsync(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);
        var paperDtos = JsonSerializer.Deserialize<List<ListeningPaperImportDto>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        if (paperDtos == null) return;

        foreach (var dto in paperDtos)
        {
            var paper = new ListeningPaper
            {
                Level = dto.Level,
                Year = dto.Year,
                Session = dto.Session.ToString(),
                AudioUrl = dto.AudioUrl
            };

            foreach (var secDto in dto.Sections)
            {
                var section = new ListeningSection
                {
                    Order = secDto.Order
                };

                foreach (var qDto in secDto.Questions)
                {
                    var question = new ListeningQuestion
                    {
                        Order = qDto.Order,
                        Stem = qDto.Stem,
                        CorrectOption = qDto.CorrectOption
                    };

                    foreach (var optDto in qDto.Options)
                    {
                        var option = new ListeningOption
                        {
                            Label = optDto.Label,
                            Content = optDto.Content
                        };
                        question.Options.Add(option);
                    }

                    section.Questions.Add(question);
                }

                paper.Sections.Add(section);
            }

            _db.ListeningPapers.Add(paper);
        }

        await _db.SaveChangesAsync();
    }
}
