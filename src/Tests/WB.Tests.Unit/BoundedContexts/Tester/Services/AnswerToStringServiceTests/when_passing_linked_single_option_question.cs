using System;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions;
using WB.Core.SharedKernels.Enumerator.Services;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.Services.AnswerToStringServiceTests
{
    internal class when_passing_linked_single_option_question : AnswerToStringServiceTestsContext
    {
        Establish context = () =>
        {
            answerToStringService = CreateAnswerToStringService();
            var sourceOfLinking =
                CreateSingleOptionQuestionModel(
                    new[]
                    {
                        new OptionModel() {Title = "1", Value = 1},
                        new OptionModel() {Title = "2", Value = 2},
                        new OptionModel() {Title = "answer", Value = 3},
                        new OptionModel() {Title = "4", Value = 4},
                    });
            sourceOfLinking.Id = sourceOfLinkId;
            var answerOnSourceOfLinking = CreateSingleOptionAnswer(3);
            answerOnSourceOfLinking.RosterVector = new decimal[] {3};

            statefulInterviewMock=new Mock<IStatefulInterview>();
            statefulInterviewMock.Setup(
                x => x.FindAnswersOfReferencedQuestionForLinkedQuestion(sourceOfLinkId, Moq.It.IsAny<Identity>()))
                .Returns(new[] { answerOnSourceOfLinking });

            questionnaireModel = Create.QuestionnaireModel(new[] {sourceOfLinking});
            singleOptionAnswer = CreateLinkedSingleOptionAnswer(new decimal[] { 3 });
            singleOptionQuestionModel = CreateLinkedSingleOptionQuestionModel(sourceOfLinkId);
        };

        Because of = () =>
            result = answerToStringService.AnswerToUIString(singleOptionQuestionModel, singleOptionAnswer, statefulInterviewMock.Object, questionnaireModel);

        It should_return_3 = () =>
            result.ShouldEqual("answer");

        static string result;
        static LinkedSingleOptionAnswer singleOptionAnswer;
        static LinkedSingleOptionQuestionModel singleOptionQuestionModel;
        static IAnswerToStringService answerToStringService;
        static Guid sourceOfLinkId=Guid.NewGuid();
        static QuestionnaireModel questionnaireModel;
        static Mock<IStatefulInterview> statefulInterviewMock;
    }
}