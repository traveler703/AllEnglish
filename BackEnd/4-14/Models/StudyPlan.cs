using System;
using System.Collections.Generic;

namespace AllEnBackend.Models
{
    public class StudyPlan
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int WordCount { get; set; } = 20;
        public string PlanType { get; set; }
        public string Title { get; set; }
        public int Duration { get; set; } = 1;
        public bool IsPublic { get; set; } = false;
        public int ArticleCount { get; set; } = 5;
        public int OralTime { get; set; } = 10;
        public int PlanListeningTime { get; set; } = 0;
    }
}