using System;
using System.Web.Http;
using System.Web.Http.Results;
using FluentAssertions;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;
using WB.UI.Headquarters.API;
using WB.UI.Headquarters.API.PublicApi;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Headquarters.ExportApiTests
{
    public class when_getting_export_process_details_for_paradata : ExportControllerTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaireExportStructureStorage = Mock.Of<IQuestionnaireExportStructureStorage>(
                    x => x.GetQuestionnaireExportStructure(questionnaireIdentity) == new QuestionnaireExportStructure());

            paraDataExportProcessDetails = new DataExportProcessDetails(DataExportFormat.Paradata,
                questionnaireIdentity, "questionnaire title")
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
            BecauseOf();
        }

        private void BecauseOf() => result = controller.ProcessDetails(questionnaireIdentity.ToString(), DataExportFormat.Paradata);

        [NUnit.Framework.Test] public void should_return_http_ok_response () =>
            result.Should().BeOfType<OkNegotiatedContentResult<ExportController.ExportDetails>>();

        [NUnit.Framework.Test] public void should_return_specified_json_object () 
        {
            var jsonResult = ((OkNegotiatedContentResult<ExportController.ExportDetails>) result).Content;

            jsonResult.ExportStatus.ShouldEqual(paraDataExportProcessDetails.Status);
            jsonResult.RunningProcess.ProgressInPercents.ShouldEqual(paraDataExportProcessDetails.ProgressInPercents);
        }
            

        private static ExportController controller;

        private static IHttpActionResult result;
        private static readonly QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("11111111111111111111111111111111"), 1);
        private static DataExportProcessDetails paraDataExportProcessDetails;
    }
}
