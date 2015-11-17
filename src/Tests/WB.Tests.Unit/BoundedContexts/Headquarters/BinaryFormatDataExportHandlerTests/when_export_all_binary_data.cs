using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Tests.Unit.SharedKernels.SurveyManagement;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.BinaryFormatDataExportHandlerTests
{
    internal class when_export_all_binary_data: BinaryFormatDataExportHandlerTestContext
    {
        Establish context = () =>
        {
            var interviewDataStorage = new TestInMemoryWriter<InterviewData>();
            var interviewSummarytorage = new TestInMemoryWriter<InterviewSummary>();
            var questionnaireStorage = new Mock<IReadSideKeyValueStorage<QuestionnaireExportStructure>>();
            binaryFormatDataExportHandler =
                CreateBinaryFormatDataExportHandler(interviewSummaries: interviewSummarytorage,
                    interviewDatas: interviewDataStorage);
        };

        Because of = () => binaryFormatDataExportHandler.ExportData(Create.AllDataExportProcess());


        private static BinaryFormatDataExportHandler binaryFormatDataExportHandler;
    }
}