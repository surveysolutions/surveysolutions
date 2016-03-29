using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using Nito.AsyncEx.Synchronous;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.CascadingSingleOptionQuestionViewModelTests
{
    internal class when_setting_FilterText_in_null : CascadingSingleOptionQuestionViewModelTestContext
    {
        Establish context = () =>
        {
            SetUp();

            var childAnswer = Mock.Of<SingleOptionAnswer>(_ => _.IsAnswered == true && _.Answer == 3);
            var parentOptionAnswer = Mock.Of<SingleOptionAnswer>(_ => _.IsAnswered == true && _.Answer == 1);

            var interview = Mock.Of<IStatefulInterview>(_
                => _.QuestionnaireId == questionnaireId
                   && _.GetSingleOptionAnswer(questionIdentity) == childAnswer
                   && _.GetSingleOptionAnswer(parentIdentity) == parentOptionAnswer);

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewId) == interview);

            var questionnaireRepository = SetupQuestionnaireRepositoryWithCascadingQuestion();

            cascadingModel = CreateCascadingSingleOptionQuestionViewModel(
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository);

            cascadingModel.InitAsync(interviewId, questionIdentity, navigationState).WaitAndUnwrapException();
        };

        Because of = () =>
            cascadingModel.FilterText = null;

        It should_set_null_filter_text = () =>
            cascadingModel.FilterText.ShouldBeNull();

        It should_set_not_empty_list_in_AutoCompleteSuggestions = () =>
            cascadingModel.AutoCompleteSuggestions.ShouldNotBeEmpty();

        It should_set_3_items_in_AutoCompleteSuggestions = () =>
            cascadingModel.AutoCompleteSuggestions.Count.ShouldEqual(3);

        It should_create_option_models_with_specified_Texts = () =>
            cascadingModel.AutoCompleteSuggestions.Select(x => x.Text).ShouldContainOnly(OptionsIfParentAnswerIs1.Select(x => x.Title));

        It should_create_option_models_with_specified_OriginalTexts = () =>
            cascadingModel.AutoCompleteSuggestions.Select(x => x.OriginalText).ShouldContainOnly(OptionsIfParentAnswerIs1.Select(x => x.Title));

        It should_create_option_models_with_specified_values = () =>
            cascadingModel.AutoCompleteSuggestions.Select(x => x.Value).ShouldContainOnly(OptionsIfParentAnswerIs1.Select(x => x.Value));

        It should_create_option_models_with_specified_ParentValues = () =>
            cascadingModel.AutoCompleteSuggestions.Select(x => x.ParentValue).ShouldContainOnly(OptionsIfParentAnswerIs1.Select(x => x.ParentValue.Value));


        private static CascadingSingleOptionQuestionViewModel cascadingModel;

        private static readonly List<CategoricalQuestionOption> OptionsIfParentAnswerIs1 = Options.Where(x => x.ParentValue == 1).ToList();
    }
}