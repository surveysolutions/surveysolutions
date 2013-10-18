using System;
using System.Collections.Generic;
using System.Linq;
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

        protected static InterviewDataExportFactory CreateInterviewDataExportFactoryForQuestionnarieWithColumns(int interviewCount = 0, params string[] variableNames)
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);

            var interviewSummaryStorageMock = new Mock<IQueryableReadSideRepositoryReader<InterviewSummary>>();

            interviewSummaryStorageMock.Setup(x => x.Query(Moq.It.IsAny<Func<IQueryable<InterviewSummary>, InterviewSummary[]>>())).Returns((InterviewSummary[]) CreateListOfApprovedInterviews(interviewCount));

            var interviewDataStorageMock = new Mock<IReadSideRepositoryReader<InterviewData>>();
            interviewDataStorageMock.Setup(x => x.GetById(Moq.It.IsAny<Guid>())).Returns((Func<InterviewData>) CreateInterviewData);

            var questionnaireStorageMock = new Mock<IVersionedReadSideRepositoryReader<QuestionnaireDocumentVersioned>>();
            questionnaireStorageMock.Setup(x => x.GetById(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>()))
                .Returns(new QuestionnaireDocumentVersioned() { Questionnaire = CreateQuestionnaireDocument(variableNames) });

            var propagationStructureStorageMock = new Mock<IVersionedReadSideRepositoryReader<QuestionnairePropagationStructure>>();
            propagationStructureStorageMock.Setup(x => x.GetById(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>()))
                .Returns(new QuestionnairePropagationStructure());

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

        protected static QuestionnaireDocument CreateQuestionnaireDocument(params string[] variableNames)
        {
            var questionnaire = new QuestionnaireDocument();

            foreach (var variableName in variableNames)
            {
                questionnaire.Children.Add(new NumericQuestion() { StataExportCaption = variableName, PublicKey = Guid.NewGuid() });
            }

            return questionnaire;
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

        protected static InterviewData CreateInterviewData()
        {
            var interview = new InterviewData();
            interview.InterviewId = Guid.NewGuid();
            interview.Levels.Add("#", new InterviewLevel(interview.InterviewId, new int[0]));
            return interview;
        }
    }
}