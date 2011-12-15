using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client;
using Raven.Client.Linq;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Indexes;

namespace RavenQuestionnaire.Core.Views.Location
{
    public class LocationBrowseViewFactory : IViewFactory<LocationBrowseInputModel, LocationBrowseView>
    {
        private IDocumentSession documentSession;

        public LocationBrowseViewFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }

        public LocationBrowseView Load(LocationBrowseInputModel input)
        {
            // Adjust the model appropriately
            var count = documentSession.Query<LocationDocument>().Count();
            if (count == 0)
                return new LocationBrowseView(input.Page, input.PageSize, count, new LocationBrowseItem[0]);
            // Perform the paged query
        /*    var usersWithAliases =
     (from user in documentSession.Query<UserDocument, UsersInLocationIndex>()
      select user).As<LocationWithUserStatistic>();*/
            var query = documentSession.Query<LocationWithUserStatistic, UsersInLocationIndex>()
                    .Skip((input.Page - 1) * input.PageSize)
                    .Take(input.PageSize).ToArray();

            
            // And enact this query
            var items = query
                .Select(x => new LocationBrowseItem(x.Id, x.Location, x.UserCount))
                .ToArray();

            return new LocationBrowseView(
                input.Page,
                input.PageSize, count,
                items.ToArray());
        }
    }
}
