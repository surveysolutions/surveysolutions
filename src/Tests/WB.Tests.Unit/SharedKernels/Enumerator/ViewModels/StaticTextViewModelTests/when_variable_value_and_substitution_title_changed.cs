using System;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Moq;
using Ncqrs.Eventing;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.StaticTextViewModelTests
{
    internal class when_variable_value_and_substitution_title_changed : StaticTextViewModelTestsContext
    {
        Establish context = () =>
        {
            var interviewId = "interviewId";
            var staticTextWithSubstitutionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var substitutedVariable1Name = "var1";
            var substitutedVariable2Name = "var2";
            var substitutedQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var substitutedVariableIdentity = new Identity(Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC"), RosterVector.Empty);

            changedVariables = new[]{new ChangedVariable(new Identity(Guid.Parse("11111111111111111111111111111111"), RosterVector.Empty),  555)};
            changedTitleIds =new []{new Identity(staticTextWithSubstitutionId, Empty.RosterVector)};

            answer = new TextAnswer();
            answer.SetAnswer("new value");
            var interview = Mock.Of<IStatefulInterview>(x =>
                x.GetVariableValueByOrDeeperRosterLevel(substitutedVariableIdentity.Id, substitutedVariableIdentity.RosterVector) == changedVariables[0].NewValue &&
                x.FindBaseAnswerByOrDeeperRosterLevel(substitutedQuestionId, Empty.RosterVector) == answer);

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewId) == interview);


            var questionnaireMock = Create.PlainQuestionnaire(Create.QuestionnaireDocument(children: new IComposite[]
            {
                Create.StaticText(publicKey: staticTextWithSubstitutionId, text: $"Your answer on question is %{substitutedVariable1Name}% and variable is %{substitutedVariable2Name}%"),
                Create.NumericRealQuestion(variable: substitutedVariable1Name, id: substitutedQuestionId),
                Create.Variable(variableName: substitutedVariable2Name, id: substitutedVariableIdentity.Id)
            }));

            var questionnaireRepository = new Mock<IPlainQuestionnaireRepository>();
            questionnaireRepository.Setup(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>())).Returns(questionnaireMock);

            ILiteEventRegistry registry = Create.LiteEventRegistry();
            liteEventBus = Create.LiteEventBus(registry);

            viewModel = CreateViewModel(questionnaireRepository.Object, interviewRepository, registry);

            Identity id = new Identity(staticTextWithSubstitutionId, Empty.RosterVector);
            viewModel.Init(interviewId, id, null);

            fakeInterview = Create.Interview();
        };

        Because of = () => liteEventBus.PublishCommittedEvents(new CommittedEventStream(fakeInterview.EventSourceId, 
            Create.CommittedEvent(payload:new VariablesChanged(changedVariables), eventSourceId: fakeInterview.EventSourceId, eventSequence: 1),
            Create.CommittedEvent(payload: new SubstitutionTitlesChanged(changedTitleIds), eventSourceId: fakeInterview.EventSourceId, eventSequence: 2)));

        It should_change_item_title = () => viewModel.StaticText.ShouldEqual($"Your answer on question is {answer.Answer} and variable is {changedVariables[0].NewValue}");

        static StaticTextViewModel viewModel;
        static ILiteEventBus liteEventBus;
        static IEventSourcedAggregateRoot fakeInterview;
        private static ChangedVariable[] changedVariables;
        private static Identity[] changedTitleIds;
        static TextAnswer answer;
    }
}

