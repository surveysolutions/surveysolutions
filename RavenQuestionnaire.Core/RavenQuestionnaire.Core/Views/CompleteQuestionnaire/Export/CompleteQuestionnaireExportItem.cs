using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Questionnaire;
using System.Linq;
namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Export
{
    public class CompleteQuestionnaireExportItem
    {
        public string Id
        {
            get { return IdUtil.ParseId(_id); }
            private set { _id = value; }
        }

        private string _id;

        public CompleteAnswer[] CompleteAnswers { get; set; }
        
        public CompleteQuestionnaireExportItem(CompleteQuestionnaireDocument doc)
        {
            this.Id = IdUtil.ParseId(doc.Id);
            this.CompleteAnswers =
                new RavenQuestionnaire.Core.Entities.CompleteQuestionnaire(doc).GetAllAnswers().ToArray();
        }
    }
}
