using System;
using System.Linq;
using Machine.Specifications;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite.Implementation;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionLinkedQuestionViewModelTests
{
    internal class when_handling_question_answered_event_on_sorted_multioption_question : MultiOptionLinkedQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            
            questionId = Create.Entity.Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), Empty.RosterVector);
            interviewId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            Guid linkedToQuestionId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.FixedRoster(fixedTitles: new[] { new FixedRosterTitle(1, "fixed 1"), new FixedRosterTitle(2, "fixed 2") }, children:new []
                {
                    Create.Entity.TextQuestion(linkedToQuestionId)
                }),
                Create.Entity.MultyOptionsQuestion(questionId.Id, linkedToQuestionId: linkedToQuestionId, areAnswersOrdered: true)
                );

            interview = Setup.StatefulInterview(questionnaire);
            interview.AnswerTextQuestion(interviewerId, linkedToQuestionId, Create.Entity.RosterVector(1), DateTime.UtcNow, "option 1");
            interview.AnswerTextQuestion(interviewerId, linkedToQuestionId, Create.Entity.RosterVector(2), DateTime.UtcNow, "option 2");

            var interviews = Setup.StatefulInterviewRepository(interview);
            var questionnaires = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);

            eventRegistry = Create.Service.LiteEventRegistry();

            questionViewModel = CreateViewModel(interviewRepository: interviews, questionnaireStorage: questionnaires, eventRegistry: eventRegistry);
            questionViewModel.Init(interviewId.FormatGuid(), questionId, Create.Other.NavigationState());
        };

        Because of = () =>
        {
            interview.AnswerMultipleOptionsLinkedQuestion(interviewerId, questionId.Id, RosterVector.Empty,
                DateTime.UtcNow, new[] {new decimal[] {2}, new decimal[] {1}});

            Setup.ApplyInterviewEventsToViewModels(interview, eventRegistry, interviewId);
        };

        It should_put_answers_order_on_option1 = () => questionViewModel.Options.First().CheckedOrder.ShouldEqual(2);
        It should_put_answers_order_on_option2 = () => questionViewModel.Options.Second().CheckedOrder.ShouldEqual(1);
        It should_put_checked_on_checked_items = () => questionViewModel.Options.Count(x => x.Checked).ShouldEqual(2);

        static MultiOptionLinkedToRosterQuestionQuestionViewModel questionViewModel;
        static Identity questionId;
        static StatefulInterview interview;
        static LiteEventRegistry eventRegistry;
        static Guid interviewId;
        static Guid interviewerId = Guid.Parse("11111111111111111111111111111111");
    }
}

