using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionLinkedQuestionViewModelTests
{
    internal class when_handling_question_answered_event_on_sorted_multioption_question : MultiOptionLinkedQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            interviewId = "interview";
            questionId = Create.Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), Empty.RosterVector);
            Guid linkedToQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            var interview = Mock.Of<IStatefulInterview>(x =>
                x.FindAnswersOfReferencedQuestionForLinkedQuestion(Moq.It.IsAny<Guid>(), Moq.It.IsAny<Identity>()) == new[]
                {
                    Create.TextAnswer("answer1", linkedToQuestionId, new []{1m}),
                    Create.TextAnswer("answer2", linkedToQuestionId, new []{2m})
                } &&
                x.Answers == new Dictionary<string, BaseInterviewAnswer>()
                );

            var questionnaire = Create.QuestionnaireModel();
            questionnaire.Questions = new Dictionary<Guid, BaseQuestionModel>();
            questionnaire.Questions.Add(questionId.Id, new LinkedMultiOptionQuestionModel
            {
                LinkedToQuestionId = linkedToQuestionId,
                AreAnswersOrdered = true
            });
            questionnaire.Questions.Add(linkedToQuestionId, new TextQuestionModel());


            var interviews = new Mock<IStatefulInterviewRepository>();
            var questionnaires = new Mock<IPlainKeyValueStorage<QuestionnaireModel>>();

            interviews.SetReturnsDefault(interview);
            questionnaires.SetReturnsDefault(questionnaire);

            questionViewModel = CreateViewModel(interviewRepository: interviews.Object, questionnaireStorage: questionnaires.Object);
            questionViewModel.Init(interviewId, questionId, Create.NavigationState());
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

