using System;
using FluentAssertions;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answering_picture_question_and_answer_is_specified : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDD0000000000");
            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.MultimediaQuestion(questionId: questionId),
            }));

            IQuestionnaireStorage questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            eventContext = new EventContext();
        };

        Because of = () => interview.AnswerPictureQuestion(userId: userId, questionId: questionId, answerTime: answerTime, rosterVector: propagationVector, pictureFileName: pictureFileName);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_PictureQuestionAnswered_event = () =>
            eventContext.ShouldContainEvent<PictureQuestionAnswered>();

        It should_raise_PictureQuestionAnswered_event_with_QuestionId_equal_to_questionId = () =>
            eventContext.GetSingleEvent<PictureQuestionAnswered>().QuestionId.ShouldEqual(questionId);

        It should_raise_PictureQuestionAnswered_event_with_UserId_equal_to_userId = () =>
            eventContext.GetSingleEvent<PictureQuestionAnswered>().UserId.ShouldEqual(userId);

        It should_raise_PictureQuestionAnswered_event_with_PropagationVector_equal_to_propagationVector = () =>
            eventContext.GetSingleEvent<PictureQuestionAnswered>().RosterVector.ShouldEqual(propagationVector);

        It should_raise_PictureQuestionAnswered_event_with_AnswerTime_equal_to_answerTime = () =>
            eventContext.GetSingleEvent<PictureQuestionAnswered>().AnswerTimeUtc.Should().BeCloseTo(DateTime.UtcNow, 2000);

        It should_raise_PictureQuestionAnswered_event_with_PictureFileName_equal_to_pictureFileName = () =>
            eventContext.GetSingleEvent<PictureQuestionAnswered>().PictureFileName.ShouldEqual(pictureFileName);

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static readonly decimal[] propagationVector = RosterVector.Empty;
        private static DateTime answerTime = 2.August(2010).At(22, 00);
        private static string pictureFileName = "my_face.jpg";
    }
}
