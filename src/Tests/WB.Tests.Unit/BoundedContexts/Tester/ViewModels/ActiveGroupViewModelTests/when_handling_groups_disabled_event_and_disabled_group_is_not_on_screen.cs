using System;
using System.Collections.Generic;
using Cirrious.MvvmCross.Plugins.Messenger;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Tester.Implementation.Aggregates;
using WB.Core.BoundedContexts.Tester.Implementation.Entities;
using WB.Core.BoundedContexts.Tester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.Tester.Repositories;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.ActiveGroupViewModelTests
{
    internal class when_handling_groups_disabled_event_and_disabled_group_is_not_on_screen : ActiveGroupViewModelTestContext
    {
        Establish context = () =>
        {
            var interviewViewModelFactoryMock = new Mock<IInterviewViewModelFactory> { DefaultValue = DefaultValue.Mock };
            var mock = new Mock<GroupNavigationViewModel>() { DefaultValue = DefaultValue.Mock };

            interviewViewModelFactoryMock
                .Setup(x => x.GetNew<GroupNavigationViewModel>())
                .Returns(mock.Object);

            var interview = Mock.Of<IStatefulInterview>(_ => _.QuestionnaireId == questionnaireId);

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewId) == interview);

            var groupItems = new List<QuestionnaireReferenceModel>
                             {
                                 CreateQuestionnaireReferenceModel("22222222222222222222222222222222", typeof(TextQuestionModel)),
                                 CreateQuestionnaireReferenceModel("33333333333333333333333333333333", typeof(GroupModel)),
                                 CreateQuestionnaireReferenceModel("44444444444444444444444444444444", typeof(RosterModel)),
                                 CreateQuestionnaireReferenceModel("55555555555555555555555555555555", typeof(IntegerNumericQuestionModel)),
                                 CreateQuestionnaireReferenceModel("66666666666666666666666666666666", typeof(QRBarcodeQuestionModel))
                             };

            var groupModel = Mock.Of<GroupModel>(_ => _.Id == groupIdentity.Id && _.Title == "hello" && _.Children == groupItems);

            var questionnaireModel = Mock.Of<QuestionnaireModel>(_ =>
                _.GroupsWithFirstLevelChildrenAsReferences == new Dictionary<Guid, GroupModel>
                                                              {
                                                                  { groupIdentity.Id, groupModel }
                                                              });

            var questionnaireRepository = Mock.Of<IPlainKeyValueStorage<QuestionnaireModel>>(x => x.GetById(questionnaireId) == questionnaireModel);

            navigationState = Mock.Of<NavigationState>(_
                => _.InterviewId == interviewId
                   && _.QuestionnaireId == questionnaireId
                   && _.CurrentGroup == groupIdentity);

            activeGroup = CreateActiveGroupViewModel(
                interviewRepository: interviewRepository,
                questionnaireRepository: questionnaireRepository,
                eventRegistry: eventRegistry.Object,
                interviewViewModelFactory: interviewViewModelFactoryMock.Object,
                messenger: messengerMock.Object);

            activeGroup.Init(interviewId, navigationState);

            activeGroup.navigationState_OnGroupChanged(new GroupChangedEventArgs
                                                       {
                                                           TargetGroup = groupIdentity
                                                       });
        };

        Because of = () =>
            activeGroup.Handle(Create.Event.GroupsDisabled(disabledGroupIdentity.Id, disabledGroupIdentity.RosterVector));

        It should_publish_message_about_item_update_in_position_1 = () =>
            messengerMock.Verify(
                x => x.Publish(Moq.It.IsAny<UpdateInterviewEntityStateMessage>()),
                Times.Never);

        static ActiveGroupViewModel activeGroup;

        static readonly string interviewId = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

        protected static readonly string questionnaireId = "Questionnaire Id";

        protected static Identity groupIdentity = Create.Identity(Guid.Parse("11111111111111111111111111111111"), new decimal[] { 1, 2 });
        protected static Identity disabledGroupIdentity = Create.Identity(Guid.Parse("33333333333333333333333333333333"), new decimal[] { 1 });

        static NavigationState navigationState;

        static readonly Mock<ILiteEventRegistry> eventRegistry = new Mock<ILiteEventRegistry>();

        static readonly Mock<IMvxMessenger> messengerMock = new Mock<IMvxMessenger>();
    }
}