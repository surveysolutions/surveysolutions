using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using NSubstitute;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.Enumerator.Aggregates;

using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.CascadingSingleOptionQuestionViewModelTests
{
    internal class when_handling_SingleOptionQuestionAnswered_for_parent_question : CascadingSingleOptionQuestionViewModelTestContext
    {
        Establish context = () =>
        {
            SetUp();
            var childAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.IsAnswered == true && _.GetAnswer() == Create.Entity.SingleOptionAnswer(answerOnChildQuestion));
            var parentOptionAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.IsAnswered == true && _.GetAnswer() == Create.Entity.SingleOptionAnswer(1));
            var secondParentOptionAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.IsAnswered == true && _.GetAnswer() == Create.Entity.SingleOptionAnswer(2));

            StatefulInterviewMock.Setup(x => x.Id).Returns(interviewGuid);
            StatefulInterviewMock.Setup(x => x.QuestionnaireIdentity).Returns(questionnaireId);
            StatefulInterviewMock.Setup(x => x.GetSingleOptionQuestion(questionIdentity)).Returns(childAnswer);
            StatefulInterviewMock.Setup(x => x.GetSingleOptionQuestion(parentIdentity)).Returns(parentOptionAnswer);

            StatefulInterviewMock.Setup(x => x.GetOptionForQuestionWithoutFilter(questionIdentity, 3, 1))
                .Returns(new CategoricalOption() { Title = "3", Value = 3 , ParentValue = 1});

            StatefulInterviewMock.Setup(x => x.GetTopFilteredOptionsForQuestion(questionIdentity, 2, string.Empty, Moq.It.IsAny<int>()))
                .Returns(Options.Where(x => x.ParentValue == 2).ToList());

            StatefulInterviewMock.Setup(x => x.GetTopFilteredOptionsForQuestion(questionIdentity, 1, "3", Moq.It.IsAny<int>()))
                .Returns(Options.Where(x => x.ParentValue == 1).ToList());

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewGuid.FormatGuid()) == StatefulInterviewMock.Object);

            var questionnaireRepository = SetupQuestionnaireRepositoryWithCascadingQuestion();

            cascadingModel = CreateCascadingSingleOptionQuestionViewModel(
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository);

            cascadingModel.Init(interviewGuid.FormatGuid(), questionIdentity, navigationState);

            StatefulInterviewMock.Setup(x => x.GetSingleOptionQuestion(parentIdentity)).Returns(secondParentOptionAnswer);
        };

        Because of = () =>
            cascadingModel.Handle(Create.Event.SingleOptionQuestionAnswered(parentIdentity.Id, parentIdentity.RosterVector, 2));

        It should_set_not_empty_list_in_AutoCompleteSuggestions = () =>
            cascadingModel.AutoCompleteSuggestions.ShouldNotBeEmpty();

        It should_set_3_items_in_AutoCompleteSuggestions = () =>
            cascadingModel.AutoCompleteSuggestions.Count.ShouldEqual(3);

        It should_create_option_models_with_specified_Texts = () =>
            cascadingModel.AutoCompleteSuggestions.Select(x => x.Text).ShouldContainOnly(OptionsIfParentAnswerIs2.Select(x => x.Title));

        It should_create_option_models_with_specified_OriginalTexts = () =>
            cascadingModel.AutoCompleteSuggestions.Select(x => x.OriginalText).ShouldContainOnly(OptionsIfParentAnswerIs2.Select(x => x.Title));

        It should_create_option_models_with_specified_values = () =>
            cascadingModel.AutoCompleteSuggestions.Select(x => Convert.ToInt32(x.Value)).ShouldContainOnly(OptionsIfParentAnswerIs2.Select(x => x.Value));

        It should_create_option_models_with_specified_ParentValues = () =>
            cascadingModel.AutoCompleteSuggestions.Select(x => Convert.ToInt32(x.ParentValue)).ShouldContainOnly(OptionsIfParentAnswerIs2.Select(x => x.ParentValue.Value));

        private static readonly List<CategoricalOption> OptionsIfParentAnswerIs2 = Options.Where(x => x.ParentValue == 2).ToList();

        private static CascadingSingleOptionQuestionViewModel cascadingModel;

        private static readonly Mock<IStatefulInterview> StatefulInterviewMock = new Mock<IStatefulInterview>();

        private static readonly int answerOnChildQuestion = 3;
    }
}