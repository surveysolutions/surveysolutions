using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Views.Status
{
    public class StatusBrowseViewFactory : IViewFactory<StatusBrowseInputModel, StatusBrowseView>
    {
        private IDocumentSession documentSession;


        public StatusBrowseViewFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }

        public StatusBrowseView Load(StatusBrowseInputModel input)
        {
            // Adjust the model appropriately
            var count = documentSession.Query<StatusDocument>().Count();
            if (count == 0)
                return new StatusBrowseView(input.Page, input.PageSize, count, new StatusBrowseItem[0]);
            // Perform the paged query
            var query = documentSession.Query<StatusDocument>()
                    .Skip((input.Page - 1) * input.PageSize)
                    .Take(input.PageSize);


            // And enact this query
            var items = query
                .Select(x => new StatusBrowseItem(x.Id, x.Title))
                .ToArray();

            return new StatusBrowseView(
                input.Page,
                input.PageSize, count,
                items.ToArray());
        }
    }
}
