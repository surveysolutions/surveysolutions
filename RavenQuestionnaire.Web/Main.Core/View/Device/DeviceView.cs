// -----------------------------------------------------------------------
// <copyright file="DeviceView.cs" company="WorldBank">
// 2012
// </copyright>
// -----------------------------------------------------------------------

namespace Main.Core.View.Device
{
    using System;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class DeviceView
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceView"/> class.
        /// </summary>
        public DeviceView()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceView"/> class.
        /// </summary>
        /// <param name="description">
        /// The description.
        /// </param>
        /// <param name="registerDate">
        /// The register date.
        /// </param>
        /// <param name="secretKey">
        /// The secret key.
        /// </param>
        /// <param name="tabletId">
        /// The tablet id.
        /// </param>
        public DeviceView(string description, DateTime registerDate, byte[] secretKey, Guid tabletId)
        {
            this.Description = description;
            this.RegisterDate = registerDate;
            this.SecretKey = secretKey;
            this.TabletId = tabletId;
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
        /// Gets or sets TabletId.
        /// </summary>
        public Guid TabletId { get; set; }


        #endregion
    }
}
