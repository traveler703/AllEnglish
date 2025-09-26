namespace AllEnBackend.Models
{
    // 表示与 OpenAI Chat API 对话时的单条消息：role（system/user/assistant）和 content。
    public class ChatMessage
    {
        // 消息角色："system"、"user" 或 "assistant"
        public string role { get; }

        // 消息内容
        public string content { get; }

        // 构造一个 ChatMessage
        public ChatMessage(string role, string content)
        {
            this.role = role;
            this.content = content;
        }
    }
}
