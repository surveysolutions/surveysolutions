namespace WB.UI.Shared.Web.MembershipProvider.Accounts.PasswordStrategies
{
    using System;
    using System.Security;
    using System.Security.Cryptography;
    using System.Text;
    using System.Web.Security;

    /// <summary>
    /// Encrypts the password by using triple des.
    /// </summary>
    public class TripleDesStrategy : IPasswordStrategy
    {
        private readonly SecureString _passphrase;

        /// <summary>
        /// Initializes a new instance of the <see cref="TripleDesStrategy"/> class.
        /// </summary>
        /// <param name="passphrase">The passphrase used to encrypt/decrypt passwords.</param>
        public TripleDesStrategy(SecureString passphrase)
        {
            this._passphrase = passphrase;
        }

        #region IPasswordStrategy Members

        /// <summary>
        /// Encrypt a password
        /// </summary>
        /// <param name="account">Account information used to encrypt password</param>
        /// <returns>
        /// encrypted password.
        /// </returns>
        public string Encrypt(AccountPasswordInfo account)
        {
            return EncryptString(account.Password, this._passphrase.ToString());
        }

        /// <summary>
        /// Decrypt a password
        /// </summary>
        /// <param name="password">Encrpted password</param>
        /// <returns>Decrypted password if decryption is possible; otherwise null.</returns>
        public string Decrypt(string password)
        {
            return DecryptString(password, this._passphrase.ToString());
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
            var clear = DecryptString(account.Password, this._passphrase.ToString());
            return clearTextPassword == clear;
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
            get { return MembershipPasswordFormat.Encrypted; }
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

        /// <summary>
        /// Encrypts a string.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="passphrase">The passphrase.</param>
        /// <returns>Encrypted string</returns>
        public static string EncryptString(string password, string passphrase)
        {
            byte[] results;
            var encoding = Encoding.UTF8;

            var hashProvider = new MD5CryptoServiceProvider();
            var key = hashProvider.ComputeHash(encoding.GetBytes(passphrase));
            var cryptoServiceProvider = new TripleDESCryptoServiceProvider
                                            {Key = key, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7};

            var dataToEncrypt = encoding.GetBytes(password);
            try
            {
                var encryptor = cryptoServiceProvider.CreateEncryptor();
                results = encryptor.TransformFinalBlock(dataToEncrypt, 0, dataToEncrypt.Length);
            }
            finally
            {
                cryptoServiceProvider.Clear();
                hashProvider.Clear();
            }

            return Convert.ToBase64String(results);
        }

        /// <summary>
        /// Decrypts a string.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="passphrase">The passphrase.</param>
        /// <returns>Decrypted string</returns>
        public static string DecryptString(string password, string passphrase)
        {
            byte[] results;
            var encoding = Encoding.UTF8;
            var hashProvider = new MD5CryptoServiceProvider();
            var key = hashProvider.ComputeHash(encoding.GetBytes(passphrase));
            var cryptoServiceProvider = new TripleDESCryptoServiceProvider
                                            {Key = key, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7};

            var dataToDecrypt = Convert.FromBase64String(password);
            try
            {
                var decryptor = cryptoServiceProvider.CreateDecryptor();
                results = decryptor.TransformFinalBlock(dataToDecrypt, 0, dataToDecrypt.Length);
            }
            finally
            {
                cryptoServiceProvider.Clear();
                hashProvider.Clear();
            }
            return encoding.GetString(results);
        }
    }
}