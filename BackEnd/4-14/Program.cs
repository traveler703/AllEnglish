using Oracle.ManagedDataAccess.Client;
using AllEnBackend.Repositories;
using AllEnBackend.Services;
using AllEnBackend.Data;
using Microsoft.EntityFrameworkCore;
using AllEnBackend.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AllEnBackend.Services.Implementations;
using AllEnBackend.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// 读取配置
var jwtSection = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSection["SecretKey"];
var issuer = jwtSection["Issuer"];
var audience = jwtSection["Audience"];

// 添加 JWT 验证
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(5)
    };
});


// 配置 Kestrel，明确监听 HTTPS 端口 7071
if (builder.Environment.IsDevelopment())
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(7071, lo => lo.UseHttps());
    });
}

// 注册 CORS 策略（允许所有来源，可根据实际需求限制）
builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowAll", policy =>
  {
      policy.AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod();
  });
});

// 注册控制器服务及 Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 注册依赖（用户管理相关）
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAdminService, AdminService>();

// 注册依赖（单词管理相关）
builder.Services.AddDbContext<AppDbContext>(options =>
  options.UseOracle(builder.Configuration.GetConnectionString("OracleDb")));

// 注册服务
builder.Services.AddControllers();
builder.Services.AddScoped<IWordRepository, WordRepository>();
builder.Services.AddScoped<IWordService, WordService>();
builder.Services.AddScoped<IArticleImportService, ArticleImportService>();
builder.Services.AddScoped<IHomeService, HomeService>();



// 注册依赖（文章管理相关）
builder.Services.AddScoped<DbContext>(provider => provider.GetService<AppDbContext>());
builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
builder.Services.AddScoped<IArticleService, ArticleService>();
builder.Services.AddScoped<IReadingQuestionRepository, ReadingQuestionRepository>();
builder.Services.AddScoped<IAttemptRepository, AttemptRepository>();

// 注册依赖（交友系统相关）
builder.Services.AddScoped<IFriendRepository, FriendRepository>();
builder.Services.AddScoped<IFriendService, FriendService>();

// 注册依赖（排行榜相关）
builder.Services.AddScoped<ILeaderboardRepository, LeaderboardRepository>();
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();
builder.Services.AddScoped<IRankingInitializeService, RankingInitializeService>();


// 注册依赖（学习计划相关）
builder.Services.AddScoped<IStudyPlanRepository, StudyPlanRepository>();
builder.Services.AddScoped<IStudyPlanService, StudyPlanService>();

// 注册依赖（成就系统相关）
builder.Services.AddScoped<IAchievementRepository, AchievementRepository>();
builder.Services.AddScoped<IAchievementService, AchievementService>();

// 注册依赖（用户学习记录相关）
builder.Services.AddScoped<IUserLearningRecordRepository, UserLearningRecordRepository>();
builder.Services.AddScoped<IUserLearningRecordService, UserLearningRecordService>();

// 注册依赖（用户每日学习单词数量相关）
builder.Services.AddScoped<IUserDailyWordsRepository, UserDailyWordsRepository>();
builder.Services.AddScoped<IUserDailyWordsService, UserDailyWordsService>();

// 注册 HttpClientFactory
builder.Services.AddHttpClient();

// 注册 AI 服务
builder.Services.AddScoped<IAIService, AIService>();

// 注册听力数据导入服务
builder.Services.AddScoped<ListeningDataImporter>();

//注册用户每日签到服务
builder.Services.AddScoped<IPayingService, PayingService>();

// 注册Adventure
builder.Services.AddScoped<IAdventureRepository, AdventureRepository>();
builder.Services.AddScoped<IAdventureTreasureRepository, AdventureTreasureRepository>();
builder.Services.AddScoped<IUserAdventureRepository, UserAdventureRepository>();
builder.Services.AddScoped<IUserAdventureTreasureRepository, UserAdventureTreasureRepository>();
builder.Services.AddScoped<IAdventureService, AdventureService>();
builder.Services.AddScoped<IAdventureTreasureService, AdventureTreasureService>();
builder.Services.AddScoped<IUserAdventureService, UserAdventureService>();

// 注册广告相关服务
builder.Services.AddScoped<IAdvertisementService, AdvertisementService>();

// 注册游戏相关服务
builder.Services.AddScoped<IGameService, GameService>();

// 注册帖子相关服务
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ILikeRepository, LikeRepository>();

builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ILikeService, LikeService>();


var app = builder.Build();

// —— 在中间件里加入静态文件支持
app.UseStaticFiles();

// 生产环境：后台启动，不阻塞端口监听
if (!app.Environment.IsDevelopment())
{
    _ = Task.Run(async () =>
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var initService = scope.ServiceProvider.GetRequiredService<IRankingInitializeService>();
            await initService.InitializeExistingUsersAsync();
        }
        catch (Exception ex)
        {
            // TODO: 这里写日志，避免吞异常
            Console.Error.WriteLine($"Init failed: {ex}");
        }
    });
}
else
{
    // 开发环境：你可以保留同步执行，方便看到异常
    using var scope = app.Services.CreateScope();
    var initService = scope.ServiceProvider.GetRequiredService<IRankingInitializeService>();
    await initService.InitializeExistingUsersAsync();
}

if (!builder.Environment.IsDevelopment())
{
    builder.WebHost.UseUrls("http://127.0.0.1:5000");
}

//// 启动时跑一次数据导入
//using (var scope = app.Services.CreateScope())
//{
//    var importer = scope.ServiceProvider
//                        .GetRequiredService<ListeningDataImporter>();
//    // 拼接你的 SeedData 文件路径
//    var seedPath = Path.Combine(
//        app.Environment.ContentRootPath,
//        "SeedData",
//        "cet6-2020-06.json"
//    );
//    await importer.ImportFromJsonAsync(seedPath);
//}

// 启用 CORS 策略
app.UseCors("AllowAll");

// 开发环境下启用 Swagger
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

// 添加默认根路由，访问 https://localhost:7071/ 时返回欢迎页面
app.MapGet("/", () =>
{
  return Results.Content("<h1>Welcome to the .NET8 HTTPS Backend</h1>", "text/html");
});

// 定义 Minimal API 接口，访问 https://localhost:7071/api/english/hello 返回简单 JSON 数据
app.MapGet("/api/english/hello", () =>
{
  return Results.Ok(new { message = "Hello from .NET8 backend created by bing!" });
});


// 读取配置中连接字符串的方式，可以通过 app.Configuration 来获取
string oracleConnectionString = app.Configuration.GetConnectionString("OracleDb");

// 定义一个 API 接口测试数据库连接
app.MapGet("/api/english/dbtest", async () =>
{
  try
  {
    using (var connection = new OracleConnection(oracleConnectionString))
    {
      await connection.OpenAsync();

      string sql = "SELECT 1 FROM DUAL";  // DUAL 是 Oracle 内置表，用于返回单行单列数据
      using (var command = new OracleCommand(sql, connection))
      {
        var result = await command.ExecuteScalarAsync();
        return Results.Ok(new { success = true, data = result });
      }
    }
  }
  catch (Exception ex)
  {
    return Results.Problem("数据库连接或查询出错: " + ex.Message);
  }
});

app.UseAuthentication();
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthorization();
app.MapControllers();
app.Run();