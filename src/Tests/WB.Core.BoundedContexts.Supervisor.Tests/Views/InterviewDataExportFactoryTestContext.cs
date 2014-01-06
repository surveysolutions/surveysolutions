using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.Tests.Views
{
    [Subject(typeof(InterviewDataExportFactory))]
    internal class InterviewDataExportFactoryTestContext {
        protected const string firstLevelkey = "#";

        protected static InterviewDataExportFactory CreateInterviewDataExportFactoryForQuestionnarieCreatedByMethod(
            Func<QuestionnaireDocument> templateCreationAction,
            Func<InterviewExportedData> dataCreationAction=null,
            int interviewCount = 0)
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);

            var interviewExportedDataStorageMock = new Mock<IQueryableReadSideRepositoryReader<InterviewExportedData>>();
            var questionnaire = templateCreationAction();


            interviewExportedDataStorageMock.Setup(
                x => x.Query(Moq.It.IsAny<Func<IQueryable<InterviewExportedData>, InterviewExportedData[]>>()))
                .Returns(CreateListOfApprovedInterviews(interviewCount,
                    () =>
                    {
                        var createInterview = dataCreationAction ?? CreateInterviewData;
                        var interview = createInterview();
                        interview.QuestionnaireId = questionnaire.PublicKey;
                        return interview;
                    }));


            var questionnaireExportStructureMock = new Mock<IVersionedReadSideRepositoryReader<QuestionnaireExportStructure>>();
            questionnaireExportStructureMock.Setup(x => x.GetById(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>()))
                .Returns(new QuestionnaireExportStructure(questionnaire, 1));

            return new InterviewDataExportFactory(
                interviewExportedDataStorageMock.Object,
                questionnaireExportStructureMock.Object);
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocument(Dictionary<string,Guid> variableNameAndQuestionId)
        {
            var questionnaire = new QuestionnaireDocument();

            foreach (var question in variableNameAndQuestionId)
            {
                questionnaire.Children.Add(new NumericQuestion() { StataExportCaption = question.Key, PublicKey = question.Value, QuestionType = QuestionType.Numeric});
            }

            return questionnaire;
        }

        protected static InterviewExportedData CreateInterviewData()
        {
            var interviewsSummary = new InterviewExportedData();
            interviewsSummary.InterviewDataByLevels = new []
            { new InterviewExportedLevel(Guid.NewGuid(), new decimal[0], new ExportedQuestion[0]) };
            return interviewsSummary;
        }

        protected static InterviewExportedData CreateInterviewWithAnswers(IEnumerable<Guid> questionsWithAnswers)
        {
            var interviewsSummary = new InterviewExportedData();
            interviewsSummary.InterviewDataByLevels = new[]
            {
                new InterviewExportedLevel(Guid.NewGuid(), new decimal[0],
                    questionsWithAnswers.Select(qId => CreateExportedQuestion(qId, "some answer")).ToArray())
            };
            return interviewsSummary;
        }

        protected static ExportedQuestion CreateExportedQuestion(Guid questionId, string answer)
        {
            var interviewQuestion = new InterviewQuestion(questionId);
            interviewQuestion.Answer = answer;
            return new ExportedQuestion(interviewQuestion, new ExportedHeaderItem(new TextQuestion()));
        }

        protected static InterviewExportedData[] CreateListOfApprovedInterviews(int count, Func<InterviewExportedData> dataCreationAction)
        {
            var interviewsSummary = new InterviewExportedData[count];

            for (int i = 0; i < count; i++)
            {
                interviewsSummary[i] = dataCreationAction();
            }

            return interviewsSummary;
        }
    }

    public static class ShouldExtensions
    {
        public static void ShouldQuestionHasOneNotEmptyAnswer(this ExportedQuestion[] questions, Guid questionId)
        {
            questions.ShouldContain(q => q.QuestionId == questionId);
            var answers = questions.First(q => q.QuestionId == questionId).Answers;
            answers.Length.ShouldEqual(1);
            var answer = answers[0];
            answer.ShouldNotBeEmpty();
        }

        public static void ShouldQuestionHasNoAnswers(this ExportedQuestion[] questions, Guid questionId)
        {
            questions.ShouldNotContain(q => q.QuestionId == questionId);
        }
    }
}