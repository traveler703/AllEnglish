﻿namespace AllEnBackend.Models
{
    public class VerifyCodeRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
