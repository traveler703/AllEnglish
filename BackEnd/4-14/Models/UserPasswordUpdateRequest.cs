namespace AllEnBackend.Models
{
    public class UserPasswordUpdateRequest
    {
        public string Email { get; set; }        // �û�����
        public string NewPassword { get; set; }  // ������
    }
}
