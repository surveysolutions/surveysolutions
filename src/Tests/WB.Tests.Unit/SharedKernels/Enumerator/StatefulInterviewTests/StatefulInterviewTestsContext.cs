using System;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    [TestOf(typeof(StatefulInterview))]
    internal class StatefulInterviewTestsContext
    {
        protected static AnsweredQuestionSynchronizationDto CreateAnsweredQuestionSynchronizationDto(Guid questionId, decimal[] rosterVector, object answer)
        {
            return Create.Entity.AnsweredQuestionSynchronizationDto(questionId, rosterVector, answer);
        }

        protected static IQuestionnaireStorage CreateQuestionnaireRepositoryStubWithOneQuestionnaire(Guid questionnaireId, IQuestionnaire questionaire = null)
        {
            return Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId, questionaire);
        }
    }
}
