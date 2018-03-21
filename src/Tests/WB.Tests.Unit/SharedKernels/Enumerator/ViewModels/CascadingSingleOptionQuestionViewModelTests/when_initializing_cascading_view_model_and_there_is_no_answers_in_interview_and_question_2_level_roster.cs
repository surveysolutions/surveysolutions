using FluentAssertions;
using Moq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.CascadingSingleOptionQuestionViewModelTests
{
    internal class when_initializing_cascading_view_model_and_there_is_no_answers_in_interview_and_question_2_level_roster : CascadingSingleOptionQuestionViewModelTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            SetUp();

            var singleOptionAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.IsAnswered() == false);
            var parentOptionAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.IsAnswered() == false);

            var interview = Mock.Of<IStatefulInterview>(_ 
                => _.GetSingleOptionQuestion(questionIdentity) == singleOptionAnswer
                   && _.GetSingleOptionQuestion(parentIdentity) == parentOptionAnswer);

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewId) == interview);

            var questionnaireRepository = SetupQuestionnaireRepositoryWithCascadingQuestion();

            cascadingModel = CreateCascadingSingleOptionQuestionViewModel(
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository);
            BecauseOf();
        }

        public void BecauseOf() =>
            cascadingModel.Init(interviewId, questionIdentity, navigationState);

        [NUnit.Framework.Test] public void should_initialize_question_state () =>
            QuestionStateMock.Verify(x => x.Init(interviewId, questionIdentity, navigationState), Times.Once);

        [NUnit.Framework.Test] public void should_subscribe_for_events () =>
            EventRegistry.Verify(x => x.Subscribe(cascadingModel, Moq.It.IsAny<string>()), Times.Once);

        [NUnit.Framework.Test] public void should_not_set_filter_text () =>
            cascadingModel.FilterText.Should().BeEmpty();

        [NUnit.Framework.Test] public void should_set_empty_list_in_AutoCompleteSuggestions () =>
            cascadingModel.AutoCompleteSuggestions.Should().BeEmpty();

        private static CascadingSingleOptionQuestionViewModel cascadingModel;
    }
}
