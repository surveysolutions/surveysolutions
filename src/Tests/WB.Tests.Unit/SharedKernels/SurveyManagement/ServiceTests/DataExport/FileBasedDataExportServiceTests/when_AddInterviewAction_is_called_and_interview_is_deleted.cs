using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.FileBasedDataExportServiceTests
{
    internal class when_AddInterviewAction_is_called_and_interview_is_deleted : FileBasedDataExportServiceTestContext
    {
        Establish context = () =>
        {
            interviewSummaryWriter = new Mock<IReadSideRepositoryWriter<InterviewSummary>>();
            interviewSummaryWriter.Setup(x => x.GetById(interviewId.FormatGuid())).Returns(new InterviewSummary() { IsDeleted = true });

            interviewExportServiceMock = new Mock<IDataExportWriter>();

            fileBasedDataExportRepositoryWriter = CreateFileBasedDataExportService(
                dataExportWriter: interviewExportServiceMock.Object, interviewSummaryWriter: interviewSummaryWriter.Object,
                user: new UserDocument());
        };

        Because of = () =>
            fileBasedDataExportRepositoryWriter.AddInterviewAction(InterviewExportedAction.InterviewerAssigned, interviewId, Guid.NewGuid(), DateTime.Now);

        It should_never_call_AddActionRecord = () =>
           interviewExportServiceMock.Verify(x => x.AddActionRecord(Moq.It.IsAny<InterviewActionExportView>(), Moq.It.IsAny<string>()), Times.Never);

        private static FileBasedDataExportRepositoryWriter fileBasedDataExportRepositoryWriter;

        private static Mock<IDataExportWriter> interviewExportServiceMock;
        private static Mock<IReadSideRepositoryWriter<InterviewSummary>> interviewSummaryWriter;
        private static Guid interviewId = Guid.NewGuid();
    }
}
