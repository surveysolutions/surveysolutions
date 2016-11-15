using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using Nito.AsyncEx.Synchronous;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;

using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionLinkedQuestionViewModelTests
{
    [Ignore("KP-8159")]
    internal class when_handling_question_answered_event_on_sorted_multioption_question : MultiOptionLinkedQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            interviewId = "interview";
            questionId = Create.Entity.Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), Empty.RosterVector);
            Guid linkedToQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            var interview = Mock.Of<IStatefulInterview>();
            //x =>
            //    x.FindAnswersOfReferencedQuestionForLinkedQuestion(Moq.It.IsAny<Guid>(), Moq.It.IsAny<Identity>()) == new[]
            //    {
            //        Create.Entity.InterviewTreeTextQuestion("answer1", linkedToQuestionId, new []{1m}),
            //        Create.Entity.InterviewTreeTextQuestion("answer2", linkedToQuestionId, new []{2m})
            //    } &&
            //    x.Answers == new Dictionary<string, BaseInterviewAnswer>()
            //    );

            var questionnaire = Mock.Of<IQuestionnaire>(_ 
                => _.GetQuestionReferencedByLinkedQuestion(questionId.Id) == linkedToQuestionId
                && _.ShouldQuestionRecordAnswersOrder(questionId.Id) == true);

            var interviews = new Mock<IStatefulInterviewRepository>();
            var questionnaires = new Mock<IQuestionnaireStorage>();

            interviews.SetReturnsDefault(interview);
            questionnaires.SetReturnsDefault(questionnaire);

            questionViewModel = CreateViewModel(interviewRepository: interviews.Object, questionnaireStorage: questionnaires.Object);
            questionViewModel.Init(interviewId, questionId, Create.Other.NavigationState());
        };

        Because of = () => questionViewModel.Handle(Create.Event.MultipleOptionsLinkedQuestionAnswered(questionId:questionId.Id,
            rosterVector: questionId.RosterVector,
            selectedRosterVectors: new[] { new decimal[] { 2 }, new decimal[] { 1 } }));

        It should_put_answers_order_on_option1 = () => questionViewModel.Options.First().CheckedOrder.ShouldEqual(2);
        It should_put_answers_order_on_option2 = () => questionViewModel.Options.Second().CheckedOrder.ShouldEqual(1);
        It should_put_checked_on_checked_items = () => questionViewModel.Options.Count(x => x.Checked).ShouldEqual(2);

        static MultiOptionLinkedToQuestionQuestionViewModel questionViewModel;
        static string interviewId;
        static Identity questionId;
    }
}

