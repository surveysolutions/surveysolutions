using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Core.BoundedContexts.QuestionnaireTester
{
    internal static class Extensions
    {
        public static Identity ToIdentityForEvents(this SharedKernels.DataCollection.Identity identity)
        {
            return new Identity(identity.Id, identity.RosterVector);
        }
    }
}