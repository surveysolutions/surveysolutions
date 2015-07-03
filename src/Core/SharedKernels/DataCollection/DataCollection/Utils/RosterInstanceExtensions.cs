using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Core.SharedKernels.DataCollection.Utils
{
    public static class RosterInstanceExtensions
    {
        public static Identity GetIdentity(this RosterInstance rosterInstance)
        {
            var rosterVector = rosterInstance.OuterRosterVector.Concat(rosterInstance.RosterInstanceId.ToEnumerable()).ToArray();
            return new Identity(rosterInstance.GroupId, rosterVector);
        }
    }
}