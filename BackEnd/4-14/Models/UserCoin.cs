using System;

namespace AllEnBackend.Models
{
    public class UserCoin
    {
        public string Id { get; set; } // ����
        public string UserId { get; set; } // ������û�ID��
        public int Coin { get; set; } = 1000;//����ң������û�ʱĬ��ֵΪ1000��
        public DateTime? LastSignDate { get; set; }
        public DateTime? FirstSignDate { get; set;}
    }
}
