using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Views.Answer;

namespace RavenQuestionnaire.Core.AbstractFactories
{
    public interface ICompleteAnswerFactory
    {
        CompleteAnswerView CreateGroup(CompleteQuestionnaireDocument doc, ICompleteAnswer answer);
        ICompleteAnswer ConvertToCompleteAnswer(IAnswer answer);
    }
}
