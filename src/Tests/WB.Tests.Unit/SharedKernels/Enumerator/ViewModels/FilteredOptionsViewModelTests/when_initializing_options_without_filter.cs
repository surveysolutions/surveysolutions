using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;



namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.FilteredOptionsViewModelTests
{
    internal class when_initializing_options_without_filter : FilteredOptionsViewModelTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            interviewId = "interview";
            questionGuid = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionId = Create.Entity.Identity(questionGuid, Empty.RosterVector);

            var questionnaire = Mock.Of<IQuestionnaire>(_
                => _.IsSupportFilteringForOptions(questionId.Id) == false
            );

            options = new[]
            {
                new CategoricalOption() {Title = "a", Value = 1},
                new CategoricalOption() {Title = "b", Value = 2},
                new CategoricalOption() {Title = "c", Value = 3},
            }.ToList();

            var interview = Mock.Of<IStatefulInterview>(x => x.GetTopFilteredOptionsForQuestion(questionId, null, string.Empty, 200) == options);

            var questionnaireStorage = new Mock<IQuestionnaireStorage>();
            var interviewRepository = new Mock<IStatefulInterviewRepository>();

            questionnaireStorage.SetReturnsDefault(questionnaire);
            interviewRepository.SetReturnsDefault(interview);

            answerNotifier = new Mock<AnswerNotifier>();

            viewModel = CreateViewModel(questionnaireRepository: questionnaireStorage.Object, 
                interviewRepository: interviewRepository.Object,
                answerNotifier:  answerNotifier.Object);
            BecauseOf();
        }

        public void BecauseOf() => viewModel.Init(interviewId, questionId, 200);

        [NUnit.Framework.Test] public void should_build_options () 
        {
            viewModel.GetOptions().Should().NotBeNull();
            viewModel.GetOptions().Count().Should().Be(3);
        }

        [NUnit.Framework.Test] public void should_contains_all_options () 
        {
            viewModel.GetOptions().Should().BeEquivalentTo(options);
        }

        [NUnit.Framework.Test] public void should_not_subscribe_model_in_answerNotify () 
        {
            answerNotifier.Verify(x => x.Init(interviewId), Times.Never());
        }

        static FilteredOptionsViewModel viewModel;
        static string interviewId;
        static Identity questionId;
        private static Guid questionGuid;
        private static List<CategoricalOption> options;
        private static Mock<AnswerNotifier> answerNotifier;
    }
}

