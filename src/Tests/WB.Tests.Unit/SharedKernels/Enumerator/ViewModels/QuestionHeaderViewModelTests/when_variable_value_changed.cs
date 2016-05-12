using System;
using System.Globalization;
using System.Threading;
using Machine.Specifications;
using Main.Core.Entities.Composite;
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
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.QuestionHeaderViewModelTests
{
    internal class when_variable_value_changed : QuestionHeaderViewModelTestsContext
    {
        Establish context = () =>
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            changedVariables = new[]
            {
                new ChangedVariableValueDto(new Identity(Guid.Parse("11111111111111111111111111111111"), RosterVector.Empty),  new DateTime(2016, 1, 31)),
                new ChangedVariableValueDto(new Identity(Guid.Parse("22222222222222222222222222222222"), RosterVector.Empty),  7.77m),
            };


            var interviewId = "interviewId";
            var substitutionTargetQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var substitutedVariable1Name = "var1";
            var substitutedVariable2Name = "var2";
            var substitutedVariable1Identity = new Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), RosterVector.Empty);
            var substitutedVariable2Identity = new Identity(Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC"), RosterVector.Empty);

            var answer = new TextAnswer();
            answer.SetAnswer("new value");
            var interview = Mock.Of<IStatefulInterview>(x =>
                x.GetVariableValue(substitutedVariable1Identity) == changedVariables[0].VariableValue &&
                x.GetVariableValue(substitutedVariable2Identity) == changedVariables[1].VariableValue);

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewId) == interview);

            var questionnaireMock = Mock.Of<IQuestionnaire>(_
            => _.GetQuestionTitle(substitutionTargetQuestionId) == $"Your first variable is %{substitutedVariable1Name}% and second is %{substitutedVariable2Name}%"
            && _.GetQuestionInstruction(substitutionTargetQuestionId) == "Instruction"
            && _.GetVariableIdByVariableName(substitutedVariable1Name) == substitutedVariable1Identity.Id
            && _.HasVariable(substitutedVariable1Name) == true
            && _.GetVariableIdByVariableName(substitutedVariable2Name) == substitutedVariable2Identity.Id
            && _.HasVariable(substitutedVariable2Name) == true
            );

            var questionnaireRepository = new Mock<IPlainQuestionnaireRepository>();
            questionnaireRepository.SetReturnsDefault(questionnaireMock);
           
            ILiteEventRegistry registry = Create.LiteEventRegistry();
            liteEventBus = Create.LiteEventBus(registry);

            viewModel = CreateViewModel(questionnaireRepository.Object, interviewRepository, registry);

            Identity id = new Identity(substitutionTargetQuestionId, Empty.RosterVector);
            viewModel.Init(interviewId, id);

            fakeInterview = Create.Interview();
        };

        Because of = () => liteEventBus.PublishCommittedEvents(new CommittedEventStream(fakeInterview.EventSourceId, 
            Create.CommittedEvent(payload:new VariablesValuesChanged(changedVariables), eventSourceId: fakeInterview.EventSourceId)));

        It should_change_item_title = () => viewModel.Title.ShouldEqual($"Your first variable is 1/31/2016 and second is 7.77");

        static QuestionHeaderViewModel viewModel;
        static ILiteEventBus liteEventBus;
        static IEventSourcedAggregateRoot fakeInterview;
        private static ChangedVariableValueDto[] changedVariables;
    }
}

