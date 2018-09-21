using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using WB.Services.Export.CsvExport;
using WB.Services.Export.CsvExport.Exporters;
using WB.Services.Export.CsvExport.Implementation;
using WB.Services.Export.DescriptionGenerator;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Questionnaire.Services;
using WB.Services.Export.Services;
using WB.Services.Export.Tenant;
using WB.Services.Export.Utils;

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

        public static QuestionnaireExportStructure QuestionnaireExportStructure(string questionnaireId)
        {
            return new QuestionnaireExportStructure()
            {
                QuestionnaireId = questionnaireId
            };
        }

        public static HeaderStructureForLevel HeaderStructureForLevel()
        {
            return new HeaderStructureForLevel { LevelScopeVector = new ValueVector<Guid>() };
        }

        public static QuestionnaireDocument QuestionnaireDocument(Guid? id = null, params IQuestionnaireEntity[] children)
        {
            return new QuestionnaireDocument
            {
                PublicKey = id ?? Guid.NewGuid(),
                Children = children
            };
        }

        public static TextQuestion TextQuestion(string questionText)
        {
            return new TextQuestion
            {
                QuestionText = questionText,
                VariableName = Guid.NewGuid().FormatGuid()
            };
        }

        public static QuestionnaireExportStructure QuestionnaireExportStructure(QuestionnaireDocument questionnaire)
        {
            var fileSystemAccessor = new Mock<IFileSystemAccessor>();
            fileSystemAccessor
                .Setup(x => x.MakeValidFileName(It.IsAny<string>()))
                .Returns((string f) => f);

            var exportViewFactory = new QuestionnaireExportStructureFactory(
                Mock.Of<ICache>(),
                Mock.Of<IQuestionnaireStorage>());
            return exportViewFactory.GetQuestionnaireExportStructure(Create.Tenant(), questionnaire);
        }

        public static InterviewSummary InterviewSummary( InterviewExportedAction status = InterviewExportedAction.ApprovedBySupervisor,
            string originatorName = "inter",
            UserRoles originatorRole = UserRoles.Interviewer,
            Guid? interviewId = null,
            DateTime? timestamp = null,
            string key = null)
            => new InterviewSummary
            {
                Status = status,
                InterviewId = interviewId ?? Guid.NewGuid(),
                Timestamp = timestamp ?? DateTime.Now,
                Key = key,
                StatusChangeOriginatorRole = originatorRole,
                StatusChangeOriginatorName = originatorName,
                InterviewerName = "inter",
                SupervisorName = "supervisor",
            };

        public static InterviewActionsExporter InterviewActionsExporter(ITenantApi<IHeadquartersApi> tenantApi,
            ICsvWriter csvWriter = null)
        {
            return new InterviewActionsExporter(InterviewDataExportSettings(),
                csvWriter ?? Mock.Of<ICsvWriter>(),
                tenantApi ?? HeadquartersApi(),
                Mock.Of<ILogger<InterviewActionsExporter>>());
        }

        public static IOptions<InterviewDataExportSettings> InterviewDataExportSettings()
        {
            return Mock.Of<IOptions<InterviewDataExportSettings>>(x => x.Value == new InterviewDataExportSettings());
        }

        public static TabularFormatExportService ReadSideToTabularFormatExportService(QuestionnaireExportStructure questionnaireExportStructure,
            ITenantApi<IHeadquartersApi> tenantApi,
            IFileSystemAccessor fileSystemAccessor = null,
            ICsvWriter csvWriter = null)
        {
            return new TabularFormatExportService(Mock.Of<ILogger<TabularFormatExportService>>(),
                tenantApi,
                Mock.Of<IInterviewsExporter>(),
                Mock.Of<ICommentsExporter>(),
                Mock.Of<IDiagnosticsExporter>(),
                Mock.Of<IInterviewActionsExporter>(),
                Mock.Of<IQuestionnaireExportStructureFactory>(x => x.GetQuestionnaireExportStructure(It.IsAny<TenantInfo>(), It.IsAny<QuestionnaireId>()) == questionnaireExportStructure),
                Mock.Of<IQuestionnaireStorage>(),
                Mock.Of<IDescriptionGenerator>(),
                Mock.Of<IEnvironmentContentService>(),
                Mock.Of<IProductVersion>(),
                fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>());
        }
    }

}
