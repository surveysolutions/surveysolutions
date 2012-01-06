using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.Report
{
    public class ReportViewInputModel
    {
        public ReportViewInputModel(string id)
        {
            Id = IdUtil.CreateReportId(id);
        }

        public string Id { get; private set; }
    }
}
