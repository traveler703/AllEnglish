using System.Collections.Generic;

namespace AllEnBackend.Dtos
{
    public class ListeningPaperDto
    {
        public int Id { get; set; }
        public string Level { get; set; }
        public int Year { get; set; }
        public string Session { get; set; }
        public string AudioUrl { get; set; } = null!;
        public int SectionCount { get; set; }
    }

    public class SectionDto
    {
        public int Id { get; set; }
        public int Order { get; set; }
        public List<ListeningQuestionDto> Questions { get; set; } = new List<ListeningQuestionDto>();
    }

    public class ListeningQuestionDto
    {
        public int Id { get; set; }
        public int Order { get; set; }
        public string Stem { get; set; }
        public string CorrectOption { get; set; }
        public List<OptionDto> Options { get; set; } = new List<OptionDto>();
    }

    public class OptionDto
    {
        public string Label { get; set; }
        public string Content { get; set; }
    }
}
