using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionLinkedQuestionViewModelTests
{
    internal class when_linked_to_question_is_answered : MultiOptionLinkedQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            questionId = new Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), new decimal[] { 1 });
            linkedToQuestionId = new Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), new decimal[] { 1 });

            //var questionnaire = Create.QuestionnaireModel(new BaseQuestionModel[] {
            //        new TextQuestionModel
            //        {
            //            Id = linkedToQuestionId.Id
            //        },
            //        new LinkedMultiOptionQuestionModel
            //        {
            //            Id = questionId.Id,
            //            LinkedToQuestionId = linkedToQuestionId.Id
            //        }
            //    });
            var questionnaire = Mock.Of<IQuestionnaire>();
            interview = new Mock<IStatefulInterview>();
            interview.SetupGet(x => x.Answers).Returns(new Dictionary<string, BaseInterviewAnswer>());
            interview.Setup(x => x.FindAnswersOfReferencedQuestionForLinkedQuestion(linkedToQuestionId.Id, Moq.It.IsAny<Identity>()))
                .Returns(new BaseInterviewAnswer[] {});

            answerNotifier = Create.AnswerNotifier();

            questionViewModel = CreateViewModel(questionnaire, interview.Object, answerNotifier);
            questionViewModel.Init("interview", questionId, Create.NavigationState());
        };

        Because of = () =>
        {
            interview.Setup(x => x.FindAnswersOfReferencedQuestionForLinkedQuestion(linkedToQuestionId.Id, Moq.It.IsAny<Identity>()))
                .Returns(new BaseInterviewAnswer[] { Create.TextAnswer("answer", null, new decimal[] { 1 }) });
            answerNotifier.Handle(Create.Event.TextQuestionAnswered(linkedToQuestionId.Id, linkedToQuestionId.RosterVector, "answer"));
        };

        It should_insert_new_option = () => questionViewModel.Options.Count.ShouldEqual(1);

        static MultiOptionLinkedToQuestionQuestionViewModel questionViewModel;
        static Identity questionId;
        static AnswerNotifier answerNotifier;
        static Identity linkedToQuestionId;
        static Mock<IStatefulInterview> interview;
    }
}

