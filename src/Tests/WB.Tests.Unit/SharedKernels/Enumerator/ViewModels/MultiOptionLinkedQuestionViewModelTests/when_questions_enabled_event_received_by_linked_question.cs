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
    internal class when_questions_enabled_event_received_by_linked_question : MultiOptionLinkedQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            var interview = new Mock<IStatefulInterview>();

            interview.Setup(x => x.FindAnswersOfReferencedQuestionForLinkedQuestion(linkedToQuestionId, Moq.It.IsAny<Identity>()))
                .Returns(new[]
                {
                    Create.TextAnswer("answer2", linkedToQuestionId, Create.RosterVector(2))
                });

            interview.Setup(x =>x.Answers)
                .Returns(new Dictionary<string, BaseInterviewAnswer>());
              

            var questionnaire = Create.QuestionnaireModel();
            var linkedMultiOptionQuestionModel = new LinkedMultiOptionQuestionModel
            {
                LinkedToQuestionId = linkedToQuestionId
            };
            questionnaire.Questions = new Dictionary<Guid, BaseQuestionModel>
            {
                {questionId.Id, linkedMultiOptionQuestionModel},
                {linkedToQuestionId, new TextQuestionModel()}
            };

            var interviews = new Mock<IStatefulInterviewRepository>();
            var questionnaires = new Mock<IPlainKeyValueStorage<QuestionnaireModel>>();

            interviews.SetReturnsDefault(interview.Object);
            questionnaires.SetReturnsDefault(questionnaire);

            questionViewModel = CreateViewModel(interviewRepository: interviews.Object, questionnaireStorage: questionnaires.Object);
            questionViewModel.Init("interviewId", questionId, Create.NavigationState());

            interview.Setup(x => x.FindAnswersOfReferencedQuestionForLinkedQuestion(linkedToQuestionId, Moq.It.IsAny<Identity>()))
                .Returns(new[]
                {
                    Create.TextAnswer("answer1", linkedToQuestionId, Create.RosterVector(1)),
                    Create.TextAnswer("answer2", linkedToQuestionId, Create.RosterVector(2))
                });
        };

        Because of = () =>
        {
            questionViewModel.Handle(Create.Event.QuestionsEnabled(linkedToQuestionId, Create.RosterVector(1)));
        };

        It should_decrease_amount_of_options = () =>
            questionViewModel.Options.Count.ShouldEqual(2);

        It should_have_option_1_with_roster_code__1 = () =>
            questionViewModel.Options.First().Value.ShouldContainOnly(Create.RosterVector(1));

        It should_have_option_2_with_roster_code__2 = () =>
            questionViewModel.Options.Second().Value.ShouldContainOnly(Create.RosterVector(2));

        private static MultiOptionLinkedToQuestionQuestionViewModel questionViewModel;
        private static readonly Guid linkedToQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static readonly Identity questionId = Create.Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), Empty.RosterVector);
    }
}