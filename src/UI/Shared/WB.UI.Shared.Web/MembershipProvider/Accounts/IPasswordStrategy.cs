
namespace WB.UI.Shared.Web.MembershipProvider.Accounts
{
    using System.Web.Security;

    /// <summary>
    /// Used to handle passwords.
    /// </summary>
    public interface IPasswordStrategy
    {
        /// <summary>
        /// Gets if passwords can be decrypted.
        /// </summary>
        bool IsPasswordsDecryptable { get; }

        /// <summary>
        /// Gets how passwords are stored in the database.
        /// </summary>
        MembershipPasswordFormat PasswordFormat { get; }

        /// <summary>
        /// Encrypt a password
        /// </summary>
        /// <param name="account">Account information used to encrypt password</param>
        /// <returns>encrypted password.</returns>
        /// <remarks>You can set the salt property which exist in the account information. 
        /// Encryption can be one way (hashing) or regular encryption</remarks>
        string Encrypt(AccountPasswordInfo account);

        /// <summary>
        /// Decrypt a password
        /// </summary>
        /// <param name="password">Encrpted password</param>
        /// <returns>Decrypted password if decryption is possible; otherwise null.</returns>
        string Decrypt(string password);

        /// <summary>
        /// Generate a new password
        /// </summary>
        /// <param name="policy">Policy that should be used when generating a new password.</param>
        /// <returns>A password which is not encrypted.</returns>
        string GeneratePassword(IPasswordPolicy policy);

        /// <summary>
        /// Compare if the specified password matches the encrypted password
        /// </summary>
        /// <param name="account">Stored acount informagtion.</param>
        /// <param name="clearTextPassword">Password specified by user.</param>
        /// <returns>true if passwords match; otherwise null</returns>
        /// <remarks>
        /// Method exists to make it possible to compare the password that the user have written
        /// with the one that have been stored in a database.
        /// </remarks>
        bool Compare(AccountPasswordInfo account, string clearTextPassword);

        /// <summary>
        /// Checks if the specified password is valid
        /// </summary>
        /// <param name="password">Password being checked</param>
        /// <param name="passwordPolicy">Policy used to validate password.</param>
        /// <returns></returns>
        bool IsValid(string password, IPasswordPolicy passwordPolicy);
    }
}