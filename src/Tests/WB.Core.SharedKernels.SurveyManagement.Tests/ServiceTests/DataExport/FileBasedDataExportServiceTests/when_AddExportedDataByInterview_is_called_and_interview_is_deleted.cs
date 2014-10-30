﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.ServiceTests.DataExport.FileBasedDataExportServiceTests
{
    internal class when_AddExportedDataByInterview_is_called_and_interview_is_deleted : FileBasedDataExportServiceTestContext
    {
        Establish context = () =>
        {
            dataExportWriterMock=new Mock<IDataExportWriter>();
            var filebaseExportRouteServiceMock = new Mock<IFilebasedExportedDataAccessor>();
            filebaseExportRouteServiceMock.Setup(x => x.GetFolderPathOfDataByQuestionnaireOrThrow(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>()))
                .Throws<InterviewDataExportException>();
            fileBasedDataExportRepositoryWriter =
                CreateFileBasedDataExportService(filebasedExportedDataAccessor: filebaseExportRouteServiceMock.Object, interviewData: new InterviewData() { IsDeleted = true }, dataExportWriter: dataExportWriterMock.Object);
        };

        Because of = () =>
            fileBasedDataExportRepositoryWriter.AddExportedDataByInterview(Guid.NewGuid());

        It should_AddOrUpdateInterviewRecords_be_never_called = () =>
            dataExportWriterMock.Verify(x => x.AddOrUpdateInterviewRecords(Moq.It.IsAny<InterviewDataExportView>(), Moq.It.IsAny<string>()), Times.Never);

        private static FileBasedDataExportRepositoryWriter fileBasedDataExportRepositoryWriter;
        private static Mock<IDataExportWriter> dataExportWriterMock;
    }
}
