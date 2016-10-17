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
    internal class when_getting_question_otions : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            options = new List<CategoricalOption>()
            {
                new CategoricalOption() {Value = 1, Title = "1"},
                new CategoricalOption() {Value = 1, Title = "2"}
            }.ToReadOnlyCollection();

            IQuestionnaireStorage questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId, 
                _ => _.GetOptionsForQuestion(questionId, null, String.Empty) == options);

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
            categoricalOptions =
                statefulInterview.GetFilteredOptionsForQuestion(questionIdentity, null, string.Empty).ToList();
        };

        It should_contains_2_elements = () =>
            categoricalOptions.Count().ShouldEqual(2);

        It should_question_options = () =>
            categoricalOptions.ShouldEqual(options.ToList());

        static StatefulInterview statefulInterview;

        static readonly Guid questionId = Guid.Parse("11111111111111111111111111111113");
        static readonly Identity questionIdentity = new Identity(questionId, new decimal[0]);
        static ReadOnlyCollection<CategoricalOption> options;

        static List<CategoricalOption> categoricalOptions;
        static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111112");
        
    }
}
