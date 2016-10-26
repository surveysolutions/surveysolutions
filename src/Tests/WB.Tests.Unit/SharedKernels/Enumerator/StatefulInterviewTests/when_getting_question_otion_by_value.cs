using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_getting_question_otion_by_value : StatefulInterviewTestsContext
    {
        private Establish context = () =>
        {

            var option1 = new CategoricalOption() {Value = 1, Title = "1"};
            options = new List<CategoricalOption>()
            {
                option1,
                new CategoricalOption() {Value = 1, Title = "2"}
            }.ToReadOnlyCollection();

            IQuestionnaireStorage questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId, 
                _ => _.GetOptionForQuestionByOptionValue(questionId, Moq.It.IsAny <decimal>()) == option1);

            var expressionState = new Mock<ILatestInterviewExpressionState>();

            expressionState.Setup(_ => _.FilterOptionsForQuestion(questionIdentity, options)).Returns(options);

            var interviewExpressionStatePrototypeProvider = Mock.Of<IInterviewExpressionStatePrototypeProvider>(
                provider => provider.GetExpressionState(questionnaireId, Moq.It.IsAny<long>()) == expressionState.Object);

            statefulInterview = Create.AggregateRoot.StatefulInterview(questionnaireId: questionnaireId, 
                questionnaireRepository: questionnaireRepository , 
                interviewExpressionStatePrototypeProvider: interviewExpressionStatePrototypeProvider);
        };

        Because of = () =>
        {
            categoricalOption = statefulInterview.GetOptionForQuestionWithoutFilter(questionIdentity, 1);
        };

        It should_question_option_be_not_null = () =>
            categoricalOption.ShouldNotBeNull();

        It should_question_option_title_as_expected = () =>
            categoricalOption.Title.ShouldEqual("1");

        static StatefulInterview statefulInterview;

        static readonly Guid questionId = Guid.Parse("11111111111111111111111111111113");
        static readonly Identity questionIdentity = new Identity(questionId, new decimal[0]);
        static ReadOnlyCollection<CategoricalOption> options;

        static CategoricalOption categoricalOption;
        static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111112");
        
    }
}
