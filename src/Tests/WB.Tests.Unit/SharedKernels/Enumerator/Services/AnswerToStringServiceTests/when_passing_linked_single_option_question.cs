using System;
using Machine.Specifications;
using Moq;
using NSubstitute;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Services;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Services.AnswerToStringServiceTests
{
    internal class when_passing_linked_single_option_question : AnswerToStringServiceTestsContext
    {
        Establish context = () =>
        {
            answerToStringService = CreateAnswerToStringService();
          
            var answerOnSourceOfLinking = CreateSingleOptionAnswer(sourceOfLinkId, 3);
            answerOnSourceOfLinking.RosterVector = Create.RosterVector(3);

            statefulInterviewMock = Substitute.For<IStatefulInterview>();
            statefulInterviewMock.FindAnswersOfReferencedQuestionForLinkedQuestion(sourceOfLinkId, Arg.Any<Identity>())
                .Returns(new[] { answerOnSourceOfLinking });

            singleOptionAnswer = CreateLinkedSingleOptionAnswer(Create.RosterVector(3), linkedId);

            questionnaire = Mock.Of<IQuestionnaire>(_ 
                => _.IsQuestionLinked(linkedId) == true
                && _.GetAnswerOptionTitle(sourceOfLinkId, 3) == "answer"
                && _.GetQuestionReferencedByLinkedQuestion(linkedId) == sourceOfLinkId);
        };

        Because of = () => 
            result = answerToStringService.AnswerToUIString(linkedId, singleOptionAnswer, statefulInterviewMock, questionnaire);

        It should_return_linked_answer_text = () => 
            result.ShouldEqual("answer");

        static string result;
        static IQuestionnaire questionnaire;
        static LinkedSingleOptionAnswer singleOptionAnswer;
        static IAnswerToStringService answerToStringService;
        static Guid sourceOfLinkId = Id.g1;
        static Guid linkedId = Id.g2;
        static IStatefulInterview statefulInterviewMock;
    }
}