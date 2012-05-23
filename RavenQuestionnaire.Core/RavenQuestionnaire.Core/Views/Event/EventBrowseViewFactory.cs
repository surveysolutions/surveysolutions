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
              IQueryable<EventDocument> query;
              int count;
              if (!input.PublickKey.HasValue)
              {
                  count = documentSession.Query<EventDocument>().Count();
                  if (count == 0)
                      return new EventBrowseView(0, count, count, new EventBrowseItem[0]);
                  // Perform the paged query
                  query = documentSession.Query<EventDocument>()
                      .Take(count);
              }
              else
              {
                  var last =
                      documentSession.Query<EventDocument>().FirstOrDefault(e => e.PublicKey == input.PublickKey.Value);
                  count = documentSession.Query<EventDocument>().Count(e => e.CreationDate>last.CreationDate);
                  if (count == 0)
                      return new EventBrowseView(0, count, count, new EventBrowseItem[0]);
                  // Perform the paged query
                  query = documentSession.Query<EventDocument>().Where(e => e.CreationDate > last.CreationDate)
                      .Take(count);
              }

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
