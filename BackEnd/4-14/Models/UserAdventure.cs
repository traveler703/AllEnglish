using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace AllEnBackend.Models
{
    [PrimaryKey(nameof(UserId), nameof(AdventureId))]
    public class UserAdventure
    {
        public string UserId { get; set; }


        public long AdventureId { get; set; }

        public string Status { get; set; }

        // 导航属性
        public Adventure Adventure { get; set; }

        
    }
}