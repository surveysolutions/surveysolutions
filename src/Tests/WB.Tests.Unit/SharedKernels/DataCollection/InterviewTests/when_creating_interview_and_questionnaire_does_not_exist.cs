using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_creating_interview_and_questionnaire_does_not_exist : InterviewTestsContext
    {
        Establish context = () =>
        {
            questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            responsibleSupervisorId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAA00");
            answersToFeaturedQuestions = new Dictionary<Guid, AbstractAnswer>();

            var repositoryWithoutQuestionnaire = Mock.Of<IQuestionnaireStorage>();

            interview = Create.AggregateRoot.Interview(questionnaireRepository: repositoryWithoutQuestionnaire);
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                interview.CreateInterview(questionnaireId, 1, responsibleSupervisorId, answersToFeaturedQuestions, DateTime.Now, userId));

        It should_throw_interview_exception = () =>
            exception.ShouldBeOfExactType<InterviewException>();

        private static Exception exception;
        private static Guid questionnaireId;
        private static Guid userId;
        private static Guid responsibleSupervisorId;
        private static Dictionary<Guid, AbstractAnswer> answersToFeaturedQuestions;
        private static Interview interview;
    }
}
