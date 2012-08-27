using System;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using System.Linq;
namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Export
{
    public class CompleteQuestionnaireExportItem
    {
        public Guid CompleteQuestionnaireKey {get; private set; }


        public ICompleteAnswer[] CompleteAnswers { get; set; }

        public CompleteQuestionnaireExportItem(CompleteQuestionnaireDocument doc)
        {
            this.CompleteQuestionnaireKey = doc.PublicKey;
            this.CompleteAnswers =doc.Find<ICompleteAnswer>(a => a.Selected).
                    ToArray();
                
        }
    }
}
