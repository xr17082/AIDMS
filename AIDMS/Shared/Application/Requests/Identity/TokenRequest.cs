﻿using System.ComponentModel.DataAnnotations;

namespace AIDMS.Shared.Application.Requests.Identity
{
    public class TokenRequest
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
