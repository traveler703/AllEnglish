namespace AllEnBackend.Models
{
    public class UserProfileUpdateRequest
    {
        public string Id { get; set; }                        // �û��� ID
        public string TypeOfContent { get; set; }             // Ҫ���µ��ֶ�����
        public string Content { get; set; }                   // Ҫ���µ�����
    }
}
