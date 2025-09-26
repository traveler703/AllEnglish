using System;
using System.Collections.Generic;

namespace AllEnBackend.Dtos
{
    /// <summary>
    /// ѧϰ�ƻ�����DTO
    /// </summary>
    public class StudyPlanDto
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


    /// <summary>
    /// �û�ѧϰ�ƻ�DTO
    /// </summary>
    public class UserStudyPlanDto
    {
        public string UserId { get; set; }
        public int PlanId { get; set; }
        public DateTime StartDate { get; set; }
        public int LearnedWordCount { get; set; } = 0;
        public int LearnedArticleCount { get; set; } = 0;
        public int ListeningTime { get; set; } = 0;
        public int LearnedOralTime { get; set; } = 0;
        public int Completed { get; set; } = 0;
    }

    /// <summary>
    /// �����û�ѧϰ�ƻ���DTO
    /// </summary>
    public class CreateUserStudyPlanDto
    {
        public string UserId { get; set; }
        public int PlanId { get; set; }
        public DateTime StartDate { get; set; }
    }

    /// <summary>
    /// ѧϰ�ƻ�����DTO�������ƻ���Ϣ���û�����
    /// </summary>
    public class StudyPlanDetailDto
    {
        public StudyPlanDto PlanInfo { get; set; }
        public UserStudyPlanDto UserProgress { get; set; }
    }

    /// <summary>
    /// ѧϰ�ƻ��б�DTO
    /// </summary>
    public class StudyPlanListDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string PlanType { get; set; }
        public int Duration { get; set; }
        public bool IsPublic { get; set; }
        public DateTime? StartDate { get; set; }
        public int ProgressPercentage { get; set; }
    }

    public class CreateStudyPlanDto
    {
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
    public class UpdateUserStudyPlanDto
    {
        public string UserId { get; set; }
        public int PlanId { get; set; }
        public int? LearnedWordCount { get; set; }
        public int? LearnedArticleCount { get; set; }
        public int? ListeningTime { get; set; }
        public int? LearnedOralTime { get; set; }
    }

    public class UpdateStudyPlanDto
    {
        public string UserId { get; set; }
        public int? WordCount { get; set; }
        public string? PlanType { get; set; }
        public string? Title { get; set; }
        public int? Duration { get; set; }
        public bool? IsPublic { get; set; }
        public int? ArticleCount { get; set; }
        public int? OralTime { get; set; }
        public int? PlanListeningTime { get; set; }
    }

    public class StudyPlanFilterDto
    {
        public string UserId { get; set; } = "-1";
        public int WordCount { get; set; } = -1;
        public string PlanType { get; set; } = "-1";
        public int Duration { get; set; } = -1;
        public int ArticleCount { get; set; } = -1;
        public int OralTime { get; set; } = -1;
        public bool? IsPublic { get; set; }
    }

    public class UserStudyPlanDetailDto
    {
        public string UserId { get; set; }
        public int PlanId { get; set; }
        public DateTime StartDate { get; set; }
        public int LearnedWordCount { get; set; }
        public int LearnedArticleCount { get; set; }
        public int ListeningTime { get; set; }
        public int LearnedOralTime { get; set; }
        public int Completed { get; set; }
        public int PlanListeningTime { get; set; }

        // ѧϰ�ƻ�����
        public string PlanTitle { get; set; }
        public string PlanType { get; set; }
        public int WordCount { get; set; }
        public int Duration { get; set; }
        public bool IsPublic { get; set; }
        public int ArticleCount { get; set; }
        public int OralTime { get; set; }
    }
}