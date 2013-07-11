namespace Main.Core.WCF
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    using Main.Core.Entities;

    /// <summary>
    /// The auth packets.
    /// </summary>
    [DataContract]
    [KnownType(typeof(AuthorizationPacket))]
    [KnownType(typeof(RegisterData))]
    public class AuthPackets
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the packets.
        /// </summary>
        [DataMember]
        public IList<IAuthorizationPacket> Packets { get; set; }

        #endregion
    }

    /// <summary>
    /// The AuthorizationService interface.
    /// </summary>
    [ServiceContract]
    public interface IAuthorizationService
    {
        #region Public Methods and Operators

        /// <summary>
        /// Requests authorization for device described by packet
        /// </summary>
        /// <param name="authorizationPacket">
        /// </param>
        /// <returns>
        /// Authorization status
        /// </returns>
        /// <remarks>
        /// Current implementation assumes only outting this packet to the 
        /// list of active authorization requests. The supervisor browser watches 
        /// this list to allow Supervisor decide about actual authorization.
        /// </remarks>
        [OperationContract]
        bool AuthorizeDevice(AuthorizationPacket authorizationPacket);

        /// <summary>
        /// Returns path of application w/o virtual path included
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        [OperationContract]
        string GetPath();

        /// <summary>
        /// Get list of all packets if authorizedRegistrationId is empty. 
        /// Otherwize cleans all packets with authorized status for specific registration id
        /// </summary>
        /// <param name="authorizedRegistrationId">
        /// registration id to filter by and clean out all authorized packets
        /// </param>
        /// <returns>
        /// The <see cref="AuthPackets"/>.
        /// </returns>
        [OperationContract]
        AuthPackets PickupAuthorizationPackets(Guid authorizedRegistrationId);

        #endregion
    }
}