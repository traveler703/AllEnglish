using System.Data;
using System.Runtime.InteropServices.Marshalling;

namespace AllEnBackend.Dtos
{
    // 用户提交答案
    public class SubmitAnswerDto
    {
        public string UserId { get; set; }
        public int ArticleID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<UserAnswer> Answers { get; set; }
    }

    public class UserAnswer
    {
        public int QuestionId { get; set; }
        public string UserResponse { get; set; }
    }

    public class AnswerResultDto
    {
        public int TotalScore { get; set; }                 // 本次答题的总分
        public int MaxPossibleScore { get; set; }           // 阅读练习最大分数
        public double Percentage { get; set; }              // 得分比率
        public List<QuestionResultDto> QuestionResults { get; set; }
    }

    public class QuestionResultDto
    {
        public int QuestionId { get; set; }
        public bool IsCorrect { get; set; }
        public int Score { get; set; }

        public string CorrectAnswer { get; set; }
    }
}
