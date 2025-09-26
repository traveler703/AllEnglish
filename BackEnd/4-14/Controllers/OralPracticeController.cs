using Dapper;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using AllEnBackend.Services;
using AllEnBackend.Models;

namespace AllEnBackend.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [ApiController]
    [Route("api/[controller]")]
    public class OralPracticeController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        private readonly IAIService _aiService;
        private readonly string _connString;

        // 构造函数，注入配置、环境和 AI 服务，并读取 Oracle 连接字符串。
        public OralPracticeController(IConfiguration config, IWebHostEnvironment env, IAIService aiService)
        {
            _config = config;
            _env = env;
            _aiService = aiService;
            _connString = _config.GetConnectionString("OracleDb");
        }

        // 启动一个新的会话，生成唯一的 SessionId 并写入 ORAL_SESSION 表。
        // 前端在每次开始练习时调用此接口，并保存返回的 sessionId。
        [HttpPost("start")]
        public async Task<IActionResult> StartSession()
        {
            var sessionId = Guid.NewGuid().ToString();
            using var db = new OracleConnection(_connString);
            await db.OpenAsync();
            await db.ExecuteAsync(@"
                INSERT INTO ORAL_SESSION (SESSION_ID)
                VALUES (:sid)",
                new { sid = sessionId });

            return Ok(new { sessionId });
        }

        // 处理一轮语音口语练习：接收音频，调用 Whisper 识别文字，ChatGPT 对话，TTS 合成，
        // 并将用户与 AI 的对话记录写入 ORAL_MESSAGE 表。
        // 前端必须通过 query string 携带 sessionId 标识会话。
        [HttpPost("oralpractice")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Post([FromQuery] string sessionId, [FromForm] IFormFile audio)
        {
            if (string.IsNullOrEmpty(sessionId))
                return BadRequest("缺少 sessionId");
            if (audio == null || audio.Length == 0)
                return BadRequest("未检测到音频文件");

            // 确定并创建 wwwroot/uploads 目录
            var webRoot = _env.WebRootPath;
            if (string.IsNullOrEmpty(webRoot))
                webRoot = Path.Combine(_env.ContentRootPath, "wwwroot");
            var uploads = Path.Combine(webRoot, "uploads");
            if (!Directory.Exists(uploads))
                Directory.CreateDirectory(uploads);

            // 保存上传的 .webm 文件到磁盘
            var webmFileName = $"{Guid.NewGuid()}.webm";
            var webmFilePath = Path.Combine(uploads, webmFileName);
            await using (var fs = new FileStream(webmFilePath, FileMode.Create, FileAccess.Write))
                await audio.CopyToAsync(fs);

            // 调用 AI 服务进行语音转文字
            string userText;
            try
            {
                userText = await _aiService.RecognizeSpeechAsync(webmFilePath);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"语音识别失败：{ex.Message}");
            }

            // 从数据库中读取该 sessionId 下的所有历史消息
            List<ChatMessage> history;
            using (var db = new OracleConnection(_connString))
            {
                await db.OpenAsync();
                var rows = await db.QueryAsync<(string Role, string Text)>(@"
                     SELECT ROLE, TEXT_CONTENT
                     FROM ORAL_MESSAGE
                     WHERE SESSION_ID = :sid
                     ORDER BY CREATED_AT",
                     new { sid = sessionId });
                history = rows.Select(r => new ChatMessage(r.Role, r.Text)).ToList();
            }

            // 构造 ChatGPT 对话所需的 messages 列表：system + 历史 + 本次 user
            var messages = new List<ChatMessage>
            {
                new ChatMessage("system", "You are an oral practice assistant.")
            };
            messages.AddRange(history);
            messages.Add(new ChatMessage("user", userText));

            // 调用 AI 服务生成对话回复
            string replyText;
            try
            {
                replyText = await _aiService.GetChatReplyAsync(messages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"AI 对话失败：{ex.Message}");
            }

            // 调用 TTS 合成回复音频
            var mp3FileName = $"{Guid.NewGuid()}.mp3";
            var mp3FilePath = Path.Combine(uploads, mp3FileName);
            try
            {
                await _aiService.SynthesizeSpeechAsync(replyText, mp3FilePath);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"语音合成失败：{ex.Message}");
            }

            // 构建前端访问的音频 URL
            var audioUrl = $"/uploads/{mp3FileName}";

            // 将用户与 AI 的对话记录写入 ORAL_MESSAGE 表
            using (var db = new OracleConnection(_connString))
            {
                await db.OpenAsync();
                const string insertMsg = @"
                    INSERT INTO ORAL_MESSAGE
                    (MESSAGE_ID, SESSION_ID, ROLE, TEXT_CONTENT, AUDIO_URL, CREATED_AT)
                    VALUES
                    (:mid, :sid, :role, :text, :audio, SYSTIMESTAMP)";
                // 用户消息
                await db.ExecuteAsync(insertMsg, new { mid = Guid.NewGuid().ToString(), sid = sessionId, role = "user", text = userText, audio = (string?)null });
                // AI 回复消息
                await db.ExecuteAsync(insertMsg, new { mid = Guid.NewGuid().ToString(), sid = sessionId, role = "assistant", text = replyText, audio = audioUrl });
            }
            // 返回给前端：原文、回复文本和音频 URL
            return Ok(new { userText, replyText, audioUrl });
        }

        // 用户点击“结束对话”时调用此接口，生成并返回整轮练习的点评（文本 + 语音 URL）。
        // 前端需传入 sessionId，以便聚合本会话所有消息进行点评。
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary([FromQuery] string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return BadRequest("缺少 sessionId");

            // 从 ORAL_MESSAGE 表中拉取该 sessionId 下的所有消息
            List<ChatMessage> history;
            using (var db = new OracleConnection(_connString))
            {
                await db.OpenAsync();
                var rows = await db.QueryAsync<(string Role, string Text)>(@"
                     SELECT ROLE, TEXT_CONTENT
                     FROM ORAL_MESSAGE
                     WHERE SESSION_ID = :sid
                     ORDER BY CREATED_AT",
                     new { sid = sessionId });
                history = rows.Select(r => new ChatMessage(r.Role, r.Text)).ToList();
            }

            // 调用 AI 服务生成对话点评
            string evaluation;
            try
            {
                evaluation = await _aiService.GetEvaluationAsync(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"点评生成失败：{ex.Message}");
            }

            // 合成点评音频并保存
            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var uploads = Path.Combine(webRoot, "uploads");
            var evalFile = $"{Guid.NewGuid()}.mp3";
            var evalPath = Path.Combine(uploads, evalFile);
            try
            {
                await _aiService.SynthesizeSpeechAsync(evaluation, evalPath);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"点评语音合成失败：{ex.Message}");
            }

            // 返回点评文本与音频 URL
            return Ok(new
            {
                evaluationText = evaluation,
                evaluationUrl = $"/uploads/{evalFile}"
            });
        }
    }
}
