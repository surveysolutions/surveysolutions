using System;
using System.Linq;
using Machine.Specifications;
using Moq;
using Nito.AsyncEx.Synchronous;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.EnumerationStageViewModelTests
{
    // TODO: KP-7672
    //internal class when_handling_QuestionsDisabled_event_with_2_questions_and_one_of_that_question_is_set_to_be_hidden_if_disabled_and_another_is_not
    //{
    //    Establish context = () =>
    //    {
    //        var questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireIdentity, questionnaire
    //            => questionnaire.ShouldBeHiddenIfDisabled(disabledAndHideIfDisabledQuestion.Id) == true
    //            && questionnaire.ShouldBeHiddenIfDisabled(disabledAndNotHideIfDisabledQuestion.Id) == false);

    //        IStatefulInterviewRepository interviewRepository = Setup.StatefulInterviewRepository(
    //            Mock.Of<IStatefulInterview>(_ => _.QuestionnaireIdentity == questionnaireIdentity));

    //        var interviewViewModelFactory = Mock.Of<IInterviewViewModelFactory>(f =>
    //            f.GetNew<GroupNavigationViewModel>() == Mock.Of<GroupNavigationViewModel>());

    //        viemModel = Create.ViewModel.EnumerationStageViewModel(
    //            questionnaireRepository: questionnaireRepository,
    //            interviewRepository: interviewRepository,
    //            interviewViewModelFactory: interviewViewModelFactory,
    //            mvxMainThreadDispatcher: Stub.MvxMainThreadDispatcher());

    //        var groupId = new Identity(Guid.NewGuid(), new decimal[0]);
    //        viemModel.Init(interviewId, Create.Other.NavigationState(), groupId, null);

    //        viemModel.Items = new ObservableRangeCollection<IInterviewEntityViewModel>(new IInterviewEntityViewModel[]
    //        {
    //            Mock.Of<IInterviewEntityViewModel>(_ => _.Identity == disabledAndHideIfDisabledQuestion),
    //            Mock.Of<IInterviewEntityViewModel>(_ => _.Identity == disabledAndNotHideIfDisabledQuestion),
    //            Mock.Of<IInterviewEntityViewModel>(_ => _.Identity == enabledQuestion),
    //        });
    //    };

    //    Because of = () =>
    //        viemModel.Handle(
    //            Create.Event.QuestionsDisabled(new[]
    //            {
    //                disabledAndHideIfDisabledQuestion,
    //                disabledAndNotHideIfDisabledQuestion,
    //            }));

    //    It should_remove_disabled_question_which_is_set_to_be_hidden_if_disabled_from_items_list = () =>
    //        viemModel.Items.Select(entity => entity.Identity)
    //            .ShouldNotContain(disabledAndHideIfDisabledQuestion);

    //    It should_not_remove_disabled_question_which_is_not_set_to_be_hidden_if_disabled_from_items_list = () =>
    //        viemModel.Items.Select(entity => entity.Identity)
    //            .ShouldContain(disabledAndNotHideIfDisabledQuestion);

    //    It should_not_remove_enabled_question_which_is_set_to_be_hidden_if_disabled_from_items_list = () =>
    //        viemModel.Items.Select(entity => entity.Identity)
    //            .ShouldContain(enabledQuestion);

    //    private static EnumerationStageViewModel viemModel;
    //    private static Identity disabledAndHideIfDisabledQuestion = Create.Entity.Identity("DDDDDDDDDDDDDDDDD111111111111111", RosterVector.Empty);
    //    private static Identity disabledAndNotHideIfDisabledQuestion = Create.Entity.Identity("DDDDDDDDDDDDDDD00000000000000000", RosterVector.Empty);
    //    private static Identity enabledQuestion = Create.Entity.Identity("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE", RosterVector.Empty);
    //    private static QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), 99);
    //    private static string interviewId = "11111111111111111111111111111111";
    //}
}