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

        private IList<IAuthorizationPacket> accumulatedPackets = new List<IAuthorizationPacket>();
        private IList<IAuthorizationPacket> newPackets = new List<IAuthorizationPacket>();

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
            try
            {
                lock (this)
                {
                    var allAvailablePackets = OnCollectAuthorizationPackets(extraUsbPackets ?? new List<IAuthorizationPacket>()).ToList();
                    if (allAvailablePackets.Count == 0)
                        return;

                    this.accumulatedPackets = this.accumulatedPackets.Union(allAvailablePackets, this).ToList();

                    var oldCount = this.newPackets.Where(p => !p.IsTreated).ToList().Count;
                    this.newPackets = this.accumulatedPackets.Where(p => !p.IsTreated).ToList();

                    if (this.newPackets.Count > oldCount && PacketsCollected != null)
                        PacketsCollected(this.newPackets);
                }
            }
            catch
            {
            }
        }

        #endregion

        #region Properties

        public IList<IAuthorizationPacket> NewServicePackets { get { lock (this) { return this.newPackets; } } }

        #endregion

        #region IEqualityComparer Members

        public bool Equals(IAuthorizationPacket x, IAuthorizationPacket y)
        {
            if (object.ReferenceEquals(x, y))
                return true;

            if (object.ReferenceEquals(x, null) || object.ReferenceEquals(y, null))
                return false;

            System.Diagnostics.Debug.Assert(x.Data != null && y.Data != null);

            return x.Data.RegistrationId == y.Data.RegistrationId && x.Data.RegisterDate == y.Data.RegisterDate;
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
                this.accumulatedPackets = this.accumulatedPackets.Where(p => p.Channel != channelType).ToList();
            }
        }

        #endregion
    }
}
