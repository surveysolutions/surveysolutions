using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete.Question;
using RavenQuestionnaire.Core.Views.Question;

namespace RavenQuestionnaire.Core.AbstractFactories
{
    public interface ICompleteQuestionFactory
    {
        AbstractQuestion Create(QuestionType type);
        CompleteQuestionView CreateQuestion(CompleteQuestionnaireDocument doc, ICompleteGroup group, ICompleteQuestion question);
        ICompleteQuestion ConvertToCompleteQuestion(IQuestion question);
        IAnswerStrategy Create(ICompleteQuestion baseQuestion);
        object GetAnswerValue(ICompleteQuestion baseQuestion);
        string GetAnswerString(QuestionType type, object answer);
    }
}
