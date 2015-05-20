using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.FileBasedDataExportServiceTests
{
    internal class when_not_empty_cache_is_disabled : FileBasedDataExportServiceTestContext
    {
        Establish context = () =>
        {
            dataExportWriterMock=new Mock<IDataExportWriter>();

            var filebaseExportDataAccessorMock = new Mock<IFilebasedExportedDataAccessor>();
            filebaseExportDataAccessorMock.Setup(x => x.GetFolderPathOfDataByQuestionnaire(questionnaireId, questionnaireVersion))
                .Returns(existingQuestionnairePath);
            filebaseExportDataAccessorMock.Setup(x => x.GetFolderPathOfDataByQuestionnaire(questionnaireForDeleteId, questionnaireForDeleteVersion))
                .Returns(deletingQuestionnairePath);
            var interviewSummaryStorageMock = new Mock<IReadSideRepositoryWriter<InterviewSummary>>();
            interviewSummaryStorageMock.Setup(x => x.GetById(Moq.It.Is<string>(i => i == interviewId.FormatGuid() || i == interviewForDeleteId.FormatGuid())))
                .Returns(new InterviewSummary() { QuestionnaireId = questionnaireId, QuestionnaireVersion = questionnaireVersion });

            interviewSummaryStorageMock.Setup(x => x.GetById(Moq.It.Is<string>(i => i == interviewByDeletedTemplateId.FormatGuid())))
                .Returns(new InterviewSummary() { QuestionnaireId = questionnaireForDeleteId, QuestionnaireVersion = questionnaireForDeleteVersion });

            fileBasedDataExportRepositoryWriter =
                CreateFileBasedDataExportService(interviewSummaryWriter: interviewSummaryStorageMock.Object,
                    filebasedExportedDataAccessor: filebaseExportDataAccessorMock.Object, dataExportWriter: dataExportWriterMock.Object, user: new UserDocument());
            fileBasedDataExportRepositoryWriter.EnableCache();
            fileBasedDataExportRepositoryWriter.AddExportedDataByInterview(interviewId);
            fileBasedDataExportRepositoryWriter.AddExportedDataByInterview(interviewByDeletedTemplateId);
            fileBasedDataExportRepositoryWriter.AddInterviewAction(InterviewExportedAction.SupervisorAssigned, interviewId, Guid.NewGuid(),DateTime.Now);
            fileBasedDataExportRepositoryWriter.DeleteInterview(interviewForDeleteId);
            fileBasedDataExportRepositoryWriter.DeleteExportedDataForQuestionnaireVersion(questionnaireForDeleteId,
                questionnaireForDeleteVersion);
        };

        Because of = () =>
            fileBasedDataExportRepositoryWriter.DisableCache();

        It should_call_once_batch_insert_for_existing_questionnaire = () =>
            dataExportWriterMock.Verify(x => x.BatchInsert(existingQuestionnairePath, Moq.It.Is<IEnumerable<InterviewDataExportView>>(_ => _.Count() == 1),
                Moq.It.Is<IEnumerable<InterviewActionExportView>>(_ => _.Count() == 1 && _.First().Action == InterviewExportedAction.SupervisorAssigned), Moq.It.Is<IEnumerable<Guid>>(_ => _.Count() == 1 && _.First() == interviewForDeleteId)), Times.Once);

        It should_never_call_batch_insert_for_deleted_questionnaire = () =>
           dataExportWriterMock.Verify(x => x.BatchInsert(deletingQuestionnairePath, Moq.It.IsAny<IEnumerable<InterviewDataExportView>>(),
               Moq.It.IsAny<IEnumerable<InterviewActionExportView>>(), Moq.It.IsAny<IEnumerable<Guid>>()), Times.Never);

        private static FileBasedDataExportRepositoryWriter fileBasedDataExportRepositoryWriter;
        private static Guid interviewId = Guid.NewGuid();
        private static Guid interviewForDeleteId = Guid.NewGuid();
        private static Guid interviewByDeletedTemplateId = Guid.NewGuid();

        private static Guid questionnaireForDeleteId = Guid.NewGuid();
        private static long questionnaireForDeleteVersion = 2;

        private static Guid questionnaireId = Guid.NewGuid();
        private static long questionnaireVersion = 1;

        private static string existingQuestionnairePath="existing questionnaire";
        private static string deletingQuestionnairePath = "deleting questionnaire";

        private static Mock<IDataExportWriter> dataExportWriterMock;
    }
}
