using AllEnBackend.Models;

namespace AllEnBackend.Services
{
    // AI 服务接口，定义了语音识别、对话回复、语音合成三项功能。
    public interface IAIService
    {
        // 语音转文字
        Task<string> RecognizeSpeechAsync(string filePath);
        // 从OpenAI得到文字的回复
        Task<string> GetChatReplyAsync(IEnumerable<ChatMessage> messages);
        // 语音合成，即文字转语音
        Task SynthesizeSpeechAsync(string text, string outputFilePath);
        // 对整轮会话做点评
        Task<string> GetEvaluationAsync(IEnumerable<ChatMessage> messages);
    }
}
