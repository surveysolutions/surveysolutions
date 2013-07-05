namespace Main.Core.Entities
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class RegisterData
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterData"/> class.
        /// </summary>
        public RegisterData()
        {
#if DEBUG
            this.RegisterDate = DateTime.MinValue;
#endif
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets Description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets RegisterDate.
        /// </summary>
        public DateTime RegisterDate { get; set; }

        /// <summary>
        /// Gets or sets SecretKey.
        /// </summary>
        public byte[] SecretKey { get; set; }

        /// <summary>
        /// The ID of registrator who puts registration event to database
        /// </summary>
        public Guid Registrator { get; set; }

        /// <summary>
        /// The ID of device or another entity to be registered
        /// </summary>
        public Guid RegistrationId { get; set; }

        /// <summary>
        /// The ID of AR
        /// </summary>
        [IgnoreDataMember]
        public Guid PublicKey { get; set; }
        

        #endregion
    }
}
