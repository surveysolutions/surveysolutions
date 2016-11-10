using System;
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
    internal class when_variable_value_and_substitution_title_changed : QuestionHeaderViewModelTestsContext
    {
        Establish context = () =>
        {
            var interviewId = "interviewId";
            var substitutionTargetQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var substitutedVariable1Name = "var1";
            var substitutedVariable2Name = "var2";
            var substitutedQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var substitutedVariableIdentity = new Identity(Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC"), RosterVector.Empty);

            changedVariables = new[]{new ChangedVariable(new Identity(Guid.Parse("11111111111111111111111111111111"), RosterVector.Empty),  555)};
            changedTitleIds =new []{new Identity(substitutionTargetQuestionId, Empty.RosterVector)};

            answer = new TextAnswer();
            answer.SetAnswer("new value");
            var interview = Mock.Of<IStatefulInterview>(x =>
                x.GetVariableValueByOrDeeperRosterLevel(substitutedVariableIdentity.Id, substitutedVariableIdentity.RosterVector) == changedVariables[0].NewValue &&
                x.FindBaseAnswerByOrDeeperRosterLevel(substitutedQuestionId, Empty.RosterVector) == answer);

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewId) == interview);

            var questionnaireMock = Mock.Of<IQuestionnaire>(_
            => _.GetQuestionTitle(substitutionTargetQuestionId) == $"Your answer on question is %{substitutedVariable1Name}% and variable is %{substitutedVariable2Name}%"
            && _.GetQuestionInstruction(substitutionTargetQuestionId) == "Instruction"
            && _.GetQuestionIdByVariable(substitutedVariable1Name) == substitutedQuestionId
            && _.HasQuestion(substitutedVariable1Name) == true
            && _.GetVariableIdByVariableName(substitutedVariable2Name) == substitutedVariableIdentity.Id
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
            Create.Other.CommittedEvent(payload:new VariablesChanged(changedVariables), eventSourceId: fakeInterview.EventSourceId, eventSequence: 1),
            Create.Other.CommittedEvent(payload: Create.Event.SubstitutionTitlesChanged(questions: changedTitleIds), eventSourceId: fakeInterview.EventSourceId, eventSequence: 2)));

        It should_change_item_title = () => viewModel.Title.HtmlText.ShouldEqual($"Your answer on question is {answer.Answer} and variable is {changedVariables[0].NewValue}");

        static QuestionHeaderViewModel viewModel;
        static ILiteEventBus liteEventBus;
        static IEventSourcedAggregateRoot fakeInterview;
        private static ChangedVariable[] changedVariables;
        private static Identity[] changedTitleIds;
        static TextAnswer answer;
    }
}

