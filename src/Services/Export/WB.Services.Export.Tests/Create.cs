using System;
using Microsoft.Extensions.Options;
using Moq;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Services;
using WB.Services.Export.Tenant;

namespace WB.Services.Export.Tests
{
    public static class Create
    {
        public static InterviewDiagnosticsInfo InterviewDiagnosticsInfo(
            Guid? interviewId = null,
            string interviewKey = null,
            InterviewStatus status = InterviewStatus.InterviewerAssigned,
            Guid? responsibleId = null, 
            string responsibleName = null, 
            int numberOfInterviewers = 0, 
            int numberRejectionsBySupervisor = 0, 
            int numberRejectionsByHq = 0, 
            int numberValidQuestions = 0, 
            int numberInvalidEntities = 0, 
            int numberUnansweredQuestions = 0, 
            int numberCommentedQuestions = 0, 
            long? interviewDuration = null)
            => new InterviewDiagnosticsInfo
            {
                InterviewId = interviewId ?? Guid.NewGuid(),
                InterviewKey = interviewKey,
                Status = status,
                ResponsibleId = responsibleId ?? Guid.NewGuid(),
                ResponsibleName = responsibleName,
                NumberOfInterviewers = numberOfInterviewers,
                NumberRejectionsBySupervisor = numberRejectionsBySupervisor,
                NumberRejectionsByHq = numberRejectionsByHq,
                NumberValidQuestions = numberValidQuestions,
                NumberInvalidEntities = numberInvalidEntities,
                NumberUnansweredQuestions = numberUnansweredQuestions,
                NumberCommentedQuestions = numberCommentedQuestions,
                InterviewDuration = interviewDuration
            };

        public static DiagnosticsExporter DiagnosticsExporter(ICsvWriter csvWriter = null,
            IFileSystemAccessor fileSystemAccessor = null,
            ITenantApi<IHeadquartersApi> headquartersApi = null)
        {
            return new DiagnosticsExporter(Mock.Of<IOptions<InterviewDataExportSettings>>(x => x.Value == new InterviewDataExportSettings()),
                fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(),
                csvWriter ?? Mock.Of<ICsvWriter>(),
                headquartersApi ?? HeadquartersApi());
        }

        public static ITenantApi<IHeadquartersApi> TenantHeadquartersApi(IHeadquartersApi api)
        {
            return Mock.Of<ITenantApi<IHeadquartersApi>>(x => x.For(It.IsAny<TenantInfo>()) == api);
        }

        public static ITenantApi<IHeadquartersApi> HeadquartersApi()
        {
            return Mock.Of<ITenantApi<IHeadquartersApi>>();
        }

        public static TenantInfo Tenant()
        {
            return new TenantInfo();
        }
    }

}
