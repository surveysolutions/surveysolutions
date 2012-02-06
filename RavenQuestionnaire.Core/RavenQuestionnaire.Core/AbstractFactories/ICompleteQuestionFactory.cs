using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Views.Question;

namespace RavenQuestionnaire.Core.AbstractFactories
{
    public interface ICompleteQuestionFactory
    {
        CompleteQuestionView CreateGroup(CompleteQuestionnaireDocument doc, ICompleteQuestion question);
        ICompleteQuestion ConvertToCompleteQuestion(IQuestion question);
    }
}
