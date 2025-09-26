namespace AllEnBackend.Dtos
{
    public class SubmitListeningPracticeDto
    {
        public int PaperId { get; set; }
        public List<QuestionAnswerDto> Answers { get; set; } = new();
    }
    public class QuestionAnswerDto
    {
        public int QuestionId { get; set; }
        public string Response { get; set; } = null!;
    }
}
