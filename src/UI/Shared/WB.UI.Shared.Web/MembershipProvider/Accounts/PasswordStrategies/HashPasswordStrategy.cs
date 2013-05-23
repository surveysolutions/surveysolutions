namespace WB.UI.Shared.Web.MembershipProvider.Accounts.PasswordStrategies
{
    using System;
    using System.Configuration.Provider;
    using System.Security.Cryptography;
    using System.Text;
    using System.Web.Security;

    /// <summary>
    /// Hash a password using a salt.
    /// </summary>
    public class HashPasswordStrategy : IPasswordStrategy
    {
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
            if (account.PasswordSalt == null)
                account.PasswordSalt = this.CreateSalt(10);
            var saltAndPwd = String.Concat(account.Password, account.PasswordSalt);
            var bytes = Encoding.Default.GetBytes(saltAndPwd);
            var sha1 = SHA1.Create();
            var computedHash = sha1.ComputeHash(bytes);
            return Convert.ToBase64String(computedHash);
        }


        /// <summary>
        /// Decrypt a password
        /// </summary>
        /// <param name="password">Encrpted password</param>
        /// <returns>Decrypted password if decryption is possible; otherwise null.</returns>
        public string Decrypt(string password)
        {
            throw new ProviderException("Password decryption is not allowed/possible.");
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
            var clearTextInfo = new AccountPasswordInfo(account.UserName, clearTextPassword)
                                    {PasswordSalt = account.PasswordSalt};
            var password = this.Encrypt(clearTextInfo);
            return account.Password == password;
        }

        /// <summary>
        /// Gets if passwords can be decrypted.
        /// </summary>
        public bool IsPasswordsDecryptable
        {
            get { return false; }
        }

        /// <summary>
        /// Gets how passwords are stored in the database.
        /// </summary>
        public MembershipPasswordFormat PasswordFormat
        {
            get { return MembershipPasswordFormat.Hashed; }
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

        /// <summary>
        /// Create a salt and convert it to base64
        /// </summary>
        /// <param name="size">The size.</param>
        /// <returns>Base64 generated salt.</returns>
        protected string CreateSalt(int size)
        {
            //Generate a cryptographic random number.
            var rng = new RNGCryptoServiceProvider();
            var buff = new byte[size];
            rng.GetBytes(buff);

            // Return a Base64 string representation of the random number.
            return Convert.ToBase64String(buff);
        }

        #endregion
    }
}