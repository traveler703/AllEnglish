namespace AllEnBackend.Models
{
	public class UserRegisterRequest
	{
		public string UserName { get; set; } = string.Empty;  // �û���
		public string Password { get; set; } = string.Empty;  // ����
		public string Email { get; set; } = string.Empty;     // ����
        public string Gender { get; set; } = string.Empty;          // �Ա��� "��"/"Ů" �� "M"/"F"��
        public string DateOfBirth { get; set; } = string.Empty;            // �������ڣ�����Ϊ��
        public string PhoneNumber { get; set; } = string.Empty;     // �绰����
        public string AvatarUrl { get; set; } = string.Empty;    // �û�ͷ��·��
    }
}
