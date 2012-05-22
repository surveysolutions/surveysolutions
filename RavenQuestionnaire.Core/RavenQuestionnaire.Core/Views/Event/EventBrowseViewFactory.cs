using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Views.Event
{
    public class EventBrowseViewFactory:  IViewFactory<EventBrowseInputModel, EventBrowseView>
    {
          private IDocumentSession documentSession;

          public EventBrowseViewFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }

          #region Implementation of IViewFactory<EventBrowseInputModel,EventBrowseView>

          public EventBrowseView Load(EventBrowseInputModel input)
          {
              var count = documentSession.Query<EventDocument>().Count();
              if (count == 0)
                  return new EventBrowseView(0, count, count, new EventBrowseItem[0]);
              // Perform the paged query
              var query = documentSession.Query<EventDocument>()
                  .Take(count);
            


              // And enact this query
              var items = query
                  .Select(
                      x =>
                      new EventBrowseItem(x.PublicKey, x.CreationDate,x.Command))
                  .ToArray();

              return new EventBrowseView(
                  0,
                  count, count,
                  items.ToArray());

          }

        #endregion
    }
}
