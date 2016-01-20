using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services.Exporters;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.CommentsExporterTests
{
    [Subject(typeof(CommentsExporter))]
    internal class CommentsExporterTestsContext
    {
        protected static CommentsExporter CreateExporter(IFileSystemAccessor fileSystemAccessor = null, 
            ICsvWriter csvWriter = null,
            IQueryableReadSideRepositoryReader<InterviewCommentaries> interviewCommentaries = null)
        {
            return new CommentsExporter(new InterviewDataExportSettings(),
                fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(),
                csvWriter ?? Mock.Of<ICsvWriter>(),
                interviewCommentaries ?? new TestInMemoryWriter<InterviewCommentaries>(),
                Create.TransactionManagerProvider(),
                Mock.Of<ILogger>());
        }
    }
}