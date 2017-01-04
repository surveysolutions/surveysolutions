using System.Linq;
using WB.Core.SharedKernels.DataCollection;

namespace WB.UI.Headquarters.API.WebInterview
{
    public partial class WebInterview
    {
        public void OnTitlesWithSubstitutionsChanged(Identity[] entityIdentities)
        {
            var updatedEntities = entityIdentities
                .Select(identity => new
                {
                    identity,
                    title = this.CurrentInterview.GetTitleText(identity)
                })
                .ToArray();

            if (updatedEntities.Any())
                this.Clients.Caller.updateTitlesWithSubstitutions(updatedEntities);
        }

        public void OnRosterTitlesChanged(Identity[] rosterInstanceIdentities)
        {
            var updatedRosterTitles = rosterInstanceIdentities
                .Select(identity => new
                {
                    identity,
                    rosterTitle = this.CurrentInterview.GetRosterTitle(identity)
                })
                .ToArray();

            if (updatedRosterTitles.Any())
                this.Clients.Caller.updateRosterTitles(updatedRosterTitles);
        }
    }
}