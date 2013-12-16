using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.Tests.Views
{
    internal class InterviewDataExportFactoryTestContext {
        protected const string firstLevelkey = "#";

        protected static InterviewDataExportFactory CreateInterviewDataExportFactoryForQuestionnarieCreatedByMethod(
            Func<QuestionnaireDocument> templateCreationAction, 
            Func<InterviewData> interviewCreationAction,
            int interviewCount = 0,
            Func<QuestionnaireRosterStructure> questionnairePropagationStructure = null)
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);

            var interviewSummaryStorageMock = new Mock<IQueryableReadSideRepositoryReader<InterviewSummary>>();

            interviewSummaryStorageMock.Setup(x => x.Query(Moq.It.IsAny<Func<IQueryable<InterviewSummary>, InterviewSummary[]>>())).Returns((InterviewSummary[]) CreateListOfApprovedInterviews(interviewCount));

            var interviewDataStorageMock = new Mock<IReadSideRepositoryReader<InterviewData>>();
            interviewDataStorageMock.Setup(x => x.GetById(Moq.It.IsAny<Guid>())).Returns(interviewCreationAction());

            var questionnaireStorageMock = new Mock<IVersionedReadSideRepositoryReader<QuestionnaireDocumentVersioned>>();
            questionnaireStorageMock.Setup(x => x.GetById(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>()))
                .Returns(new QuestionnaireDocumentVersioned()
                {
                    Questionnaire =  templateCreationAction()
                });

            var propagationStructureStorageMock = new Mock<IVersionedReadSideRepositoryReader<QuestionnaireRosterStructure>>();
            propagationStructureStorageMock.Setup(x => x.GetById(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>()))
                .Returns(questionnairePropagationStructure==null? new QuestionnaireRosterStructure() : questionnairePropagationStructure());

            var linkedQuestionStructureStorageMock = new Mock<IVersionedReadSideRepositoryReader<ReferenceInfoForLinkedQuestions>>();
            linkedQuestionStructureStorageMock.Setup(x => x.GetById(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>()))
                .Returns<Guid, long>(
                    (questionnaireId, version) => new ReferenceInfoForLinkedQuestions(questionnaireId, version, new Dictionary<Guid, ReferenceInfoByQuestion>()));

            return new InterviewDataExportFactory(interviewDataStorageMock.Object,
                interviewSummaryStorageMock.Object,
                questionnaireStorageMock.Object,
                propagationStructureStorageMock.Object,
                linkedQuestionStructureStorageMock.Object);
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocument(Dictionary<string,Guid> variableNameAndQuestionId)
        {
            var questionnaire = new QuestionnaireDocument();

            foreach (var question in variableNameAndQuestionId)
            {
                questionnaire.Children.Add(new NumericQuestion() { StataExportCaption = question.Key, PublicKey = question.Value });
            }

            return questionnaire;
        }

        protected static InterviewData CreateInterviewData()
        {
            var interview = new InterviewData();
            interview.InterviewId = Guid.NewGuid();
            interview.Levels.Add(firstLevelkey, new InterviewLevel(interview.InterviewId, new decimal[0]));
            return interview;
        }

        protected static InterviewData CreateInterviewWithAnswers(IEnumerable<Guid> questionsWithAnswers)
        {
            var interview = CreateInterviewData();
            var firstLevel = interview.Levels[firstLevelkey];

            foreach (var questionsWithAnswer in questionsWithAnswers)
            {
                var question = firstLevel.GetOrCreateQuestion(questionsWithAnswer);
                question.Answer = "some answer";
            }

            return interview;
        }

        protected static InterviewSummary[] CreateListOfApprovedInterviews(int count)
        {
            var interviewsSummary = new InterviewSummary[count];

            for (int i = 0; i < count; i++)
            {
                interviewsSummary[i] = new InterviewSummary();
            }

            return interviewsSummary;
        }
    }

    public static class ShouldExtensions
    {
        public static void ShouldQuestionHasOneNotEmptyAnswer(this Dictionary<Guid, ExportedQuestion> questions, Guid questionId)
        {
            questions.Keys.ShouldContain(qId => qId == questionId);
            var answers = questions[questionId].Answers;
            answers.Length.ShouldEqual(1);
            var answer = answers[0];
            answer.ShouldNotBeEmpty();
        }

        public static void ShouldQuestionHasNoAnswers(this Dictionary<Guid, ExportedQuestion> questions, Guid questionId)
        {
            questions.Keys.ShouldNotContain(qId => qId == questionId);
        }
    }
}