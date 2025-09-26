namespace AllEnBackend.Models
{
    public class UserAchievement
    {
        public string UserId { get; set; } = string.Empty;     // �û�ID
        public int AchievementId { get; set; }                 // �ɾ�ID
        public int HasGained { get; set; } = 0;                // �Ƿ�ȡ�óɾͣ�0=δȡ�ã�1=��ȡ�ã�
        public DateTime? GainDate { get; set; }                // ȡ�óɾ͵�����

        // ��������
        public User User { get; set; } = null!;
        public Achievement Achievement { get; set; } = null!;
    }
} 