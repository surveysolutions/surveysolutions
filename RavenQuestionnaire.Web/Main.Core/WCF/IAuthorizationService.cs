using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Main.Core.Entities;

namespace Main.Core.WCF
{
    [DataContract]
    [KnownType(typeof(AuthorizationPacket))]
    [KnownType(typeof(RegisterData))]
    public class AuthPackets
    {
        [DataMember]
        public IList<IAuthorizationPacket> Packets { get; set; }
    }

    [ServiceContract]
    public interface IAuthorizationService
    {
        /// <summary>
        /// Returns path of application w/o virtual path included
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        string GetPath();

        /// <summary>
        /// Requests authorization for device described by packet
        /// </summary>
        /// <param name="authorizationPacket"></param>
        /// <returns>Authorization status</returns>
        /// <remarks>Current implementation assumes only outting this packet to the 
        /// list of active authorization requests. The supervisor browser watches 
        /// this list to allow Supervisor decide about actual authorization.</remarks>
        [OperationContract]
        bool AuthorizeDevice(AuthorizationPacket authorizationPacket);

        /// <summary>
        /// Get list of all packets if authorizedRegistrationId is empty. 
        /// Otherwize cleans all packets with authorized status for specific registration id
        /// </summary>
        /// <param name="authorizedRegistrationId">registration id to filter by and clean out all authorized packets</param>
        /// <returns></returns>
        [OperationContract]
        AuthPackets PickupAuthorizationPackets(Guid authorizedRegistrationId);
    }
}
