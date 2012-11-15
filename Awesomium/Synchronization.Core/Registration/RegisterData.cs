using System;

namespace Synchronization.Core.Registration
{
    /// <summary>
    /// TODO: Update summary.
 
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
        /// ID of device or supervisor
        /// </summary>
        public Guid Registrator { get; set; }

        /// <summary>
        /// The id of registrator who puts registration event to database
        /// </summary>
        public Guid RegistrationId { get; set; }

        #endregion

        public override string ToString()
        {
            return string.Format("Device: {0}, Authorized: {1}", Description, RegisterDate);
        }
    }
}
