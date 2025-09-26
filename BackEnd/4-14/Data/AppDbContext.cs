using Microsoft.EntityFrameworkCore;
using AllEnBackend.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace AllEnBackend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Vocabulary> Vocabularies { get; set; } = null!;
        public DbSet<Definition> Definitions { get; set; } = null!;
        public DbSet<Translation> Translations { get; set; } = null!;
        public DbSet<UserWord> UserWords { get; set; } = null!;
        public DbSet<Article> Articles { get; set; } = null!;
        public DbSet<ReadingQuestion> Questions { get; set; } = null!;
        public DbSet<Attempt> Attempts { get; set; } = null!;
        public DbSet<UserCoin> UserCoins { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<Advertisement> Advertisement { get; set; }
        public DbSet<UserInventory> UserInventories { get; set; }

        // 交友部分
        public DbSet<Friend> Friends { get; set; } = null!;

        // 排行榜部分
        public DbSet<UserRankingData> UserRankingData { get; set; } = null !;
        public DbSet<LeaderboardReward> LeaderboardRewards { get; set; } = null!;
        public DbSet<UserBestRanking> UserBestRanking { get; set; } = null!;
        public DbSet<UserRankHistory> UserRankHistory { get; set; } = null!;

        // 听力部分
        public DbSet<ListeningPaper> ListeningPapers { get; set; }
        public DbSet<ListeningSection> ListeningSections { get; set; }
        public DbSet<ListeningQuestion> ListeningQuestions { get; set; }
        public DbSet<ListeningOption> ListeningOptions { get; set; }
        public DbSet<ListeningPracticeRecord> ListeningPracticeRecords { get; set; } = null!;

        // 新增的学习计划相关表
        public DbSet<StudyPlan> StudyPlans { get; set; } = null!;
        public DbSet<UserStudyPlan> UserStudyPlans { get; set; } = null!;
        
        // 成就系统相关表
        public DbSet<Achievement> Achievements { get; set; } = null!;
        public DbSet<UserAchievement> UserAchievements { get; set; } = null!;

        // 闯关系统相关表
        public DbSet<Adventure> Adventures { get; set; }
        public DbSet<AdventureTreasure> AdventureTreasures { get; set; }
        public DbSet<UserAdventure> UserAdventures { get; set; }
        public DbSet<UserAdventureTreasure> UserAdventureTreasures { get; set; }

        // 游戏相关表
        public DbSet<Puzzle> Puzzle { get; set; }
        public DbSet<Clue> Clue { get; set; }

        // 用户学习记录
        public DbSet<UserLearningRecord> UserLearningRecords { get; set; } = null!;
        
        // 用户每日学习单词数量
        public DbSet<UserDailyWords> UserDailyWords { get; set; } = null!;

        // 帖子相关
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 创建一个自定义的字符串到List<string>的转换器
            var tagsConverter = new ValueConverter<List<string>, string>(
                // 将List<string>转换为字符串
                v => JsonSerializer.Serialize(v, new JsonSerializerOptions()),
                // 将字符串转换为List<string>
                v => SafeDeserializeList(v)
            );


            // 配置 User 表
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("USERS");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.UserName).HasColumnName("USERNAME");
                entity.Property(e => e.Password).HasColumnName("PASSWORD");
                entity.Property(e => e.Email).HasColumnName("EMAIL");
                entity.Property(e => e.Gender).HasColumnName("GENDER");
                entity.Property(e => e.DateOfBirth).HasColumnName("DATEOFBIRTH");
                entity.Property(e => e.PhoneNumber).HasColumnName("PHONENUMBER");
                entity.Property(e => e.Category).HasColumnName("CATEGORY");
                entity.Property(e => e.AvatarUrl).HasColumnName("AVATARURL");
            });

            // 配置 VOCABULARY 表
            modelBuilder.Entity<Vocabulary>(entity =>
            {
                entity.ToTable("VOCABULARY");
                entity.Property(v => v.Id).HasColumnName("ID");
                entity.Property(v => v.WordName).HasColumnName("WORD");
                entity.Property(v => v.Coverage).HasColumnName("COVERAGE");
            });

            // 配置 DEFINITION 表
            modelBuilder.Entity<Definition>(entity =>
            {
                entity.ToTable("DEFINITION");
                entity.Property(d => d.Id).HasColumnName("DEFINITION_ID");
                entity.Property(d => d.EnglishDefinition).HasColumnName("DEFINITION_TEXT");

                entity.HasOne(d => d.Vocabulary)
                    .WithMany(v => v.Definitions)
                    .HasForeignKey("WORD_ID")
                    .HasConstraintName("WORD_ID");
            });

            // 配置 TRANSLATION 表
            modelBuilder.Entity<Translation>(entity =>
            {
                entity.ToTable("TRANSLATION");
                entity.Property(t => t.Id).HasColumnName("TRANSLATION_ID");
                entity.Property(t => t.ChineseTranslation).HasColumnName("TRANSLATION_TEXT");

                entity.HasOne(t => t.Vocabulary)
                    .WithMany(v => v.Translations)
                    .HasForeignKey("WORD_ID")
                    .HasConstraintName("WORD_ID");
            });

            // 配置 USER_WORD 表
            modelBuilder.Entity<UserWord>(entity =>
            {
                entity.ToTable("USER_WORD");
                entity.HasKey(uw => new { uw.UserId, uw.WordId });

                entity.HasOne(uw => uw.Vocabulary)
                    .WithMany()
                    .HasForeignKey(uw => uw.WordId)
                    .HasConstraintName("FK_USER_WORD_VOCABULARY");
            });

            // 配置 READING_ARTICLES 表
            modelBuilder.Entity<Article>(entity =>
            {
                entity.ToTable("READING_ARTICLE");
                entity.HasKey(a => a.ArticleId);

                entity.Property(a => a.ArticleId).HasColumnName("ARTICLE_ID");
                entity.Property(a => a.CourseId).HasColumnName("COURSE_ID");
                entity.Property(a => a.Title).HasColumnName("TITLE").HasMaxLength(200);
                entity.Property(a => a.Content).HasColumnName("CONTENT").HasColumnType("CLOB");
                entity.Property(a => a.Difficulty).HasColumnName("DIFFICULTY");
                entity.Property(a => a.CoverImage).HasColumnName("COVER_IMAGE").HasMaxLength(200).IsRequired(false);
                entity.Property(a => a.Category).HasColumnName("CATEGORY").HasMaxLength(50).IsRequired(false);
                entity.Property(a => a.ReadingTime).HasColumnName("READING_TIME").IsRequired(false);
                entity.Property(a => a.WordCount).HasColumnName("WORD_COUNT").IsRequired(false);

                // 使用自定义转换器处理Tags
                entity.Property(a => a.Tags)
                      .HasColumnName("TAGS")
                      .HasConversion(tagsConverter);

                entity.Property(a => a.CreatedAt).HasColumnName("CREATED_AT").IsRequired(false);
                entity.Property(a => a.UpdatedAt).HasColumnName("UPDATED_AT").IsRequired(false);
            });

            // 配置 QUESTION 表
            modelBuilder.Entity<ReadingQuestion>(entity =>
            {
                entity.ToTable("QUESTION");
                entity.HasKey(q => q.Id);

                entity.Property(q => q.Id).HasColumnName("ID");
                entity.Property(q => q.ArticleId).HasColumnName("ARTICLEID");
                entity.Property(q => q.Seqo).HasColumnName("SEQO").HasMaxLength(20).IsRequired(false);
                entity.Property(q => q.Kind).HasColumnName("KIND").IsRequired(false);
                entity.Property(q => q.Stem).HasColumnName("STEM").HasColumnType("CLOB").IsRequired(false);
                entity.Property(q => q.Options).HasColumnName("OPTIONS").HasColumnType("CLOB").IsRequired(false);
                entity.Property(q => q.AnswerKey).HasColumnName("ANSWERKEY").HasColumnType("CLOB").IsRequired(false);
                entity.Property(q => q.Score).HasColumnName("SCORE").IsRequired(false);
                entity.Property(q => q.CreatedAt).HasColumnName("CREATEDAT").IsRequired(false);

                entity.HasOne(q => q.Article)
                    .WithMany()
                    .HasForeignKey(q => q.ArticleId)
                    .HasConstraintName("FK_QUESTION_ARTICLE")
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // 配置 ATTEMPT 表
            modelBuilder.Entity<Attempt>(entity =>
            {
                entity.ToTable("ATTEMPT");
                entity.HasKey(a => a.Id);

                entity.Property(a => a.Id)
                    .HasDefaultValueSql("ATTEMPT_SEQ.NEXTVAL")
                    .ValueGeneratedOnAdd();

                entity.Property(a => a.Id).HasColumnName("ID");
                entity.Property(a => a.UserId).HasColumnName("USERID").IsRequired(false);
                entity.Property(a => a.ArticleId).HasColumnName("ARTICLEID").IsRequired(false);
                entity.Property(a => a.StartTime).HasColumnName("STARTTIME").IsRequired(false);
                entity.Property(a => a.EndTime).HasColumnName("ENDTIME").IsRequired(false);
                entity.Property(a => a.TotalScore).HasColumnName("TOTALSCORE");
                entity.Property(a => a.Status).HasColumnName("STATUS").HasMaxLength(20).IsRequired(false);
                entity.Property(a => a.CreatedAt).HasColumnName("CREATEDAT").IsRequired(false);
                entity.Property(a => a.UpdatedAt).HasColumnName("UPDATEDAT").IsRequired(false);

                entity.HasOne(a => a.Article)
                    .WithMany()
                    .HasForeignKey(a => a.ArticleId)
                    .HasConstraintName("FK_ATTEMPT_ARTICLE")
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // 配置 PHONETIC 表
            modelBuilder.Entity<Phonetic>(entity =>
            {
                entity.ToTable("PHONETIC");
                entity.Property(p => p.Id).HasColumnName("PHONETIC_ID");
                entity.Property(p => p.PhoneticText).HasColumnName("PHONETIC_TEXT");

                entity.HasOne(p => p.Vocabulary)
                    .WithMany(v => v.Phonetics)
                    .HasForeignKey("WORD_ID")
                    .HasConstraintName("WORD_ID");
            });

            // 配置 EXAMPLE 表
            modelBuilder.Entity<Example>(entity =>
            {
                entity.ToTable("EXAMPLE");
                entity.Property(e => e.Id).HasColumnName("EXAMPLE_ID");
                entity.Property(e => e.ExampleText).HasColumnName("EXAMPLE_TEXT");

                entity.HasOne(e => e.Vocabulary)
                    .WithMany(v => v.Examples)
                    .HasForeignKey("WORD_ID")
                    .HasConstraintName("WORD_ID");
            });

            // 配置 USERCOIN 表
            modelBuilder.Entity<UserCoin>(entity =>
            {
                entity.ToTable("USERCOIN");
                entity.HasKey(u => u.Id);

                entity.Property(u => u.Id).HasColumnName("ID").HasMaxLength(50);
                entity.Property(u => u.UserId).HasColumnName("USERID").HasMaxLength(50);
                entity.Property(u => u.Coin).HasColumnName("COIN");
                entity.Property(u => u.LastSignDate).HasColumnName("LASTSIGNDATE");
                entity.Property(u => u.FirstSignDate).HasColumnName("FIRSTSIGNDATE");

                entity.HasOne<User>()  // 外键关联到 Users 表
                      .WithMany()
                      .HasForeignKey(u => u.UserId)
                      .HasConstraintName("FK_USERCOIN_USERID");
            });

            // 配置LISTENING_PRACTICE_RECORD表
            modelBuilder.Entity<ListeningPracticeRecord>(entity =>
            {
                entity.ToTable("LISTENING_PRACTICE_RECORD");
                entity.HasKey(r => r.Id);

                // 告诉 EF ID 是数据库自己生成
                entity.Property(r => r.Id)
                      .HasColumnName("ID")
                      .ValueGeneratedOnAdd();

                entity.Property(r => r.UserId)
                      .HasColumnName("USER_ID");
                entity.Property(r => r.ListeningPaperId)
                      .HasColumnName("LISTENING_PAPER_ID");
                entity.Property(r => r.CompletedAt)
                      .HasColumnName("COMPLETED_AT");
                entity.Property(r => r.Score)
                      .HasColumnName("SCORE");
            });

            // 配置 FRIENDS 表
            modelBuilder.Entity<Friend>(entity =>
            {
                entity.ToTable("FRIENDS");                  // 表名配置

                entity.HasKey(e => e.FriendId);             // 主键配置

                entity.Property(e => e.FriendId)            // 字段映射和配置
                    .HasColumnName("FRIENDS_ID")
                    .ValueGeneratedOnAdd();
                entity.Property(e => e.UserId)
                    .HasColumnName("USER_ID")
                    .HasMaxLength(50)
                    .IsRequired();
                entity.Property(e => e.FriendUserId)
                    .HasColumnName("FRIENDS_USER_ID")
                    .HasMaxLength(50)
                    .IsRequired();
                entity.Property(e => e.Status)
                    .HasColumnName("STATUS")
                    .IsRequired();
                entity.Property(e => e.CreatedAt)
                    .HasColumnName("CREATED_AT")
                    .HasColumnType("datetime");
                entity.Property(e => e.UpdateAt)
                    .HasColumnName("UPDATE_AT")
                    .HasColumnType("datetime");
            });

            // 配置 UserRankingData 表
            modelBuilder.Entity<UserRankingData>(entity =>
            {
                entity.ToTable("USER_RANKING_DATA");
                entity.HasKey(e => e.Id);

                // 字段映射和配置
                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedOnAdd()
                    .IsRequired();
                entity.Property(e => e.UserId)
                    .HasColumnName("USERID")
                    .HasMaxLength(50)
                    .IsRequired();
                entity.Property(e => e.Username)
                    .HasColumnName("USERNAME")
                    .HasMaxLength(30)
                    .IsRequired();
                entity.Property(e => e.Score)
                    .HasColumnName("SCORE");
                entity.Property(e => e.ActivityScore)
                    .HasColumnName("ACTIVITYSCORE");
                entity.Property(e => e.ReadingCount)
                    .HasColumnName("READINGCOUNT");
                entity.Property(e => e.WordCount)
                    .HasColumnName("WORDCOUNT");
                entity.Property(e => e.ListeningCount)
                    .HasColumnName("LISTENINGCOUNT");
                entity.Property(e => e.OralScore)
                    .HasColumnName("ORALSCORE");
                entity.Property(e => e.CurrentRankScore)
                    .HasColumnName("CURRENTRANKSCORE");
                entity.Property(e => e.CurrentRankActivity)
                    .HasColumnName("CURRENTRANKACTIVITY");
                entity.Property(e => e.LastRankScore)
                    .HasColumnName("LASTRANKSCORE");
                entity.Property(e => e.LastRankActivity)
                    .HasColumnName("LASTRANKACTIVITY");
                entity.Property(e => e.LastUpdated)
                    .HasColumnName("LASTUPDATED")
                    .HasColumnType("datetime");
                entity.Property(e => e.CreatedAt)
                    .HasColumnName("CREATEDAT")
                    .HasColumnType("datetime");
            });

            // 配置 LeaderboardRewards 表
            modelBuilder.Entity<LeaderboardReward>(entity =>
            {
                entity.ToTable("LEADERBOARDREWARDS");         // 表名配置

                entity.HasKey(e => e.Id);                     // 主键配置

                entity.Property(e => e.Id)                    // 字段映射和配置
                    .HasColumnName("ID")
                    .HasMaxLength(36)
                    .ValueGeneratedOnAdd()
                    .IsRequired();
                entity.Property(e => e.RankType)
                    .HasColumnName("RANKTYPE")
                    .HasMaxLength(20)
                    .IsRequired();
                entity.Property(e => e.TimeRange)
                    .HasColumnName("TIMERANGE")
                    .HasMaxLength(20)
                    .IsRequired();
                entity.Property(e => e.RankStart)
                    .HasColumnName("RANKSTART")
                    .IsRequired();
                entity.Property(e => e.RankEnd)
                    .HasColumnName("RANKEND")
                    .IsRequired();
                entity.Property(e => e.CoinReward)
                    .HasColumnName("COINREWARD")
                    .IsRequired();
                entity.Property(e => e.PointReward)
                    .HasColumnName("POINTREWARD")
                    .IsRequired();
                entity.Property(e => e.CreatedAt)
                    .HasColumnName("CREATEDAT")
                    .HasColumnType("datetime");
            });

            // 配置 UserRankHistory 表
            modelBuilder.Entity<UserRankHistory>(entity =>
            {
                entity.ToTable("USERRANKHISTORY");          // 表名配置
                entity.HasKey(e => e.Id);                   // 主键配置

                // 字段映射和配置
                entity.Property(e => e.Id)                    
                    .HasColumnName("ID")
                    .ValueGeneratedOnAdd()
                    .IsRequired();
                entity.Property(e => e.UserId)
                    .HasColumnName("USERID")
                    .HasMaxLength(50)
                    .IsRequired();
                entity.Property(e => e.UserName)
                    .HasColumnName("USERNAME")
                    .HasMaxLength(20)
                    .IsRequired();
                entity.Property(e => e.Score)
                    .HasColumnName("SCORE");
                entity.Property(e => e.Activity)
                    .HasColumnName("ACTIVITY");
                entity.Property(e => e.CreatedAt)
                    .HasColumnName("CREATEDAT")
                    .HasColumnType("datetime");
            });

            // 配置 UserBestRanking 表
            modelBuilder.Entity<UserBestRanking>(entity =>
            {
                entity.ToTable("USERBESTRANKING");            // 表名配置
                entity.HasKey(e => e.Id);                     // 主键配置

                // 字段映射和配置
                entity.Property(e => e.Id)                    
                    .HasColumnName("ID")
                    .ValueGeneratedOnAdd()
                    .IsRequired();
                entity.Property(e => e.UserId)
                    .HasColumnName("USERID")
                    .HasMaxLength(20)
                    .IsRequired();
                entity.Property(e => e.BestRankScore)
                    .HasColumnName("BESTRANKSCORE");
                entity.Property(e => e.BestRankActivity)
                    .HasColumnName("BESTRANKACTIVITY");
                entity.Property(e => e.BestScore)
                    .HasColumnName("BESTSCORE");
                entity.Property(e => e.BestActivityScore)
                    .HasColumnName("BESTACTIVITYSCORE");
                entity.Property(e => e.AchievedAt)
                    .HasColumnName("ACHIEVEDAT")
                    .HasColumnType("datetime");
                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("UPDATEDAT")
                    .HasColumnType("datetime");
                entity.Property(e => e.CreatedAt)
                    .HasColumnName("CREATEDAT")
                    .HasColumnType("datetime");
            });

            // 配置 STUDY_PLAN 表
            modelBuilder.Entity<StudyPlan>(entity =>
            {
                entity.ToTable("STUDY_PLAN");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedOnAdd(); // 确保使用Oracle序列

                entity.Property(e => e.UserId)
                    .HasColumnName("USER_ID")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.WordCount)
                    .HasColumnName("WORD_COUNT")
                    .HasDefaultValue(20);

                entity.Property(e => e.PlanType)
                    .HasColumnName("PLAN_TYPE")
                    .HasMaxLength(30);

                entity.Property(e => e.Title)
                    .HasColumnName("TITLE")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Duration)
                    .HasColumnName("DURATION")
                    .HasDefaultValue(1);

                // Oracle中布尔值通常用NUMBER(1)表示
                entity.Property(e => e.IsPublic)
                    .HasColumnName("IS_PUBLIC")
                    .HasDefaultValue(0)
                    .HasConversion<int>(); // 将bool转换为int

                entity.Property(e => e.ArticleCount)
                    .HasColumnName("ARTICLE_COUNT")
                    .HasDefaultValue(5);

                entity.Property(e => e.OralTime)
                    .HasColumnName("ORAL_TIME")
                    .HasDefaultValue(10);

                entity.Property(e => e.PlanListeningTime)
                    .HasColumnName("PLAN_LISTENING_TIME")
                    .HasDefaultValue(0)
                    .IsRequired();

            });

            // 配置 USER_STUDY_PLAN 表
            modelBuilder.Entity<UserStudyPlan>(entity =>
            {
                entity.ToTable("USER_STUDY_PLAN");
                entity.HasKey(e => new { e.UserId, e.PlanId });

                entity.Property(e => e.UserId)
                    .HasColumnName("USER_ID")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.PlanId)
                    .HasColumnName("PLAN_ID")
                    .IsRequired();

                entity.Property(e => e.StartDate)
                    .HasColumnName("START_DATE")
                    .HasColumnType("DATE")
                    .IsRequired();

                entity.Property(e => e.LearnedWordCount)
                    .HasColumnName("LEARNED_WORD_COUNT")
                    .HasDefaultValue(0)
                    .IsRequired();

                entity.Property(e => e.LearnedArticleCount)
                    .HasColumnName("LEARNED_ARTICLE_COUNT")
                    .HasDefaultValue(0)
                    .IsRequired();

                entity.Property(e => e.ListeningTime)
                    .HasColumnName("LISTENING_TIME")
                    .HasDefaultValue(0)
                    .IsRequired();

                entity.Property(e => e.LearnedOralTime)
                    .HasColumnName("LEARNED_ORAL_TIME")
                    .HasDefaultValue(0)
                    .IsRequired();

                entity.Property(e => e.Completed)
                    .HasColumnName("COMPLETED")
                    .HasDefaultValue(0)
                    .IsRequired();

                // 正确配置与 StudyPlan 的关系
                entity.HasOne(usp => usp.Plan)
                    .WithMany()
                    .HasForeignKey(usp => usp.PlanId)
                    .HasConstraintName("FK_USER_STUDY_PLAN_PLAN")
                    .OnDelete(DeleteBehavior.Cascade);

                // 配置与 User 的关系（如果需要）
                entity.HasOne(usp => usp.User)
                    .WithMany()
                    .HasForeignKey(usp => usp.UserId)
                    .HasConstraintName("FK_USER_STUDY_PLAN_USER")
                    .OnDelete(DeleteBehavior.Cascade);

            });

             // 配置 MATERIAL 表
            modelBuilder.Entity<Material>(entity =>
            {
                entity.ToTable("MATERIAL");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.MaterialType).HasColumnName("MATERIALTYPE");
                entity.Property(e => e.ExamType).HasColumnName("EXAMTYPE");
                entity.Property(e => e.SkillType).HasColumnName("SKILLTYPE");
                entity.Property(e => e.Price).HasColumnName("PRICE");
                entity.Property(e => e.UpdateDate).HasColumnName("UPDATEDATE");
                entity.Property(e => e.IsActive).HasColumnName("IS_ACTIVE");
                entity.Property(e => e.Description).HasColumnName("DESCRIPTION");
                entity.Property(e => e.Url).HasColumnName("URL");
                entity.Property(e => e.PreviewUrl).HasColumnName("PREVIEWURL");
            });

            // 配置 USER_INVENTORIES 表
            modelBuilder.Entity<UserInventory>(entity =>
            {
                entity.ToTable("USER_INVENTORIES");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.UserId).HasColumnName("USER_ID");
                entity.Property(e => e.MaterialId).HasColumnName("MATERIAL_ID");
                entity.Property(e => e.PurchaseDate).HasColumnName("PURCHASE_DATE");
                entity.Property(e => e.PurchasePrice).HasColumnName("PURCHASE_PRICE");
                entity.Property(e => e.OrderId).HasColumnName("ORDER_ID");
                entity.Property(e => e.IsActive).HasColumnName("IS_ACTIVE");
            });

            // 配置 Advertisement 表
            modelBuilder.Entity<Advertisement>(entity =>
            {
                entity.ToTable("ADVERTISEMENT");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.MediaUrl).HasColumnName("MEDIAURL");
                entity.Property(e => e.TargetId).HasColumnName("TARGETID");
                entity.Property(e => e.Context).HasColumnName("CONTEXT");
                entity.Property(e => e.Status).HasColumnName("STATUS");
                entity.Property(e => e.ClickCount).HasColumnName("CLICKCOUNT");
                entity.Property(e => e.CreateTime).HasColumnName("CREATETIME");
            });
            
            // 配置 ACHIEVEMENT 表
            modelBuilder.Entity<Achievement>(entity =>
            {
                entity.ToTable("ACHIEVEMENT");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Title)
                    .HasColumnName("TITLE")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Description)
                    .HasColumnName("DESCRIPTION")
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(e => e.CoinCount)
                    .HasColumnName("COIN_COUNT")
                    .HasDefaultValue(0);

                entity.Property(e => e.ArticleCount)
                    .HasColumnName("ARTICLE_COUNT")
                    .HasDefaultValue(0);

                entity.Property(e => e.WordCount)
                    .HasColumnName("WORD_COUNT")
                    .HasDefaultValue(0);

                entity.Property(e => e.OralTime)
                    .HasColumnName("ORAL_TIME")
                    .HasDefaultValue(0);

                entity.Property(e => e.ListeningTime)
                    .HasColumnName("LISTENING_TIME")
                    .HasDefaultValue(0);

                entity.Property(e => e.ArticlePerday)
                    .HasColumnName("ARTICLE_PERDAY")
                    .HasDefaultValue(0);

                entity.Property(e => e.WordPerday)
                    .HasColumnName("WORD_PERDAY")
                    .HasDefaultValue(0);

                entity.Property(e => e.OralPerday)
                    .HasColumnName("ORAL_PERDAY")
                    .HasDefaultValue(0);

                entity.Property(e => e.ListeningPerday)
                    .HasColumnName("LISTENING_PERDAY")
                    .HasDefaultValue(0);
            });

            // 配置 USER_ACHIEVEMENTS 表
            modelBuilder.Entity<UserAchievement>(entity =>
            {
                entity.ToTable("USER_ACHIEVEMENTS");
                entity.HasKey(e => new { e.UserId, e.AchievementId });

                entity.Property(e => e.UserId)
                    .HasColumnName("USER_ID")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.AchievementId)
                    .HasColumnName("ACHIEVEMENT_ID")
                    .IsRequired();

                entity.Property(e => e.HasGained)
                    .HasColumnName("HAS_GAINED")
                    .HasDefaultValue(0)
                    .IsRequired();

                entity.Property(e => e.GainDate)
                    .HasColumnName("GAIN_DATE");

                // 配置外键关系
                entity.HasOne(ua => ua.User)
                    .WithMany()
                    .HasForeignKey(ua => ua.UserId)
                    .HasConstraintName("FK_USER_ACHIEVEMENTS_USER")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ua => ua.Achievement)
                    .WithMany()
                    .HasForeignKey(ua => ua.AchievementId)
                    .HasConstraintName("FK_USER_ACHIEVEMENTS_ACHIEVEMENT")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // 配置 Adventure 表

            modelBuilder.Entity<Adventure>(entity =>
            {
                // 表名和Schema（如果需要）
                entity.ToTable("ADVENTURES"); // Oracle默认表名大写

                // 主键配置
                entity.HasKey(e => e.Id)
                    .HasName("PK_ADVENTURES");

                // 列映射
                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasColumnType("NUMBER(10)");

                entity.Property(e => e.LevelNumber)
                    .HasColumnName("LEVEL_NUMBER")
                    .HasColumnType("NUMBER(3)")
                    .IsRequired();

                entity.Property(e => e.Title)
                    .HasColumnName("TITLE")
                    .HasColumnType("VARCHAR2(100)")
                    .IsRequired();

                entity.Property(e => e.Description)
                    .HasColumnName("DESCRIPTION")
                    .HasColumnType("CLOB");

                entity.Property(e => e.Type)
                    .HasColumnName("TYPE")
                    .HasColumnType("VARCHAR2(20)")
                    .IsRequired()
                    .HasConversion<string>(); // 用于枚举转换

                entity.Property(e => e.Difficulty)
                    .HasColumnName("DIFFICULTY")
                    .HasColumnType("VARCHAR2(20)")
                    .IsRequired()
                    .HasConversion<string>();

                entity.Property(e => e.TargetType)
                    .HasColumnName("TARGET_TYPE")
                    .HasColumnType("VARCHAR2(50)")
                    .IsRequired();

                entity.Property(e => e.TargetValue)
                    .HasColumnName("TARGET_VALUE")
                    .HasColumnType("NUMBER(10)")
                    .IsRequired();

                entity.Property(e => e.RoutePath)
                    .HasColumnName("ROUTE_PATH")
                    .HasColumnType("VARCHAR2(100)")
                    .IsRequired();

                entity.Property(e => e.RouteParams)
                    .HasColumnName("ROUTE_PARAMS")
                    .HasColumnType("CLOB");

                entity.Property(e => e.Icon)
                    .HasColumnName("ICON")
                    .HasColumnType("VARCHAR2(50)");

                entity.Property(e => e.RewardExp)
                    .HasColumnName("REWARD_EXP")
                    .HasColumnType("NUMBER(10)")
                    .HasDefaultValue(0);

                entity.Property(e => e.RewardCoins)
                    .HasColumnName("REWARD_COINS")
                    .HasColumnType("NUMBER(10)")
                    .HasDefaultValue(0);

                entity.Property(e => e.IsActive)
                    .HasColumnName("IS_ACTIVE")
                    .HasColumnType("NUMBER(1)")
                    .HasDefaultValue(1)
                    .HasConversion<int>(); // 将bool转换为NUMBER(1)

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("CREATED_AT")
                    .HasColumnType("TIMESTAMP")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("UPDATED_AT")
                    .HasColumnType("TIMESTAMP")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // 唯一约束
                entity.HasIndex(e => e.LevelNumber)
                    .IsUnique()
                    .HasDatabaseName("UK_ADVENTURES_LEVEL_NUMBER");

                // 检查约束（EF Core 5.0+ 支持）
                entity.HasCheckConstraint("CK_ADVENTURES_TYPE", "TYPE IN ('vocabulary', 'reading', 'listening', 'speaking', 'comprehensive')");

                entity.HasCheckConstraint("CK_ADVENTURES_DIFFICULTY", "DIFFICULTY IN ('beginner', 'intermediate', 'advanced')");

                entity.HasCheckConstraint("CK_ADVENTURES_IS_ACTIVE", "IS_ACTIVE IN (0, 1)");
            });

            // 配置 AdventureTreasure 表
            modelBuilder.Entity<AdventureTreasure>(entity =>
            {
                entity.ToTable("ADVENTURE_TREASURES");

                entity.HasKey(e => e.Id)
                    .HasName("PK_ADVENTURE_TREASURES");

                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasColumnType("NUMBER(10)");

                entity.Property(e => e.LevelNumber)
                    .HasColumnName("LEVEL_NUMBER")
                    .HasColumnType("NUMBER(3)")
                    .IsRequired();

                entity.Property(e => e.Title)
                    .HasColumnName("TITLE")
                    .HasColumnType("VARCHAR2(100)")
                    .IsRequired();

                entity.Property(e => e.Description)
                    .HasColumnName("DESCRIPTION")
                    .HasColumnType("CLOB");

                entity.Property(e => e.Rewards)
                    .HasColumnName("REWARDS")
                    .HasColumnType("CLOB")
                    .IsRequired();

                entity.Property(e => e.Icon)
                    .HasColumnName("ICON")
                    .HasColumnType("VARCHAR2(50)");

                entity.Property(e => e.IsActive)
                    .HasColumnName("IS_ACTIVE")
                    .HasColumnType("NUMBER(1)")
                    .HasDefaultValue(1)
                    .HasConversion<int>();

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("CREATED_AT")
                    .HasColumnType("TIMESTAMP")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("UPDATED_AT")
                    .HasColumnType("TIMESTAMP")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // 唯一约束
                entity.HasIndex(e => e.LevelNumber)
                    .IsUnique()
                    .HasDatabaseName("UK_TREASURES_LEVEL_NUMBER");

                // 检查约束
                entity.HasCheckConstraint("CK_TREASURES_IS_ACTIVE", "IS_ACTIVE IN (0, 1)");
            });

            // 配置 UserAdventure 表
            modelBuilder.Entity<UserAdventure>(entity =>
            {
                entity.ToTable("USER_ADVENTURES");
                entity.Property(e => e.UserId).HasColumnName("USER_ID");
                entity.Property(e => e.AdventureId).HasColumnName("ADVENTURE_ID");
                entity.Property(e => e.Status).HasColumnName("STATUS");

                // 配置与 Adventure 的关系
                entity.HasOne(ua => ua.Adventure)
                    .WithMany()
                    .HasForeignKey(ua => ua.AdventureId)
                    .HasConstraintName("FK_USER_ADVENTURE_ADVENTURE");
            });

            // 配置 UserAdventureTreasure 表
            modelBuilder.Entity<UserAdventureTreasure>(entity =>
            {
                entity.ToTable("USER_ADVENTURE_TREASURE");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.UserId).HasColumnName("USER_ID");
                entity.Property(e => e.TreasureId).HasColumnName("TREASURE_ID");
                entity.Property(e => e.OpenedAt).HasColumnName("OPENED_AT");
                entity.Property(e => e.RewardsReceived).HasColumnName("REWARDS_RECEIVED");

                // 配置与 AdventureTreasure 的关系
                entity.HasOne(ut => ut.Treasure)
                    .WithMany()
                    .HasForeignKey(ut => ut.TreasureId)
                    .HasConstraintName("FK_USER_ADVENTURE_TREASURE_TREASURE");
            });

            // 配置 USER_LEARNING_RECORD 表
            modelBuilder.Entity<UserLearningRecord>(entity =>
            {
                entity.ToTable("USER_LEARNING_RECORD");
                entity.HasKey(e => e.UserId);

                entity.Property(e => e.UserId)
                    .HasColumnName("USER_ID")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.ArticleCount)
                    .HasColumnName("ARTICLE_COUNT")
                    .HasDefaultValue(0)
                    .IsRequired();

                entity.Property(e => e.WordCount)
                    .HasColumnName("WORD_COUNT")
                    .HasDefaultValue(0)
                    .IsRequired();

                entity.Property(e => e.OralTime)
                    .HasColumnName("ORAL_TIME")
                    .HasDefaultValue(0)
                    .IsRequired();

                entity.Property(e => e.ListeningTime)
                    .HasColumnName("LISTENING_TIME")
                    .HasDefaultValue(0)
                    .IsRequired();

                entity.Property(e => e.ArticlePerDay)
                    .HasColumnName("ARTICLE_PER_DAY")
                    .HasDefaultValue(0)
                    .IsRequired();

                entity.Property(e => e.WordPerDay)
                    .HasColumnName("WORD_PER_DAY")
                    .HasDefaultValue(0)
                    .IsRequired();

                entity.Property(e => e.OralPerDay)
                    .HasColumnName("ORAL_PER_DAY")
                    .HasDefaultValue(0)
                    .IsRequired();

                entity.Property(e => e.ListeningPerDay)
                    .HasColumnName("LISTENING_PER_DAY")
                    .HasDefaultValue(0)
                    .IsRequired();

                // 配置外键关系
                entity.HasOne(ulr => ulr.User)
                    .WithMany()
                    .HasForeignKey(ulr => ulr.UserId)
                    .HasConstraintName("FK_USER_LEARNING_RECORD_USER")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // 配置 Clue
            modelBuilder.Entity<Clue>(entity =>
            {
                entity.ToTable("CROSSWORD_CLUE");
                entity.HasKey(e => e.ClueId);

                entity.Property(e => e.ClueId)
                    .HasColumnName("CLUE_ID")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.PuzzleId)
                    .HasColumnName("PUZZLE_ID")
                    .HasDefaultValue(0)
                    .IsRequired();

                entity.Property(e => e.Direction)
                    .HasColumnName("DIRECTION")
                    .HasDefaultValue(0)
                    .IsRequired();

                entity.Property(e => e.ClueDescription)
                    .HasColumnName("CLUE_DESCRIPTION")
                    .HasDefaultValue(0)
                    .IsRequired();

                entity.Property(e => e.Answer)
                    .HasColumnName("ANSWER")
                    .HasDefaultValue(0)
                    .IsRequired();

                entity.Property(e => e.BeginH)
                    .HasColumnName("BEGIN_H")
                    .HasDefaultValue(0)
                    .IsRequired();

                entity.Property(e => e.BeginL)
                    .HasColumnName("BEGIN_L")
                    .HasDefaultValue(0)
                    .IsRequired();
            });

            // 配置 Puzzle
            modelBuilder.Entity<Puzzle>(entity =>
            {
                entity.ToTable("CROSSWORD_PUZZLE");
                entity.HasKey(e => e.PuzzleId);

                entity.Property(e => e.PuzzleId)
                    .HasColumnName("PUZZLE_ID")
                    .HasDefaultValue(0)
                    .IsRequired();

                entity.Property(e => e.Degree)
                    .HasColumnName("DEGREE")
                    .HasDefaultValue(0)
                    .IsRequired();
            });

            // 配置 UserDailyWords
            modelBuilder.Entity<UserDailyWords>(entity =>
            {
                entity.ToTable("USER_DAILY_WORDS");
                entity.HasKey(e => new { e.UserId, e.StudyDate });

                entity.Property(e => e.UserId)
                    .HasColumnName("USER_ID")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.StudyDate)
                    .HasColumnName("STUDY_DATE")
                    .IsRequired();

                entity.Property(e => e.WordCount)
                    .HasColumnName("WORD_COUNT")
                    .HasDefaultValue(0)
                    .IsRequired();

                // 配置外键关系
                entity.HasOne(udw => udw.User)
                    .WithMany()
                    .HasForeignKey(udw => udw.UserId)
                    .HasConstraintName("FK_USER_DAILY_WORDS_USER")
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // 配置 Posts 表
            modelBuilder.Entity<Post>(entity =>
            {
                entity.ToTable("POSTS");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID")
                      .ValueGeneratedOnAdd(); 
                entity.Property(e => e.UserId)
                      .HasColumnName("USERID")
                      .IsRequired()
                      .HasMaxLength(50);
                entity.Property(e => e.Content)
                      .HasColumnName("CONTENT");
                entity.Property(e => e.ImageUrl)
                      .HasColumnName("IMAGEURL")
                      .HasMaxLength(50);
                entity.Property(e => e.CreatedAt)
                      .HasColumnName("CREATEDAT")
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt)
                      .HasColumnName("UPDATEDAT")
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // 配置 Comments 表
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.ToTable("COMMENTS");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.PostId)
                      .HasColumnName("POSTID");
                entity.Property(e => e.UserId)
                      .HasColumnName("USERID")
                      .IsRequired()
                      .HasMaxLength(50);
                entity.Property(e => e.Content)
                      .HasColumnName("CONTENT")
                      .IsRequired();
                entity.Property(e => e.CreatedAt)
                      .HasColumnName("CREATEDAT")
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt)
                      .HasColumnName("UPDATEDAT")
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // 配置 Likes 表
            modelBuilder.Entity<Like>(entity =>
            {
                entity.ToTable("LIKES");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                      .HasColumnName("ID");
                entity.Property(e => e.PostId)
                      .HasColumnName("POSTID");
                entity.Property(e => e.UserId)
                     .HasColumnName("USERID")
                     .IsRequired()
                     .HasMaxLength(50);
                entity.Property(e => e.CreatedAt)
                      .HasColumnName("CREATEDAT")
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
        }

        // 安全反序列化方法
        private static List<string> SafeDeserializeList(string value)
        {
            if (string.IsNullOrEmpty(value))
                return new List<string>();

            try
            {
                return JsonSerializer.Deserialize<List<string>>(value, new JsonSerializerOptions()) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

    }
}