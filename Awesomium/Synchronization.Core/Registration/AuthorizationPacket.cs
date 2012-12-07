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
    public abstract class AuthorizationPacket : IServiceAuthorizationPacket//, IComparable<IServiceAuthorizationPacket>
    {
        protected AuthorizationPacket(RegisterData data, bool viaUsbChannel)
        {
            Data = data;
            IsUsbChannel = viaUsbChannel;
        }

        public abstract ServicePackectType Type { get; }
        public RegisterData Data { get; private set; }
        public bool IsUsbChannel { get; private set; }
        public bool IsAuthorized { get; set; }

        public static bool operator !=(AuthorizationPacket x, AuthorizationPacket y)
        {
            return !(x == y);
        }

        public static bool operator ==(AuthorizationPacket x, AuthorizationPacket y)
        {
            if (object.ReferenceEquals(x, y))
                return true;

            System.Diagnostics.Debug.Assert(x.Data != null && y.Data != null);

            return x.Data.RegistrationId == y.Data.RegistrationId;
        }

        public bool Equals(AuthorizationPacket x, AuthorizationPacket y)
        {
            return x == y;
        }

        public override bool Equals(object obj)
        {
            return Equals(this, (obj as AuthorizationPacket));
        }

        public override int GetHashCode()
        {
            return this.Data == null ? base.GetHashCode() : this.Data.RegistrationId.GetHashCode();
        }
    }
}
