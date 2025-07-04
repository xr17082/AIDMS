﻿using System.ComponentModel.DataAnnotations;

namespace AIDMS.Shared.Application.Requests.Identity
{
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
