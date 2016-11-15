using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.Enumerator.Aggregates;

using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.CascadingSingleOptionQuestionViewModelTests
{
    [Ignore("KP-8159")]
    internal class when_setting_FilterText_after_handling_SingleOptionQuestionAnswered_for_parent_question : CascadingSingleOptionQuestionViewModelTestContext
    {
        Establish context = () =>
        {
            SetUp();

            var childAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.IsAnswered == true && _.GetAnswer().SelectedValue == 3);
            var parentOptionAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.IsAnswered == true && _.GetAnswer().SelectedValue == 1);
            var secondParentOptionAnswer = Mock.Of<InterviewTreeSingleOptionQuestion>(_ => _.IsAnswered == true && _.GetAnswer().SelectedValue == 2);

            StatefulInterviewMock.Setup(x => x.Id).Returns(interviewGuid);
            StatefulInterviewMock.Setup(x => x.QuestionnaireIdentity).Returns(questionnaireId);
            StatefulInterviewMock.Setup(x => x.GetSingleOptionQuestion(questionIdentity)).Returns(childAnswer);
            StatefulInterviewMock.Setup(x => x.GetSingleOptionQuestion(parentIdentity)).Returns(parentOptionAnswer);

            StatefulInterviewMock.Setup(x => x.GetOptionForQuestionWithoutFilter(questionIdentity, 3, 1))
                .Returns(new CategoricalOption() {Title = "3", Value = 3, ParentValue = 1});

            StatefulInterviewMock.Setup(x => x.GetTopFilteredOptionsForQuestion(Moq.It.IsAny<Identity>(), Moq.It.IsAny<int?>(),
                Moq.It.IsAny<string>(), Moq.It.IsAny<int>()))
                .Returns((Identity identity, int? value, string filter, int count) => 
                    Options.Where(x => x.ParentValue == value && x.Title.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0).ToList());


            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewGuid.FormatGuid()) == StatefulInterviewMock.Object);

            var questionnaireRepository = SetupQuestionnaireRepositoryWithCascadingQuestion();

            var optionsRepository = SetupOptionsRepositoryForQuestionnaire(questionIdentity.Id);

            cascadingModel = CreateCascadingSingleOptionQuestionViewModel(
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository);

            cascadingModel.Init(interviewGuid.FormatGuid(), questionIdentity, navigationState);

            StatefulInterviewMock.Setup(x => x.GetSingleOptionQuestion(parentIdentity)).Returns(secondParentOptionAnswer);

            cascadingModel.Handle(Create.Event.SingleOptionQuestionAnswered(parentIdentity.Id, parentIdentity.RosterVector, 2));
        };

        Because of = () =>
            cascadingModel.FilterText = "c";

        It should_set_not_empty_list_in_AutoCompleteSuggestions = () =>
            cascadingModel.AutoCompleteSuggestions.ShouldNotBeEmpty();

        It should_set_2_options_in_AutoCompleteSuggestions = () =>
            cascadingModel.AutoCompleteSuggestions.Count.ShouldEqual(2);

        It should_format_first_option_in_AutoCompleteSuggestions = () =>
        {
            var firstOption = cascadingModel.AutoCompleteSuggestions.ElementAt(0);
            firstOption.Text.ShouldEqual("title <b>c</b><b>c</b><b>c</b> 5");
            firstOption.Value.ShouldEqual(5);
            firstOption.ParentValue.ShouldEqual(2);
            firstOption.OriginalText.ShouldEqual("title ccc 5");
        };

        It should_format_second_option_in_AutoCompleteSuggestions = () =>
        {
            var firstOption = cascadingModel.AutoCompleteSuggestions.ElementAt(1);
            firstOption.Text.ShouldEqual("title b<b>c</b>w 6");
            firstOption.Value.ShouldEqual(6);
            firstOption.ParentValue.ShouldEqual(2);
            firstOption.OriginalText.ShouldEqual("title bcw 6");
        };

        private static CascadingSingleOptionQuestionViewModel cascadingModel;
        protected static readonly List<CategoricalOption> options = new List<CategoricalOption>
        {
            Create.Entity.CategoricalQuestionOption(1, "title abc 1", 1),
            Create.Entity.CategoricalQuestionOption(2, "title def 2", 1),
            Create.Entity.CategoricalQuestionOption(3, "title klo 3", 1),
            Create.Entity.CategoricalQuestionOption(4, "title gha 4", 2),
            Create.Entity.CategoricalQuestionOption(5, "title ccc 5", 2),
            Create.Entity.CategoricalQuestionOption(6, "title bcw 6", 2)
        };

        private static readonly Mock<IStatefulInterview> StatefulInterviewMock = new Mock<IStatefulInterview>();
    }
}