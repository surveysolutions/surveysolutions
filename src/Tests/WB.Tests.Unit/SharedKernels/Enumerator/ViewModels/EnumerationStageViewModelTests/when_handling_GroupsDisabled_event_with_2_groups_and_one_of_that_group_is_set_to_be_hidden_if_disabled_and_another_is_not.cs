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
    //internal class when_handling_GroupsDisabled_event_with_2_groups_and_one_of_that_group_is_set_to_be_hidden_if_disabled_and_another_is_not
    //{
    //    Establish context = () =>
    //    {
    //        var questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireIdentity, questionnaire
    //            => questionnaire.ShouldBeHiddenIfDisabled(disabledAndHideIfDisabledGroup.Id) == true
    //            && questionnaire.ShouldBeHiddenIfDisabled(disabledAndNotHideIfDisabledGroup.Id) == false);

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
    //            Mock.Of<IInterviewEntityViewModel>(_ => _.Identity == disabledAndHideIfDisabledGroup),
    //            Mock.Of<IInterviewEntityViewModel>(_ => _.Identity == disabledAndNotHideIfDisabledGroup),
    //            Mock.Of<IInterviewEntityViewModel>(_ => _.Identity == enabledGroup),
    //        });
    //    };

    //    Because of = () =>
    //        viemModel.Handle(
    //            Create.Event.GroupsDisabled(new[]
    //            {
    //                disabledAndHideIfDisabledGroup,
    //                disabledAndNotHideIfDisabledGroup,
    //            }));

    //    It should_remove_disabled_group_which_is_set_to_be_hidden_if_disabled_from_items_list = () =>
    //        viemModel.Items.Select(entity => entity.Identity)
    //            .ShouldNotContain(disabledAndHideIfDisabledGroup);

    //    It should_not_remove_disabled_group_which_is_not_set_to_be_hidden_if_disabled_from_items_list = () =>
    //        viemModel.Items.Select(entity => entity.Identity)
    //            .ShouldContain(disabledAndNotHideIfDisabledGroup);

    //    It should_not_remove_enabled_group_which_is_set_to_be_hidden_if_disabled_from_items_list = () =>
    //        viemModel.Items.Select(entity => entity.Identity)
    //            .ShouldContain(enabledGroup);

    //    private static EnumerationStageViewModel viemModel;
    //    private static Identity disabledAndHideIfDisabledGroup = Create.Entity.Identity("DDDDDDDDDDDDDDDDD111111111111111", RosterVector.Empty);
    //    private static Identity disabledAndNotHideIfDisabledGroup = Create.Entity.Identity("DDDDDDDDDDDDDDD00000000000000000", RosterVector.Empty);
    //    private static Identity enabledGroup = Create.Entity.Identity("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE", RosterVector.Empty);
    //    private static QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), 99);
    //    private static string interviewId = "11111111111111111111111111111111";
    //}
}