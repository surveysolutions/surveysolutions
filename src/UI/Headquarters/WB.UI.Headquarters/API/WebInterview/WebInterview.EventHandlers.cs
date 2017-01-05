using System.Linq;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.UI.Headquarters.API.WebInterview
{
    public partial class WebInterview : 
        ILiteEventHandler<SubstitutionTitlesChanged>,
        ILiteEventHandler<RosterInstancesTitleChanged>
    {
        public void Handle(SubstitutionTitlesChanged @event)
        {
            var updatedEntities = @event.Groups.Concat(@event.Questions).Concat(@event.StaticTexts)
                .Select(identity => new
                {
                    identity,
                    title = this.currentInterview.GetTitleText(identity)
                })
                .ToArray();

            if (updatedEntities.Any())
                this.Clients.Group(this.interviewId).updateTitlesWithSubstitutions(updatedEntities);
        }

        public void Handle(RosterInstancesTitleChanged @event)
        {
            var updatedRosterTitles = @event.ChangedInstances
                .Select(identity => new
                {
                    identity = identity.RosterInstance.GetIdentity(),
                    rosterTitle = identity.Title
                })
                .ToArray();

            if (updatedRosterTitles.Any())
                this.Clients.Group(this.interviewId).updateRosterTitles(updatedRosterTitles);
        }
    }
}