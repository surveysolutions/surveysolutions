namespace Main.Core.WCF
{
    using Main.Core.Entities;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class AuthorizationPacket : IAuthorizationPacket
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        public RegisterData Data { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is authorized.
        /// </summary>
        public bool IsAuthorized { get; set; }

        #endregion
    }
}