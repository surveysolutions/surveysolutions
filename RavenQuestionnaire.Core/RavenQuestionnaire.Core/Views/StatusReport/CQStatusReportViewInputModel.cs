using System;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.StatusReport
{
    public class CQStatusReportViewInputModel
    {
        public CQStatusReportViewInputModel(string qId, Guid sId)
        {
            this.StatusId = sId;
            this.QuestionnaireId = qId;
        }
        public Guid StatusId { get; set; }
        public string QuestionnaireId { get; set; }
    }
}
