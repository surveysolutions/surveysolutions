using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;
using WB.Core.SharedKernels.DataCollection;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.ViewModels.MultiOptionLinkedQuestionViewModelTests
{
    public class when_title_of_linked_question_changes : MultiOptionLinkedQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            questionId = new Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), new decimal[] { 1 });
            linkedToQuestionId = new Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), new decimal[] { 1 });

            var questionnaire = Create.QuestionnaireModel(new BaseQuestionModel[] {
                    new TextQuestionModel
                    {
                        Id = linkedToQuestionId.Id
                    },
                    new LinkedMultiOptionQuestionModel
                    {
                        Id = questionId.Id,
                        LinkedToQuestionId = linkedToQuestionId.Id
                    }
                });

            interview = new Mock<IStatefulInterview>();
            interview.SetupGet(x => x.Answers).Returns(new Dictionary<string, BaseInterviewAnswer>());
            interview.Setup(x => x.FindAnswersOfReferencedQuestionForLinkedQuestion(linkedToQuestionId.Id, Moq.It.IsAny<Identity>()))
                .Returns(new BaseInterviewAnswer[] { Create.TextAnswer("answer", null, new decimal[] { 1 }) });

            answerNotifier = Create.AnswerNotifier();

            viewModel = CreateViewModel(questionnaire, interview.Object, answerNotifier);
            viewModel.Init("interview", questionId, Create.NavigationState());
        };

        Because of = () =>
        {
            interview.Setup(x => x.FindAnswersOfReferencedQuestionForLinkedQuestion(linkedToQuestionId.Id, Moq.It.IsAny<Identity>()))
                .Returns(new BaseInterviewAnswer[] { Create.TextAnswer("changed", null, new decimal[] { 1 }) });
            answerNotifier.Handle(Create.Event.TextQuestionAnswered(linkedToQuestionId.Id, linkedToQuestionId.RosterVector, "changed"));
        };

        It should_insert_new_option = () => viewModel.Options.First().Title.ShouldEqual("changed");

        static MultiOptionLinkedQuestionViewModel viewModel;
        static Identity questionId;
        static AnswerNotifier answerNotifier;
        static Identity linkedToQuestionId;
        static Mock<IStatefulInterview> interview; 
    }
}