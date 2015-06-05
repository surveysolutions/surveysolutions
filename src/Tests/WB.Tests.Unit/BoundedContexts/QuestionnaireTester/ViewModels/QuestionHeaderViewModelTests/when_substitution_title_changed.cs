using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveySolutions.Implementation.Services;
using It = Machine.Specifications.It;
using EventsIdentity = WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.Identity;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.ViewModels.QuestionHeaderViewModelTests
{
    public class when_substitution_title_changed : QuestionHeaderViewModelTestsContext
    {
        Establish context = () =>
        {
            var interviewId = "interviewId";
            var substitutionTargetQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var substitedQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            var answer = new MaskedTextAnswer();
            answer.SetAnswer("new value");
            var interview = Mock.Of<IStatefulInterview>(x => x.FindBaseAnswerByOrDeeperRosterLevel(substitedQuestionId, Empty.RosterVector) == answer);

            var questionnaire = new QuestionnaireModel
            {
                Questions = new Dictionary<Guid, BaseQuestionModel>(),
                QuestionsByVariableNames = new Dictionary<string, BaseQuestionModel>()
            };
            var substTargetModel = new MaskedTextQuestionModel
            {
                Title = "Old title %substitute%",
                Id = substitutionTargetQuestionId
            };
            var substitutedModel = new MaskedTextQuestionModel
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

            EventsIdentity[] changedTitleIds = { new EventsIdentity(substitutionTargetQuestionId, Empty.RosterVector) };
            var eventsToBePublished = new List<UncommittedEvent>
            {
                Create.UncommittedEvent(new SubstitutionTitlesChanged(changedTitleIds))
            };

            fakeInterview = Mock.Of<IAggregateRoot>(x => x.GetUncommittedChanges() == eventsToBePublished);
        };

        Because of = () => liteEventBus.PublishUncommitedEventsFromAggregateRoot(fakeInterview, null);

        It should_change_item_title = () => viewModel.Title.ShouldEqual("Old title new value");

        static QuestionHeaderViewModel viewModel;
        static ILiteEventBus liteEventBus;
        static IAggregateRoot fakeInterview;
    }
}

