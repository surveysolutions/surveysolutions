﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    internal class when_answering_picture_question_and_answer_is_specified : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDD0000000000");
            var questionnaire = Mock.Of<IQuestionnaire>
                (_
                    => _.HasQuestion(questionId) == true &&
                        _.GetQuestionType(questionId) == QuestionType.Multimedia
                );

            SetupInstanceToMockedServiceLocator<IQuestionnaireRepository>(
                CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire));

            interview = CreateInterview(questionnaireId: questionnaireId);

            eventContext = new EventContext();
        };

        Because of = () =>
            interview.AnswerPictureQuestion(userId: userId, questionId: questionId, answerTime: answerTime,
                                              rosterVector: propagationVector, pictureFileName: pictureFileName);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_PictureQuestionAnswered_event = () =>
            eventContext.ShouldContainEvent<PictureQuestionAnswered>();

        [Ignore("Interview state shoul return validity status")]
        It should_raise_ValidityChanges_event = () =>
            eventContext.ShouldContainEvent<AnswersDeclaredValid>();

        It should_raise_PictureQuestionAnswered_event_with_QuestionId_equal_to_questionId = () =>
            eventContext.GetSingleEvent<PictureQuestionAnswered>().QuestionId.ShouldEqual(questionId);

        It should_raise_PictureQuestionAnswered_event_with_UserId_equal_to_userId = () =>
            eventContext.GetSingleEvent<PictureQuestionAnswered>().UserId.ShouldEqual(userId);

        It should_raise_PictureQuestionAnswered_event_with_PropagationVector_equal_to_propagationVector = () =>
            eventContext.GetSingleEvent<PictureQuestionAnswered>().PropagationVector.ShouldEqual(propagationVector);

        It should_raise_PictureQuestionAnswered_event_with_AnswerTime_equal_to_answerTime = () =>
            eventContext.GetSingleEvent<PictureQuestionAnswered>().AnswerTime.ShouldEqual(answerTime);

        It should_raise_PictureQuestionAnswered_event_with_PictureFileName_equal_to_pictureFileName = () =>
            eventContext.GetSingleEvent<PictureQuestionAnswered>().PictureFileName.ShouldEqual(pictureFileName);

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static decimal[] propagationVector = new decimal[0];
        private static DateTime answerTime = DateTime.Now;
        private static string pictureFileName = "my_face.jpg";
    }
}
