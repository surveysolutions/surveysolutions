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
        protected IList<EventDocument> AccumulateWithPaging(Func<EventDocument,bool> predicate)
        {
            List<EventDocument> retval = new List<EventDocument>();
            int count;
            Raven.Client.Linq.RavenQueryStatistics stats;
            documentSession.Query<EventDocument>()
                .Statistics(out stats).Where(predicate).ToList();

            count = stats.TotalResults;
            if (count == 0)
                return retval;
            int queryLimit = 128;
            int step = 0;
            while (step < count)
            {
                retval.AddRange(
                    documentSession.Query<EventDocument>().Skip(step).Take(queryLimit).Where(predicate).ToList());
                step += queryLimit;
            }
            return retval;
        }

        public EventBrowseView Load(EventBrowseInputModel input)
        {
            IList<EventDocument> query;
          //  int count;
            if (!input.PublickKey.HasValue)
            {
                // Perform the paged query
                query = AccumulateWithPaging(IsEventImportable);
            }
            else
            {
                var last =
                    documentSession.Query<EventDocument>().FirstOrDefault(e => e.PublicKey == input.PublickKey.Value);
                if (last == null)
                    query = AccumulateWithPaging(IsEventImportable);
                else
                    query = AccumulateWithPaging(e => e.CreationDate > last.CreationDate && IsEventImportable(e));
            }

            // And enact this query
            var items = query
                .Select(
                    x =>
                    new EventBrowseItem(x.PublicKey, x.CreationDate, x.Command))
                .ToArray();

            return new EventBrowseView(
                0,
                query.Count, query.Count,
                items.ToArray());

        }

        #endregion
    }
}
