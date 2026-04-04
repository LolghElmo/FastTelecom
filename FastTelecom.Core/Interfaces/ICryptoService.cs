using System;
using System.Collections.Generic;
using System.Text;

namespace FastTelecom.Domain.Interfaces
{
    public interface ICryptoService
    {
        string Encrypt(string plaintext);
        string UserNameHash { get; }
        string UserPswdHash { get; }
    }
}
