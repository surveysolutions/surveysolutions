using System.Collections.Generic;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Status.StatusElement;
using System.Linq;

namespace RavenQuestionnaire.Core.Views.Status
{
    /// <summary>
    /// 
    /// </summary>
    public class StatusView
    {
        public string QuestionnaireId
        {
            get { return IdUtil.ParseId(_questionnaireId); }
            set { _questionnaireId = value; }
        }

        private string _questionnaireId;

        public string Id
        {
            get { return IdUtil.ParseId(_statusId); }
            set { _statusId = value; }
        }
        private string _statusId;

        public List<StatusItemView> StatusElements { get; set; }

        public StatusView()
        {
            StatusElements = new List<StatusItemView>();
        }

        public StatusView(StatusDocument doc)
        {
            Id = doc.Id;
            QuestionnaireId = doc.QuestionnaireId;
            StatusElements = doc.Statuses.Select(x => new StatusItemView(x, doc.Id, doc.QuestionnaireId)).ToList();
        }

        public static StatusView New()
        {
            return new StatusView();
        }
    }
}
