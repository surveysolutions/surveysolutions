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
        CompleteQuestionView CreateQuestion(CompleteQuestionnaireDocument doc, ICompleteGroup group, ICompleteQuestion question);
        ICompleteQuestion ConvertToCompleteQuestion(IQuestion question);
        IAnswerStrategy Create(ICompleteQuestion<ICompleteAnswer> baseQuestion);
        object GetAnswerValue(ICompleteQuestion baseQuestion);
        string GetAnswerString(QuestionType type, object answer);
    }
}
