using System;
using System.Web.Http;
using System.Web.Http.Results;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.Headquarters.API;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Headquarters.ExportApiTests
{
    public class when_getting_export_process_details_for_paradata : ExportControllerTestsContext
    {
        Establish context = () =>
        {
            var questionnaireExportStructureStorage = Mock.Of<IQuestionnaireExportStructureStorage>(
                    x => x.GetQuestionnaireExportStructure(questionnaireIdentity) == new QuestionnaireExportStructure());

            paraDataExportProcessDetails = new ParaDataExportProcessDetails(DataExportFormat.Paradata)
            {
                Status = DataExportStatus.Running,
                ProgressInPercents = 98
            };

            var dataExportProcessesService = Mock.Of<IDataExportProcessesService>(
                x=> x.GetAllProcesses() == new[] {paraDataExportProcessDetails} &&
                    x.GetRunningExportProcesses() == new[] { paraDataExportProcessDetails});

            var dataExportStatusReader = Create.Service.DataExportStatusReader(
                questionnaireExportStructureStorage: questionnaireExportStructureStorage,
                dataExportProcessesService: dataExportProcessesService);

            controller = CreateExportController(dataExportStatusReader: dataExportStatusReader);
        };

        Because of = () => result = controller.ProcessDetails(questionnaireIdentity.ToString(), "paradata");

        It should_return_http_ok_response = () =>
            result.ShouldBeOfExactType<OkNegotiatedContentResult<ExportController.ExportDetails>>();

        It should_return_specified_json_object = () =>
        {
            var jsonResult = ((OkNegotiatedContentResult<ExportController.ExportDetails>) result).Content;

            jsonResult.ExportStatus.ShouldEqual(paraDataExportProcessDetails.Status.ToString());
            jsonResult.RunningProcess.ProgressInPercents.ShouldEqual(paraDataExportProcessDetails.ProgressInPercents);
        };
            

        private static ExportController controller;

        private static IHttpActionResult result;
        private static readonly QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("11111111111111111111111111111111"), 1);
        private static IDataExportProcessDetails paraDataExportProcessDetails;
    }
}