namespace WB.UI.Designer.Models
{
    /// <summary>
    /// The email confirmation model.
    /// </summary>
    public class EmailConfirmationModel
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets the confirmation token.
        /// </summary>
        public string ConfirmationToken { get; set; }

        /// <summary>
        ///     Gets or sets the email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        ///     Gets or sets the user name.
        /// </summary>
        public string UserName { get; set; }

        #endregion
    }
}