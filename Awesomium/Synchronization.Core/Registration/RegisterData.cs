using System;

namespace Synchronization.Core.Registration
{
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
            this.RegisterDate = DateTime.Now;
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
        /// Gets or sets registered Id (tablet Id on CAPI or .
        /// </summary>
        public Guid RegisterId { get; set; }

        /// <summary>
        /// Gets or sets GuidCurrentUser.
        /// </summary>
        public Guid GuidCurrentUser { get; set; }

        #endregion
    }
}
