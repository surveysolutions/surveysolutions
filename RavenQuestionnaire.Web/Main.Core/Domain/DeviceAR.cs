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
        private DateTime registeredDate;

        /// <summary>
        /// Field modificationDate
        /// </summary>
        private DateTime modificationDate;

        /// <summary>
        /// Field TabletId
        /// </summary>
        private Guid idForRegistration = Guid.Empty;

        /// <summary>
        /// Field registrator
        /// </summary>
        private Guid registrator = Guid.Empty;

        /// <summary>
        /// Field PublicKey
        /// </summary>
        private byte[] secretKey;

        /// <summary>
        /// Field Description
        /// </summary>
        private string description;

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
        public DeviceAR(Guid publicKey, string description, byte[] secretKey, Guid idForRegistration, Guid registrator)
            : base(publicKey)
        {
            this.ApplyEvent(
               new NewDeviceRegistered
               {
                   Registrator = registrator,
                   IdForRegistration = idForRegistration,
                   RegisteredDate = DateTime.Now,
                   SecretKey = secretKey,
                   Description = description                   
               });
        }

        #endregion
        
        #region Methods

        public void UpdateDevice(string description, byte[] secretKey, Guid registrator)
        {
            this.ApplyEvent(
              new UpdateRegisteredDevice
              {
                  Registrator = registrator,
                  DeviceId = idForRegistration,
                  SecretKey = secretKey,
                  Description = description
              });
        }

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
            this.modificationDate = e.RegisteredDate;
            this.idForRegistration = e.IdForRegistration;
            this.registrator = e.Registrator;
        }

        /// <summary>
        /// Event handler for the UpdateRegisteredDevice event.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnUpdateRegisteredDevice(UpdateRegisteredDevice e)
        {
            this.description = e.Description;
            this.secretKey = e.SecretKey;
            this.registrator = e.Registrator;
            this.modificationDate = DateTime.Now;
        }

        #endregion
    }
}
