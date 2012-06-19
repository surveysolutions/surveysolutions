using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Raven.Client;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Views.Event
{
    public class EventBrowseViewFactory : IViewFactory<EventBrowseInputModel, EventBrowseView>
    {
        private IDocumentSession documentSession;

        public EventBrowseViewFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;

            using (Stream stream = this.GetType().Assembly.
               GetManifestResourceStream("RavenQuestionnaire.Core.Views.Event." + "ExportCommands.xml"))
            {
                using (StreamReader sr = new StreamReader(stream))
                {
                   // result = sr.ReadToEnd();
                    XDocument xdoc = XDocument.Load(sr);

                    exportableCommands = (from lv1 in xdoc.Descendants("Command")
                                          select lv1.Attribute("name").Value).ToArray();
                }
            }
        
        }

        private string[] exportableCommands;
        #region Implementation of IViewFactory<EventBrowseInputModel,EventBrowseView>
        protected bool IsEventImportable(EventDocument e)
        {

            return exportableCommands.Contains(e.Command.GetType().FullName);
        }

        public EventBrowseView Load(EventBrowseInputModel input)
        {
            IList<EventDocument> query;
            int count;
            if (!input.PublickKey.HasValue)
            {
                count = documentSession.Query<EventDocument>().Count(IsEventImportable);
                if (count == 0)
                    return new EventBrowseView(0, count, count, new EventBrowseItem[0]);
                // Perform the paged query
                query = documentSession.Query<EventDocument>().Where(IsEventImportable)
                    .Take(count).ToList();
            }
            else
            {
                var last =
                    documentSession.Query<EventDocument>().FirstOrDefault(e => e.PublicKey == input.PublickKey.Value);
                count = documentSession.Query<EventDocument>().Count(e => e.CreationDate > last.CreationDate && IsEventImportable(e));
                if (count == 0)
                    return new EventBrowseView(0, count, count, new EventBrowseItem[0]);
                // Perform the paged query
                query = documentSession.Query<EventDocument>().Where(e => e.CreationDate > last.CreationDate && IsEventImportable(e))
                    .Take(count).ToList();
            }

            // And enact this query
            var items = query
                .Select(
                    x =>
                    new EventBrowseItem(x.PublicKey, x.CreationDate, x.Command))
                .ToArray();

            return new EventBrowseView(
                0,
                count, count,
                items.ToArray());

        }

        #endregion
    }
}
