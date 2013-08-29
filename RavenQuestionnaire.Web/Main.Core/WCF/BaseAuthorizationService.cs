namespace Main.Core.WCF
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    
    using Main.Core.Entities;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class BaseAuthorizationService : IAuthorizationService, IEqualityComparer<IAuthorizationPacket>
    {
        private static IList<IAuthorizationPacket> packets = new List<IAuthorizationPacket>();

        protected virtual string OnGetPath()
        {
            return string.Empty;
        }

        public string GetPath()
        {
            return OnGetPath();
        }

        public bool AuthorizeDevice(AuthorizationPacket authorizationPacket)
        {
            if (authorizationPacket == null || authorizationPacket.IsAuthorized)
                return false;

            lock (packets)
            {
                packets.Add(authorizationPacket);
                packets = packets.Distinct(this).ToList();
            }

            return true;
        }

        public AuthPackets PickupAuthorizationPackets(Guid authorizedRegistrationId)
        {
            lock (packets)
            {
                if (authorizedRegistrationId == Guid.Empty)
                    return new AuthPackets() { Packets = packets.ToList() };

                var returnList = 
                    packets.Where(p => p.Data.RegistrationId == authorizedRegistrationId && p.IsAuthorized).ToList();

                packets = packets.Except(returnList).ToList();

                return new AuthPackets() { Packets = returnList };
            }           
        }

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

        #region Static Helpers

        /// <summary>
        /// Marks repective packets as authorized to check this by CAPI later
        /// </summary>
        /// <param name="tabletId"></param>
        /// <remarks>Perhaps this functionality should be replaced by checking respective event by CAPI in database directly</remarks>
        public static bool ConfirmAuthorizedRequest(RegisterData data)
        {
            bool result = false;

            lock (packets)
            {
                foreach (var p in packets)
                {
                    if (p.IsAuthorized)
                        continue;

                    if(p.Data.RegistrationId == data.RegistrationId)
                    {
                        p.Data = data;
                        p.IsAuthorized = true;

                        result = true;
                    }
                }
            }

            return result;
        }

        #endregion
    }


}
