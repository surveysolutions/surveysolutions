// -----------------------------------------------------------------------
// <copyright file="IAuthorizationService.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Synchronization.Core.Interface
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public enum ServicePacketType
    {
        Request,
        Responce,
    }

    public enum ServicePacketChannel
    {
        Usb,
        Net,
    }

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface IAuthorizationPacket
    {
        IRegisterData Data { get; set; }
        bool IsAuthorized { get; set; }

        ServicePacketType PacketType { get; }
        ServicePacketChannel Channel { get; }
        bool IsMarkedToAuthorize { get; }

        void SetChannel(ServicePacketChannel channel);
        void SetRegistrator(Guid registrar);
        void SetRegistrationId(Guid supervisorId);

        void MarkToAuthorize(bool toAuthorize);
        bool IsTreated { get; set; }
    }
}
