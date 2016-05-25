using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.Preloading
{
    public class QuestionnairePreloadingDataInputModel
    {
        public QuestionnairePreloadingDataInputModel(Guid id, long questionnaireVerstion)
        {
            this.QuestionnaireId = id;
            this.QuestionnaireVerstion = questionnaireVerstion;
        }
       
        public Guid QuestionnaireId { get; private set; }
        public long QuestionnaireVerstion { get; private set; }
    }
}
