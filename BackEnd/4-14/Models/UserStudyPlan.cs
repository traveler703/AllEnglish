using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AllEnBackend.Models
{
    public class UserStudyPlan
    {
        public string UserId { get; set; }
        public int PlanId { get; set; }
        public DateTime StartDate { get; set; }
        public int LearnedWordCount { get; set; } = 0;
        public int LearnedArticleCount { get; set; } = 0;
        public int ListeningTime { get; set; } = 0;
        public int LearnedOralTime { get; set; } = 0;
        public int Completed { get; set; } = 0; // 新增Completed属性
        // 导航属性
        public StudyPlan Plan { get; set; }
        public User User { get; set; }
    }
}