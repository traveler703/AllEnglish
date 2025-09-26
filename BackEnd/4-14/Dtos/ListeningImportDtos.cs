using System.Collections.Generic;

namespace AllEnBackend.Dtos
{
    public class ListeningPaperImportDto
    {
        public string Level { get; set; } = null!;
        public int Year { get; set; }
        public int Session { get; set; }
        public string AudioUrl { get; set; } = null!;
        public List<ListeningSectionImportDto> Sections { get; set; } = new();
    }

    public class ListeningSectionImportDto
    {
        public int Order { get; set; }
        public List<ListeningQuestionImportDto> Questions { get; set; } = new();
    }

    public class ListeningQuestionImportDto
    {
        public int Order { get; set; }
        public string Stem { get; set; } = null!;
        public string CorrectOption { get; set; } = null!;
        public List<ListeningOptionImportDto> Options { get; set; } = new();
    }

    public class ListeningOptionImportDto
    {
        public string Label { get; set; } = null!;
        public string Content { get; set; } = null!;
    }
}
