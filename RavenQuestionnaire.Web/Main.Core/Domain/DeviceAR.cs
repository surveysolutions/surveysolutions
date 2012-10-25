// -----------------------------------------------------------------------
// <copyright file="DeviceAR.cs" company="WorldBank">
// 2012
// </copyright>
// -----------------------------------------------------------------------

namespace Main.Core.Domain
{
    using System;
    using Main.Core.Events.Synchronization;
    using Ncqrs.Domain;

    /// <summary>
    /// Class for registered device information
    /// </summary>
    public class DeviceAR : AggregateRootMappedByConvention
    {
        #region Fields

        /// <summary>
        /// Field RegisteredDate
        /// </summary>
        private DateTime RegisteredDate;

        /// <summary>
        /// Field TabletId
        /// </summary>
        private Guid TabletId;

        /// <summary>
        /// Field PublicKey
        /// </summary>
        private byte[] SecretKey;

        /// <summary>
        /// Field Description
        /// </summary>
        private string Description;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceAR"/> class.
        /// </summary>
        public DeviceAR()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceAR"/> class.
        /// </summary>
        /// <param name="description">
        /// The description.
        /// </param>
        /// <param name="tabletId">
        /// The tablet id.
        /// </param>
        /// <param name="secretKey">
        /// The secret Key.
        /// </param>
        /// <param name="registeredDate">
        /// The registered date.
        /// </param>
        public DeviceAR(string description, Guid tabletId, byte[] secretKey, DateTime registeredDate)
        {
            this.ApplyEvent(
               new NewDeviceRegistered
               {
                   SecretKey = secretKey,
                   RegisteredDate = registeredDate,
                   TabletId = tabletId,
                   Description = description
               });
        }

        #endregion
        
        #region Methods

        /// <summary>
        /// Event handler for the NewDeviceRegistered event. This method
        /// is automaticly wired as event handler based on convension.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnNewDeviceRegister(NewDeviceRegistered e)
        {
            this.Description = e.Description;
            this.SecretKey = e.SecretKey;
            this.RegisteredDate = e.RegisteredDate;
            this.TabletId = e.TabletId;
        }

        #endregion
    }
}
