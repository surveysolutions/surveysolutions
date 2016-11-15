using System;
using System.Globalization;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;

using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.QuestionHeaderViewModelTests
{
    internal class when_variable_value_changed : QuestionHeaderViewModelTestsContext
    {
        Establish context = () =>
        {
            changedCulture = new ChangeCurrentCulture(CultureInfo.InvariantCulture);
            changedVariables = new[]
            {
                new ChangedVariable(new Identity(Guid.Parse("11111111111111111111111111111111"), RosterVector.Empty),  new DateTime(2016, 1, 31)),
                new ChangedVariable(new Identity(Guid.Parse("22222222222222222222222222222222"), RosterVector.Empty),  7.77m),
            };


            var interviewId = "interviewId";
            var substitutionTargetQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var substitutedVariable1Name = "var1";
            var substitutedVariable2Name = "var2";
            var substitutedVariable1Identity = new Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), RosterVector.Empty);
            var substitutedVariable2Identity = new Identity(Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC"), RosterVector.Empty);
            
            var interview = Mock.Of<IStatefulInterview>(x =>
                x.GetVariableValueByOrDeeperRosterLevel(substitutedVariable1Identity.Id, substitutedVariable1Identity.RosterVector) == changedVariables[0].NewValue &&
                x.GetVariableValueByOrDeeperRosterLevel(substitutedVariable2Identity.Id, substitutedVariable2Identity.RosterVector) == changedVariables[1].NewValue);

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewId) == interview);

            var questionnaireMock = Mock.Of<IQuestionnaire>(_
            => _.GetQuestionTitle(substitutionTargetQuestionId) == $"Your first variable is %{substitutedVariable1Name}% and second is %{substitutedVariable2Name}%"
            && _.GetQuestionInstruction(substitutionTargetQuestionId) == "Instruction"
            && _.GetVariableIdByVariableName(substitutedVariable1Name) == substitutedVariable1Identity.Id
            && _.HasVariable(substitutedVariable1Name) == true
            && _.GetVariableIdByVariableName(substitutedVariable2Name) == substitutedVariable2Identity.Id
            && _.HasVariable(substitutedVariable2Name) == true
            );

            var questionnaireRepository = new Mock<IQuestionnaireStorage>();
            questionnaireRepository.SetReturnsDefault(questionnaireMock);
           
            ILiteEventRegistry registry = Create.Service.LiteEventRegistry();
            liteEventBus = Create.Service.LiteEventBus(registry);

            viewModel = CreateViewModel(questionnaireRepository.Object, interviewRepository, registry);

            Identity id = new Identity(substitutionTargetQuestionId, Empty.RosterVector);
            viewModel.Init(interviewId, id);

            fakeInterview = Create.AggregateRoot.Interview();
        };

        Because of = () => liteEventBus.PublishCommittedEvents(new CommittedEventStream(fakeInterview.EventSourceId, 
            Create.Other.CommittedEvent(payload:new VariablesChanged(changedVariables), eventSourceId: fakeInterview.EventSourceId)));

        It should_change_item_title = () => viewModel.Title.HtmlText.ShouldEqual("Your first variable is 01/31/2016 and second is 7.77");

        Cleanup cleanup = () => changedCulture.Dispose();

        static QuestionHeaderViewModel viewModel;
        static ILiteEventBus liteEventBus;
        static IEventSourcedAggregateRoot fakeInterview;
        private static ChangedVariable[] changedVariables;
        public static ChangeCurrentCulture changedCulture;
    }
}

