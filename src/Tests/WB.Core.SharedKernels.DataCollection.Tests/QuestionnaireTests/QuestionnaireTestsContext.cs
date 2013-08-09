using System;
using System.Linq;
using Main.Core.Documents;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.SharedKernels.DataCollection.Tests.QuestionnaireTests
{
    public class QuestionnaireTestsContext
    {
        public static T GetSingleEvent<T>(EventContext eventContext)
        {
            return (T)eventContext.Events.Single(e => e.Payload is T).Payload;
        }

        public static T GetLastEvent<T>(EventContext eventContext)
        {
            return (T)eventContext.Events.Last(e => e.Payload is T).Payload;
        }

        public static Questionnaire CreateQuestionnaire()
        {
            return new Questionnaire(new Guid(), new QuestionnaireDocument());
        }
    }
}