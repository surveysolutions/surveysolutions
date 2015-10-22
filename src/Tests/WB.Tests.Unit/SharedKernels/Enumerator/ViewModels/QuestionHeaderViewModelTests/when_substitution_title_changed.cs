using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions;
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

            var questionnaire = new QuestionnaireModel
            {
                Questions = new Dictionary<Guid, BaseQuestionModel>(),
                QuestionsByVariableNames = new Dictionary<string, BaseQuestionModel>()
            };
            var substTargetModel = new TextQuestionModel
            {
                Title = "Old title %substitute%",
                Id = substitutionTargetQuestionId
            };
            var substitutedModel = new TextQuestionModel
            {
                Id = substitedQuestionId,
                Variable = "substitute"
            };
            questionnaire.Questions[substitutionTargetQuestionId] = substTargetModel;
            questionnaire.Questions[substitedQuestionId] = substitutedModel;
            questionnaire.QuestionsByVariableNames["blah"] = substTargetModel;
            questionnaire.QuestionsByVariableNames[substitutedModel.Variable] = substitutedModel;

            var interviewRepository = Mock.Of<IStatefulInterviewRepository>(x => x.Get(interviewId) == interview);
            var questionnaireRepository = Mock.Of<IPlainKeyValueStorage<QuestionnaireModel>>(x => x.GetById(Moq.It.IsAny<string>()) == questionnaire);

            ILiteEventRegistry registry = Create.LiteEventRegistry();
            liteEventBus = Create.LiteEventBus(registry);

            viewModel = CreateViewModel(questionnaireRepository,
                interviewRepository,
                registry);

            Identity id = new Identity(substitutionTargetQuestionId, Empty.RosterVector);
            viewModel.Init(interviewId, id);

            changedTitleIds =
                new Identity[]
                {
                    new Identity(substitutionTargetQuestionId, Empty.RosterVector)
                };
            fakeInterview = Create.Interview();
        };

        Because of = () => liteEventBus.PublishCommitedEvents(fakeInterview, new CommittedEventStream(fakeInterview.EventSourceId, 
            Create.CommittedEvent(payload:new SubstitutionTitlesChanged(changedTitleIds), eventSourceId: fakeInterview.EventSourceId)));

        It should_change_item_title = () => viewModel.Title.ShouldEqual("Old title new value");

        static QuestionHeaderViewModel viewModel;
        static ILiteEventBus liteEventBus;
        static IAggregateRoot fakeInterview;
        private static Identity[] changedTitleIds;
    }
}

