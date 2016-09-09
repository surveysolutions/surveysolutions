using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using MvvmCross.Platform.Core;
using Nito.AsyncEx.Synchronous;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.EnumerationStageViewModelTests
{
    [Ignore("KP-7672")]
    internal class when_handling_StaticTextsEnabled_event
    {
        Establish context = () =>
        {
            var questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireIdentity, questionnaire
                => questionnaire.ShouldBeHiddenIfDisabled(disabledAndHideIfDisabledStaticText.Id) == true
                   && questionnaire.ShouldBeHiddenIfDisabled(disabledAndNotHideIfDisabledStaticText.Id) == false);

            var interviewRepository = Setup.StatefulInterviewRepository(Mock.Of<IStatefulInterview>(interview
                => interview.QuestionnaireIdentity == questionnaireIdentity
                   && interview.IsEnabled(disabledAndHideIfDisabledStaticText) == false
                   && interview.IsEnabled(disabledAndNotHideIfDisabledStaticText) == false
                   && interview.IsEnabled(enabledStaticText) == true));

            var interviewViewModelFactory = Mock.Of<IInterviewViewModelFactory>(__ =>
                __.GetEntities(Moq.It.IsAny<string>(), Moq.It.IsAny<Identity>(), Moq.It.IsAny<NavigationState>()) == new[]
                {
                    Mock.Of<IInterviewEntityViewModel>(_ => _.Identity == enabledStaticText),
                    Mock.Of<IInterviewEntityViewModel>(_ => _.Identity == disabledAndHideIfDisabledStaticText),
                    Mock.Of<IInterviewEntityViewModel>(_ => _.Identity == disabledAndNotHideIfDisabledStaticText),
                }.AsEnumerable()
                && __.GetNew<GroupNavigationViewModel>() == Mock.Of<GroupNavigationViewModel>());

            var mvxMainThreadDispatcher = Stub.MvxMainThreadDispatcher();
            Setup.InstanceToMockedServiceLocator<IMvxMainThreadDispatcher>(mvxMainThreadDispatcher);

            viemModel = Create.ViewModel.EnumerationStageViewModel(
                questionnaireRepository: questionnaireRepository,
                interviewViewModelFactory: interviewViewModelFactory,
                interviewRepository: interviewRepository,
                mvxMainThreadDispatcher: mvxMainThreadDispatcher);

            var groupId = new Identity(Guid.NewGuid(), new decimal[0]);
            viemModel.Init(interviewId, Create.Other.NavigationState(), groupId, null);
            (viemModel.Items as INotifyCollectionChanged).CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Replace)
                    updateItemCollection.Set();
            };
        };

        Because of = () =>
            viemModel.Handle(Create.Event.StaticTextsEnabled(enabledStaticText, disabledAndNotHideIfDisabledStaticText));

        It should_put_enabled_static_text_to_items_list = () =>
            viemModel.Items.OfType<IInterviewEntityViewModel>().Select(entity => entity.Identity).ShouldContain(enabledStaticText);

        It should_not_put_disabled_static_text_which_is_set_to_be_hidden_if_disabled_to_items_list = () =>
            viemModel.Items.OfType<IInterviewEntityViewModel>().Select(entity => entity.Identity).ShouldNotContain(disabledAndHideIfDisabledStaticText);

        It should_put_disabled_static_text_which_is_not_set_to_be_hidden_if_disabled_to_items_list = () =>
            viemModel.Items.OfType<IInterviewEntityViewModel>().Select(entity => entity.Identity).ShouldContain(disabledAndNotHideIfDisabledStaticText);

        It should_raise_the_collection_item_changed_event = () => 
            updateItemCollection.WaitOne(TimeSpan.FromMilliseconds(100)).ShouldBeTrue();

        private static EnumerationStageViewModel viemModel;
        private static Identity enabledStaticText = Create.Entity.Identity("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE", RosterVector.Empty);
        private static Identity disabledAndHideIfDisabledStaticText = Create.Entity.Identity("DDDDDDDDDDDDDDDDD111111111111111", RosterVector.Empty);
        private static Identity disabledAndNotHideIfDisabledStaticText = Create.Entity.Identity("DDDDDDDDDDDDDDD00000000000000000", RosterVector.Empty);
        private static QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), 99);
        private static string interviewId = "11111111111111111111111111111111";
        private static readonly ManualResetEvent updateItemCollection = new ManualResetEvent(false);
    }
}