// -----------------------------------------------------------------------
// <copyright file="DeviceAR.cs" company="WorldBank">
// 2012
// </copyright>
// -----------------------------------------------------------------------

namespace Main.Core.Domain
{
    using System;

    using Main.Core.Entities.SubEntities;
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
        private DateTime registeredDate;

        /// <summary>
        /// Field TabletId
        /// </summary>
        private Guid tabletId;

        /// <summary>
        /// Field PublicKey
        /// </summary>
        private byte[] secretKey;

        /// <summary>
        /// Field Description
        /// </summary>
        private string description;

        /// <summary>
        /// Field PublicKey
        /// </summary>
        private Guid publicKey;

        private Guid registrator;

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
        public DeviceAR(string description, Guid tabletId, byte[] secretKey, DateTime registeredDate, Guid guidSupervisor)
            : base(tabletId)
        {
            this.ApplyEvent(
               new NewDeviceRegistered
               {
                   SecretKey = secretKey,
                   RegisteredDate = registeredDate,
                   TabletId = tabletId,
                   Description = description,
                   PublicKey = new Guid(secretKey),
                   Registrator = guidSupervisor
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
            this.description = e.Description;
            this.secretKey = e.SecretKey;
            this.registeredDate = e.RegisteredDate;
            this.tabletId = e.TabletId;
            this.publicKey = e.PublicKey;
            this.registrator = e.Registrator;
        }

        #endregion
    }
}
