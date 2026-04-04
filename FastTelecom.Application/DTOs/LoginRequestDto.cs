using System;
using System.Collections.Generic;
using System.Text;

namespace FastTelecom.Application.DTOs
{
    public sealed class LoginRequestDto
    {
        public required string Username { get; init; }
        public required string Password { get; init; }
    }
}
