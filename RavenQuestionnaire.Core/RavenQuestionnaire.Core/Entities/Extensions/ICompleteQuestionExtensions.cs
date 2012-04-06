using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.AbstractFactories;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Entities.Extensions
{
    public static class ICompleteQuestionExtensions
    {
        public static object GetValue(this ICompleteQuestion question)
        {
            if (question == null)
                return null;
            var factory = new CompleteQuestionFactory();
            return factory.GetAnswerValue(question);

        }
       
   //     public static ICompleteQuestion 
    }
}
