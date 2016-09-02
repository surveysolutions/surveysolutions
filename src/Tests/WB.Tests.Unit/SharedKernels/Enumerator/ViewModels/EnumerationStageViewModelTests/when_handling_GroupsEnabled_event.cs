using System;
using System.Linq;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Nito.AsyncEx.Synchronous;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.EnumerationStageViewModelTests
{
    internal class when_handling_GroupsEnabled_event
    {
        Establish context = () =>
        {
            var questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireIdentity, questionnaire
                => questionnaire.ShouldBeHiddenIfDisabled(disabledAndHideIfDisabledGroup.Id) == true
                && questionnaire.ShouldBeHiddenIfDisabled(disabledAndNotHideIfDisabledGroup.Id) == false);

            var interviewRepository = Setup.StatefulInterviewRepository(Mock.Of<IStatefulInterview>(interview
                => interview.QuestionnaireIdentity == questionnaireIdentity
                && interview.IsEnabled(disabledAndHideIfDisabledGroup) == false
                && interview.IsEnabled(disabledAndNotHideIfDisabledGroup) == false
                && interview.IsEnabled(enabledGroup) == true));

            var interviewViewModelFactory = Mock.Of<IInterviewViewModelFactory>(__ =>
                __.GetEntities(Moq.It.IsAny<string>(), Moq.It.IsAny<Identity>(), Moq.It.IsAny<NavigationState>()) == new[]
                {
                    Mock.Of<IInterviewEntityViewModel>(_ => _.Identity == enabledGroup),
                    Mock.Of<IInterviewEntityViewModel>(_ => _.Identity == disabledAndHideIfDisabledGroup),
                    Mock.Of<IInterviewEntityViewModel>(_ => _.Identity == disabledAndNotHideIfDisabledGroup),
                }.AsEnumerable()
                && __.GetNew<GroupNavigationViewModel>() == Mock.Of<GroupNavigationViewModel>());

            viemModel = Create.ViewModel.EnumerationStageViewModel(
                questionnaireRepository: questionnaireRepository,
                interviewViewModelFactory: interviewViewModelFactory,
                interviewRepository: interviewRepository,
                mvxMainThreadDispatcher: Stub.MvxMainThreadDispatcher(),
                compositeCollectionInflationService: new CompositeCollectionInflationService());

            var groupId = new Identity(Guid.NewGuid(), new decimal[0]);
            viemModel.Init(interviewId, Create.Other.NavigationState(), groupId, null);
        };

        Because of = () =>
            viemModel.Handle(
                Create.Event.GroupsEnabled());

        It should_put_enabled_group_to_items_list = () =>
            viemModel.Items.OfType<IInterviewEntityViewModel>().Select(entity => entity.Identity)
                .ShouldContain(enabledGroup);

        It should_not_put_disabled_group_which_is_set_to_be_hidden_if_disabled_to_items_list = () =>
            viemModel.Items.OfType<IInterviewEntityViewModel>().Select(entity => entity.Identity)
                .ShouldNotContain(disabledAndHideIfDisabledGroup);

        It should_put_disabled_group_which_is_not_set_to_be_hidden_if_disabled_to_items_list = () =>
            viemModel.Items.OfType<IInterviewEntityViewModel>().Select(entity => entity.Identity)
                .ShouldContain(disabledAndNotHideIfDisabledGroup);

        private static EnumerationStageViewModel viemModel;
        private static Identity enabledGroup = Create.Entity.Identity("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE", RosterVector.Empty);
        private static Identity disabledAndHideIfDisabledGroup = Create.Entity.Identity("DDDDDDDDDDDDDDDDD111111111111111", RosterVector.Empty);
        private static Identity disabledAndNotHideIfDisabledGroup = Create.Entity.Identity("DDDDDDDDDDDDDDD00000000000000000", RosterVector.Empty);
        private static QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), 99);
        private static string interviewId = "11111111111111111111111111111111";
    }
}