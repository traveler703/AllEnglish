using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AllEnBackend.Models;

namespace AllEnBackend.Services
{
    public class AIService : IAIService
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;

        // 构造函数
        public AIService(IConfiguration config, IHttpClientFactory httpFactory)
        {
            // 从配置里读取BaseUrl
            var apiKey = config["OpenAI:ApiKey"];
            _baseUrl = config["OpenAI:BaseUrl"]?.TrimEnd('/') ?? throw new ArgumentNullException("OpenAI:BaseUrl 配置缺失");
            _http = httpFactory.CreateClient();
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }

        // 语音转文字
        public async Task<string> RecognizeSpeechAsync(string filePath)
        {
            using var content = new MultipartFormDataContent();
            await using var fs = File.OpenRead(filePath);
            content.Add(new StreamContent(fs), "file", Path.GetFileName(filePath));
            content.Add(new StringContent("whisper-1"), "model");
            // 调用Whisper API
            var url = $"{_baseUrl}/audio/transcriptions";
            var response = await _http.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            using var jsonStream = await response.Content.ReadAsStreamAsync();
            var doc = await JsonDocument.ParseAsync(jsonStream);
            return doc.RootElement.GetProperty("text").GetString();
        }

        // 从OpenAI得到文字的回复
        public async Task<string> GetChatReplyAsync(IEnumerable<ChatMessage> messages)
        {
            // 构建 OpenAI 请求体
            var payload = new
            {
                model = "gpt-3.5-turbo",
                messages = messages.Select(m => new { role = m.role, content = m.content }).ToArray()
            };
            var jsonPayload = JsonSerializer.Serialize(payload);
            using var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var url = $"{_baseUrl}/chat/completions";
            var response = await _http.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            var doc = await JsonDocument.ParseAsync(stream);
            return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        }

        // 语音合成，即文字转语音
        public async Task SynthesizeSpeechAsync(string text, string outputFilePath)
        {
            var ttsPayload = new
            {
                model = "tts-1",
                voice = "alloy",
                input = text,
                format = "mp3"
            };
            var jsonTts = JsonSerializer.Serialize(ttsPayload);
            using var ttsContent = new StringContent(jsonTts, Encoding.UTF8, "application/json");

            // 调用语音和成TTS API
            var url = $"{_baseUrl}/audio/speech";
            var ttsResponse = await _http.PostAsync(url, ttsContent);
            ttsResponse.EnsureSuccessStatusCode();

            var audioBytes = await ttsResponse.Content.ReadAsByteArrayAsync();
            await File.WriteAllBytesAsync(outputFilePath, audioBytes);
        }

        // 对整轮会话做点评
        public async Task<string> GetEvaluationAsync(IEnumerable<ChatMessage> messages)
        {
            // 在所有历史消息后面，添加一个“点评”system角色
            var evalPrompt = new ChatMessage(
                "system",
                "You are an experienced English tutor. " +
                "After reviewing the conversation below, provide direct, second-person feedback using “you”. " +
                "Comment on student's grammar, fluency, and vocabulary usage, then give specific, actionable suggestions for improvement. " +
                "Keep the evaluation concise, constructive, and in clear English without any markdown. " +
                "Begin with: “Here is your personalized evaluation of this oral practice:”"
            );

            // 把 prompt 放到首位，再加上所有消息
            var evalMessages = new List<ChatMessage> { evalPrompt };
            evalMessages.AddRange(messages);

            var payload = new
            {
                model = "gpt-3.5-turbo",
                messages = evalMessages.Select(m => new { role = m.role, content = m.content }).ToArray()
            };
            var jsonPayload = JsonSerializer.Serialize(payload);
            using var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var url = $"{_baseUrl}/chat/completions";
            var response = await _http.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            var doc = await JsonDocument.ParseAsync(stream);
            return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        }
    }
}
