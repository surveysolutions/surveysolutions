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

    public enum ServicePackectType
    {
        Request,
        Responce,
    }

    /// <summary>
    /// Service request basic interface
    /// </summary>
    public interface IServiceAuthorizationPacket
    {
        ServicePackectType Type { get; }
        RegisterData Data { get; }
        bool IsUsbChannel { get; }
        bool IsAuthorized { get; set; }
    }

    public interface IRequestPacket : IServiceAuthorizationPacket
    {
    }

    public interface IResponcePacket : IServiceAuthorizationPacket
    {
    }

}
