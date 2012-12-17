// -----------------------------------------------------------------------
// <copyright file="ServiceRegistrationPacket.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Synchronization.Core.Registration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Synchronization.Core.Interface;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public partial class AuthorizationPacket : IAuthorizationPacket//, IComparable<IServiceAuthorizationPacket>
    {
        public AuthorizationPacket(RegisterData data, ServicePacketChannel channel, ServicePacketType type)
        {
            Data = data;
            Channel = channel;
            PacketType = type;
        }

        public AuthorizationPacket()
            : this(null, ServicePacketChannel.Net, ServicePacketType.Request) // fix non-serializable values
        {
        }

        public ServicePacketChannel Channel { get; private set; }
        public ServicePacketType PacketType { get; private set; }
        public bool IsMarkedToAuthorize { get; private set; }
        public bool IsTreated { get; set; }

        IRegisterData IAuthorizationPacket.Data { get { return this.Data; } set { this.Data = value as RegisterData; } }

        public void SetChannel(ServicePacketChannel channel)
        {
            Channel = channel;
        }

        public void MarkToAuthorize(bool toAuthorize)
        {
            IsMarkedToAuthorize = !IsAuthorized && toAuthorize;
        }

        public void SetRegistrator(Guid registrar)
        {
            Data.Registrator = registrar;
        }

        public void SetRegistrationId(Guid registrationId)
        {
            Data.RegistrationId = registrationId;
        }
    }
}
