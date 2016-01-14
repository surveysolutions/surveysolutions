using System;
using System.Collections.Generic;
using System.IO;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Accessors;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;
using It = Moq.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ViewsTests.TabularParaDataWriterTests
{
    [Subject(typeof(TabularParaDataAccessor))]
    internal class TabularParaDataWriterTestContext
    {
        protected static TabularParaDataAccessor CreateTabularParaDataWriter(ICsvWriterService csvWriterService = null,
            IFileSystemAccessor fileSystemAccessor = null, IReadSideRepositoryWriter<InterviewSummary> interviewSummaryWriter = null, IFilebasedExportedDataAccessor filebasedExportedDataAccessor=null)
        {
            return new TabularParaDataAccessor(Mock.Of<ICsvWriter>(_=>_.OpenCsvWriter(It.IsAny<Stream>(),It.IsAny<string>())== Mock.Of<ICsvWriterService>()),
                fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(),
                interviewSummaryWriter ?? Mock.Of<IReadSideRepositoryWriter<InterviewSummary>>(), new InterviewDataExportSettings("", true,1,1,1), Mock.Of<IArchiveUtils>());
        }

        protected static InterviewHistoryView CreateInterviewHistoryView(Guid? id=null)
        {
            return new InterviewHistoryView(id??Guid.NewGuid(), new List<InterviewHistoricalRecordView>(), Guid.NewGuid(), 1);
        }

        protected static InterviewHistoricalRecordView CreateInterviewHistoricalRecordView()
        {
            return new InterviewHistoricalRecordView(1, InterviewHistoricalAction.AnswerSet, "test", "a",
                new Dictionary<string, string>(), DateTime.Now);
        }
    }
}
