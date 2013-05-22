namespace WB.UI.Shared.Web.MembershipProvider.Accounts.PasswordStrategies
{
    using System;
    using System.Web.Security;

    /// <summary>
    /// Strategy that do nothing with the passwords.
    /// </summary>
    public class ClearTextStrategy : IPasswordStrategy
    {
        private Random _random = new Random();

        #region Implementation of IPasswordStrategy

        /// <summary>
        /// Encrypt a password
        /// </summary>
        /// <param name="account">Account information used to encrypt password</param>
        /// <returns>
        /// encrypted password.
        /// </returns>
        public string Encrypt(AccountPasswordInfo account)
        {
            return account.Password;
        }

        /// <summary>
        /// Decrypt a password
        /// </summary>
        /// <param name="password">Encrpted password</param>
        /// <returns>Decrypted password if decryption is possible; otherwise null.</returns>
        public string Decrypt(string password)
        {
            return password;
        }

        /// <summary>
        /// Generate a new password
        /// </summary>
        /// <param name="policy">Policy that should be used when generating a new password.</param>
        /// <returns>A password which is not encrypted.</returns>
        public string GeneratePassword(IPasswordPolicy policy)
        {
            return policy.GeneratePassword();
        }

        /// <summary>
        /// Compare if the specified password matches the encrypted password
        /// </summary>
        /// <param name="account">Stored acount informagtion.</param>
        /// <param name="clearTextPassword">Password specified by user.</param>
        /// <returns>
        /// true if passwords match; otherwise null
        /// </returns>
        public bool Compare(AccountPasswordInfo account, string clearTextPassword)
        {
            return account.Password.Equals(clearTextPassword);
        }

        /// <summary>
        /// Gets if passwords can be decrypted.
        /// </summary>
        public bool IsPasswordsDecryptable
        {
            get { return true; }
        }

        /// <summary>
        /// Gets how passwords are stored in the database.
        /// </summary>
        public MembershipPasswordFormat PasswordFormat
        {
            get { return MembershipPasswordFormat.Clear; }
        }

        /// <summary>
        /// Checks if the specified password is valid
        /// </summary>
        /// <param name="password">Password being checked</param>
        /// <param name="passwordPolicy">Policy used to validate password.</param>
        /// <returns></returns>
        public bool IsValid(string password, IPasswordPolicy passwordPolicy)
        {
            return passwordPolicy.IsPasswordValid(password);
        }

        #endregion
    }
}