using System;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Moq;
using Ncqrs.Eventing;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;

using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.StaticTextViewModelTests
{
    internal class when_substitution_title_changed : StaticTextViewModelTestsContext
    {
        Establish context = () =>
        {
            var interviewId = "interviewId";
            var staticTextWithSubstitutionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var substitedQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            var answer = new TextAnswer();
            answer.SetAnswer("new value");
            var interview = Mock.Of<IStatefulInterview>(x => x.FindBaseAnswerByOrDeeperRosterLevel(substitedQuestionId, Empty.RosterVector) == answer);

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewId) == interview);

            var questionnaireMock = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Entity.StaticText(publicKey: staticTextWithSubstitutionId, text: "Old title %substitute%"),
                Create.Entity.NumericRealQuestion(variable: "substitute", id: substitedQuestionId)
            }));

            var questionnaireRepository = new Mock<IQuestionnaireStorage>();
            questionnaireRepository.Setup(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<string>())).Returns(questionnaireMock);

            ILiteEventRegistry registry = Create.Service.LiteEventRegistry();
            liteEventBus = Create.Service.LiteEventBus(registry);

            viewModel = CreateViewModel(questionnaireRepository.Object, interviewRepository, registry);

            Identity id = new Identity(staticTextWithSubstitutionId, Empty.RosterVector);
            viewModel.Init(interviewId, id, null);

            changedTitleIds =
                new Identity[]
                {
                    new Identity(staticTextWithSubstitutionId, Empty.RosterVector)
                };
            fakeInterview = Create.AggregateRoot.Interview();
        };

        Because of = () => liteEventBus.PublishCommittedEvents(new CommittedEventStream(fakeInterview.EventSourceId, 
            Create.Other.CommittedEvent(payload: Create.Event.SubstitutionTitlesChanged(staticTexts: changedTitleIds), eventSourceId: fakeInterview.EventSourceId)));

        It should_change_item_title = () => viewModel.Text.PlainText.ShouldEqual("Old title new value");

        static StaticTextViewModel viewModel;
        static ILiteEventBus liteEventBus;
        static IEventSourcedAggregateRoot fakeInterview;
        private static Identity[] changedTitleIds;
    }
}

