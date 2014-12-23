using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;
using It = Moq.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ViewsTests.InterviewHistoryWriterTests
{
    [Subject(typeof(InterviewHistoryWriter))]
    public class InterviewHistoryWriterTestContext
    {
        protected static InterviewHistoryWriter CreateInterviewHistoryWriter(ICsvWriterService csvWriterService = null,
            IFileSystemAccessor fileSystemAccessor = null, IReadSideRepositoryWriter<InterviewSummary> interviewSummaryWriter = null, IFilebasedExportedDataAccessor filebasedExportedDataAccessor=null)
        {
            return new InterviewHistoryWriter(Mock.Of<ICsvWriterFactory>(_=>_.OpenCsvWriter(It.IsAny<Stream>(),It.IsAny<string>())== Mock.Of<ICsvWriterService>()),
                fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(),
                interviewSummaryWriter ?? Mock.Of<IReadSideRepositoryWriter<InterviewSummary>>(), filebasedExportedDataAccessor??Mock.Of<IFilebasedExportedDataAccessor>());
        }

        protected static InterviewHistoryView CreateInterviewHistoryView()
        {
            return new InterviewHistoryView(Guid.NewGuid(), new List<InterviewHistoricalRecordView>(), Guid.NewGuid(), 1);
        }

        protected static InterviewHistoricalRecordView CreateInterviewHistoricalRecordView()
        {
            return new InterviewHistoricalRecordView(1, InterviewHistoricalAction.AnswerSet, "test", "a",
                new Dictionary<string, string>(), DateTime.Now);
        }
    }
}
