
namespace WB.UI.Designer.Providers.Membership
{
    /// <summary>
    /// Information used by the password strategies.
    /// </summary>
    public class AccountPasswordInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AccountPasswordInfo"/> class.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        public AccountPasswordInfo(string username, string password)
        {
            UserName = username;
            Password = password;
        }

        /// <summary>
        /// Gest or sets the salt which was used when hashing the password.
        /// </summary>
        public string PasswordSalt { get; set; }

        /// <summary>
        /// Gets the password
        /// </summary>
        public string Password { get; private set; }

        /// <summary>
        /// Gets username for the accoount
        /// </summary>
        public string UserName { get; private set; }
    }
}