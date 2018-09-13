using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services.Exporters;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.CommentsExporterTests
{
    [NUnit.Framework.TestOf(typeof(CommentsExporter))]
    internal class CommentsExporterTestsContext
    {
        protected static CommentsExporter CreateExporter(IFileSystemAccessor fileSystemAccessor = null, 
            ICsvWriter csvWriter = null,
            IQueryableReadSideRepositoryReader<InterviewCommentaries> interviewCommentaries = null,
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryStorage = null)
        {
            return new CommentsExporter(new InterviewDataExportSettings(),
                fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(),
                csvWriter ?? Mock.Of<ICsvWriter>(),
                interviewCommentaries ?? new TestInMemoryWriter<InterviewCommentaries>(),
                Create.Service.TransactionManagerProvider(),
                Mock.Of<ILogger>(),
                interviewSummaryStorage ?? new TestInMemoryWriter<InterviewSummary>());
        }
    }
}
