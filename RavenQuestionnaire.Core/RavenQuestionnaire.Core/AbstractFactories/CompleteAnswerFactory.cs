using System;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Views.Answer;

namespace RavenQuestionnaire.Core.AbstractFactories
{
    public class CompleteAnswerFactory : ICompleteAnswerFactory
    {
        #region Implementation of IComepleteAnswerFactory

        public CompleteAnswerView CreateGroup(CompleteQuestionnaireDocument doc, ICompleteAnswer answer)
        {
            throw new NotImplementedException();
        }

        public ICompleteAnswer ConvertToCompleteAnswer(IAnswer answer)
        {
            var simpleAnswer = answer as Answer;
            if (simpleAnswer != null)
                return (CompleteAnswer)simpleAnswer;
            throw new ArgumentException();
        }

        #endregion
    }
}
