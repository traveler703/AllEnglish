namespace AllEnBackend.Models
{
    public class UserProfileUpdateRequest
    {
        public string Id { get; set; }                        // 用户的 ID
        public string TypeOfContent { get; set; }             // 要更新的字段类型
        public string Content { get; set; }                   // 要更新的内容
    }
}
