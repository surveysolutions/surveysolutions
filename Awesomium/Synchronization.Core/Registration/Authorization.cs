// -----------------------------------------------------------------------
// <copyright file="Authorization.cs" company="">
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
    public abstract class Authorization : IAuthorizationServiceWatcher, IEqualityComparer<IAuthorizationPacket>
    {
        #region Members

        private IList<IAuthorizationPacket> requests = new List<IAuthorizationPacket>();

        #endregion

        #region C-tor

        protected Authorization()
        {
        }

        #endregion

        protected abstract IList<IAuthorizationPacket> OnCollectAuthorizationPackets(IList<IAuthorizationPacket> availablePackets);

        #region IAuthorizationServiceWatcher members

        public event AuthorizationPacketsAlarm PacketsCollected;

        public void CollectAuthorizationPackets(IList<IAuthorizationPacket> extraUsbPackets)
        {
            System.Diagnostics.Debug.Assert(extraUsbPackets != null);

            try
            {
                lock (this)
                {
                    //var oldCount = this.requests.Count;

                    this.requests.Clear();

                    var allPackets = OnCollectAuthorizationPackets(extraUsbPackets).ToList();

                    this.requests = allPackets.ToList();

                    if (this.requests.Count > 0/*oldCount*/ && PacketsCollected != null)
                        PacketsCollected(this.requests);
                }
            }
            catch
            {
            }
        }

        #endregion

        #region Properties

        public IList<IAuthorizationPacket> ServicePackets { get { lock (this) { return this.requests.ToList(); } } }

        #endregion

        #region IEqualityComparer Members

        public bool Equals(IAuthorizationPacket x, IAuthorizationPacket y)
        {
            if (object.ReferenceEquals(x, y))
                return true;

            if (object.ReferenceEquals(x, null) || object.ReferenceEquals(y, null))
                return false;

            System.Diagnostics.Debug.Assert(x.Data != null && y.Data != null);

            return x.Data.RegistrationId == y.Data.RegistrationId && x.IsAuthorized == y.IsAuthorized;
        }

        public int GetHashCode(IAuthorizationPacket x)
        {
            return object.ReferenceEquals(x, null) ? 0 : x.Data.RegistrationId.GetHashCode();
        }

        #endregion

        #region Operations

        internal void CleanChannelPackets(ServicePacketChannel channelType)
        {
            lock (this)
            {
                this.requests = this.requests.Where(p => p.Channel != channelType).ToList();
            }
        }

        #endregion
    }
}
