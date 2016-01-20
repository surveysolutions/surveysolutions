using System.Threading;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services.Exporters;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.InterviewActionsExporterTests
{
    [Subject(typeof(InterviewActionsExporter))]
    internal class InterviewActionsExporterTestsContext
    {
        protected static InterviewActionsExporter CreateExporter(ICsvWriter csvWriter = null,
            IFileSystemAccessor fileSystemAccessor = null,
            IQueryableReadSideRepositoryReader<InterviewStatuses> interviewStatuses = null,
            QuestionnaireExportStructure questionnaireExportStructure = null)
        {
            return new InterviewActionsExporter(new InterviewDataExportSettings(), 
                fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(),
                csvWriter ?? Mock.Of<ICsvWriter>(),
                Create.TransactionManagerProvider(),
                interviewStatuses ?? new TestInMemoryWriter<InterviewStatuses>(),
                Mock.Of<ILogger>());
        }
    }
}