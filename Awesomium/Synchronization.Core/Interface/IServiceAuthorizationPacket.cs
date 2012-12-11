// -----------------------------------------------------------------------
// <copyright file="IServiceRequest.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Synchronization.Core.Interface
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Synchronization.Core.Registration;

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
    /// Service request basic interface
    /// </summary>
    public interface IServiceAuthorizationPacket
    {
        ServicePacketType Type { get; }
        RegisterData Data { get; }
        ServicePacketChannel Channel { get; }
        bool IsAuthorized { get; set; }
    }

    public interface IRequestPacket : IServiceAuthorizationPacket
    {
    }

    public interface IResponcePacket : IServiceAuthorizationPacket
    {
    }

}
