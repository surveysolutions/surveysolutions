using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Entities
{
    class Report : IEntity<ReportDocument>

    {
        private ReportDocument innerDocument;
        public string Id
        {
            get
            {
                return innerDocument.Id;
            }
        }

        public ReportDocument GetInnerDocument()
        {
            return this.innerDocument;
        }

        public Report(ReportDocument document)
        {
            this.innerDocument = document;
        }

        public Report(string title, string description)
        {
            innerDocument = new ReportDocument() { Title = title, Description = description };
        }
    }
}
