using System;

namespace WB.Core.GenericSubdomains.Portable
{
    public interface IPasswordHasher
    {
        string Hash(string password);

        bool VerifyPassword(string hashedPassword, string password);
    }
}
