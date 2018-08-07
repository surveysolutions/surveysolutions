using System;

namespace WB.Core.GenericSubdomains.Portable
{
    public interface IPasswordHasher
    {
        string Hash(string password);
        PasswordVerificationResult VerifyPassword(string hashedPassword, string password);
    }

    public enum PasswordVerificationResult
    {
        Failed,
        Success,
        SuccessRehashNeeded,
    }
}
