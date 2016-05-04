using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.QuestionHeaderViewModelTests
{
    internal class when_substitution_title_changed : QuestionHeaderViewModelTestsContext
    {
        Establish context = () =>
        {
            var interviewId = "interviewId";
            var substitutionTargetQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var substitedQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            var answer = new TextAnswer();
            answer.SetAnswer("new value");
            var interview = Mock.Of<IStatefulInterview>(x => x.FindBaseAnswerByOrDeeperRosterLevel(substitedQuestionId, Empty.RosterVector) == answer);

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewId) == interview);

            var questionnaireMock = Mock.Of<IQuestionnaire>(_
            => _.GetQuestionTitle(substitutionTargetQuestionId) == "Old title %substitute%"
            && _.GetQuestionInstruction(substitutionTargetQuestionId) == "Instruction"
            && _.GetQuestionIdByVariable("substitute") == substitedQuestionId
            );

            var questionnaireRepository = new Mock<IPlainQuestionnaireRepository>();
            questionnaireRepository.SetReturnsDefault(questionnaireMock);
           
            ILiteEventRegistry registry = Create.LiteEventRegistry();
            liteEventBus = Create.LiteEventBus(registry);

            viewModel = CreateViewModel(questionnaireRepository.Object, interviewRepository, registry);

            Identity id = new Identity(substitutionTargetQuestionId, Empty.RosterVector);
            viewModel.Init(interviewId, id);

            changedTitleIds =
                new Identity[]
                {
                    new Identity(substitutionTargetQuestionId, Empty.RosterVector)
                };
            fakeInterview = Create.Interview();
        };

        Because of = () => liteEventBus.PublishCommittedEvents(new CommittedEventStream(fakeInterview.EventSourceId, 
            Create.CommittedEvent(payload:new SubstitutionTitlesChanged(changedTitleIds), eventSourceId: fakeInterview.EventSourceId)));

        It should_change_item_title = () => viewModel.Title.ShouldEqual("Old title new value");

        static QuestionHeaderViewModel viewModel;
        static ILiteEventBus liteEventBus;
        static IAggregateRoot fakeInterview;
        private static Identity[] changedTitleIds;
    }
}

