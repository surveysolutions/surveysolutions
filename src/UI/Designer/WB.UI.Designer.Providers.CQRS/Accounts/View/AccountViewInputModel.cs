namespace WB.UI.Designer.Providers.CQRS.Accounts.View
{
    /// <summary>
    /// The account view input model.
    /// </summary>
    public class AccountViewInputModel
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountViewInputModel"/> class.
        /// </summary>
        /// <param name="providerUserKey">
        /// The provider user key.
        /// </param>
        public AccountViewInputModel(object providerUserKey)
        {
            this.ProviderUserKey = providerUserKey;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountViewInputModel"/> class.
        /// </summary>
        /// <param name="accountName">
        /// The account name.
        /// </param>
        /// <param name="accountEmail">
        /// The account email.
        /// </param>
        /// <param name="confirmationToken">
        /// The confirmation token.
        /// </param>
        /// <param name="resetPasswordToken">
        /// The reset password token.
        /// </param>
        public AccountViewInputModel(
            string accountName, string accountEmail, string confirmationToken, string resetPasswordToken)
        {
            this.AccountName = accountName;
            this.AccountEmail = accountEmail;
            this.ConfirmationToken = confirmationToken;
            this.ResetPasswordToken = resetPasswordToken;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Account email
        /// </summary>
        public string AccountEmail { get; protected set; }

        /// <summary>
        ///     Account name
        /// </summary>
        public string AccountName { get; protected set; }

        /// <summary>
        ///     Account confirmation token
        /// </summary>
        public string ConfirmationToken { get; protected set; }

        /// <summary>
        ///     Account Id
        /// </summary>
        public object ProviderUserKey { get; protected set; }

        /// <summary>
        /// Gets or sets the reset password token.
        /// </summary>
        public string ResetPasswordToken { get; protected set; }

        #endregion
    }
}