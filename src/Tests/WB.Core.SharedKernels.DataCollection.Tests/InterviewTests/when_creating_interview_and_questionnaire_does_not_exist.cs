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
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    internal class when_creating_interview_and_questionnaire_does_not_exist : InterviewTestsContext
    {
        Establish context = () =>
        {
            questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            answersToFeaturedQuestions = new Dictionary<Guid, object>();

            var repositoryWithoutQuestionnaire = Mock.Of<IQuestionnaireRepository>(repository
                => repository.GetQuestionnaire(questionnaireId) == null as IQuestionnaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(repositoryWithoutQuestionnaire);
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                new Interview(userId, questionnaireId, answersToFeaturedQuestions, DateTime.Now));

        It should_throw_interview_exception = () =>
            exception.ShouldBeOfType<InterviewException>();

        private static Exception exception;
        private static Guid questionnaireId;
        private static Guid userId;
        private static Dictionary<Guid, object> answersToFeaturedQuestions;
    }
}
