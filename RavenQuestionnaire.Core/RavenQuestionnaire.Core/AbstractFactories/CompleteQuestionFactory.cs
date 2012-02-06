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
    public class CompleteQuestionFactory : ICompleteQuestionFactory
    {
        #region Implementation of ICompleteQuestionFactory

        public CompleteQuestionView CreateGroup(CompleteQuestionnaireDocument doc, ICompleteQuestion question)
        {
            throw new NotImplementedException();
        }

        public ICompleteQuestion ConvertToCompleteQuestion(IQuestion question)
        {
            var simpleQuestion = question as Question;
            if (simpleQuestion != null)
                return (CompleteQuestion) simpleQuestion;
            var bindedQuestion = question as BindedQuestion;
            if(bindedQuestion!=null)
                return new CompleteQuestion();
            throw new ArgumentException();
        }

        #endregion
    }
}
