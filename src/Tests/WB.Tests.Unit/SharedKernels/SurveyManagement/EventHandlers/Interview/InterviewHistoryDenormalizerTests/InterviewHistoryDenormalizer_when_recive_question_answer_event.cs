using System;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewHistoryDenormalizerTests
{
    [TestFixture]
    internal class InterviewHistoryDenormalizer_when_recive_question_answer_event : InterviewHistoryDenormalizerTestContext
    {
        [Test]
        public void when_recive_DateTimeAnswer_for_currenttime_question()
        {
            var questionId = Guid.NewGuid();
            var questionnaireId = Guid.NewGuid();
            var variableName = "dt1";
            var answer = new DateTime(2017, 10, 19, 15, 46, 37);

            var interviewHistoryView = CreateInterviewHistoryView();
            var questionnaireDocument = Create.Entity.QuestionnaireDocument(questionnaireId, children: new[]
            {
                Create.Entity.DateTimeQuestion(questionId, isTimestamp: true)
            });
            var questionnaireStorage = Stub<IQuestionnaireStorage>.Returning(questionnaireDocument);
            var interviewExportedDataDenormalizer = CreateInterviewHistoryDenormalizer(
                questionnaire: CreateQuestionnaireExportStructure(questionId, variableName),
                questionnaireStorage: questionnaireStorage);
            var dateTimeQuestionAnswered = Create.PublishedEvent.DateTimeQuestionAnswered(questionId: questionId, answer: answer);


            interviewExportedDataDenormalizer.Update(interviewHistoryView, dateTimeQuestionAnswered);

            var recordView = interviewHistoryView.Records[0];
            Assert.AreEqual(recordView.Action, InterviewHistoricalAction.AnswerSet);
            Assert.AreEqual(recordView.Parameters["answer"], answer.ToString(DateTimeFormat.DateWithTimeFormat));
        }

        [Test]
        public void when_recive_DateTimeAnswer_for_datetime_question()
        {
            var questionId = Guid.NewGuid();
            var variableName = "dt1";
            var answer = new DateTime(2017, 10, 19, 15, 46, 37);

            var interviewHistoryView = CreateInterviewHistoryView();
            var questionnaireDocument = Create.Entity.QuestionnaireDocument(children: new[]
            {
                Create.Entity.DateTimeQuestion(questionId, isTimestamp: false)
            });
            var questionnaireStorage = Stub<IQuestionnaireStorage>.Returning(questionnaireDocument);
            var interviewExportedDataDenormalizer = CreateInterviewHistoryDenormalizer(
                questionnaire: CreateQuestionnaireExportStructure(questionId, variableName),
                questionnaireStorage: questionnaireStorage);
            var dateTimeQuestionAnswered = Create.PublishedEvent.DateTimeQuestionAnswered(questionId: questionId, answer: answer);


            interviewExportedDataDenormalizer.Update(interviewHistoryView, dateTimeQuestionAnswered);

            var recordView = interviewHistoryView.Records[0];
            Assert.AreEqual(recordView.Action, InterviewHistoricalAction.AnswerSet);
            Assert.AreEqual(recordView.Parameters["answer"], answer.ToString(DateTimeFormat.DateFormat));
        }
    }
}