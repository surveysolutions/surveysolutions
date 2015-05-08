using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using Moq;
using WB.Core.GenericSubdomains.Utils.Rest;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ReadSideRepositoryDataExportWriterTests
{
    [Subject(typeof(ReadSideRepositoryDataExportWriter))]
    internal class ReadSideRepositoryDataExportWriterTestContext
    {
        protected static ReadSideRepositoryDataExportWriter CreateReadSideRepositoryDataExportWriter(
            IReadSideRepositoryWriter<InterviewExportedDataRecord> interviewExportedDataStorage = null,
            IReadSideRepositoryWriter<InterviewHistory> interviewActionsDataStorage = null)
        {
            return
                new ReadSideRepositoryDataExportWriter(
                    interviewExportedDataStorage ?? new TestInMemoryWriter<InterviewExportedDataRecord>(),
                    interviewActionsDataStorage ?? new TestInMemoryWriter<InterviewHistory>(),
                    new NewtonJsonUtils());
        }
    }
}
