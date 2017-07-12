using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.ReadSideToTabularFormatExportServiceTests
{
    internal class when_creating_template_for_preloading_from_questionnaire_export_structure : ReadSideToTabularFormatExportServiceTestContext
    {
        Establish context = () =>
        {
            questionnaireExportStructure = CreateQuestionnaireExportStructure(
                questionnaireId,
                questionnaireVersion,
                CreateHeaderStructureForLevel("main level"),
                CreateHeaderStructureForLevel("nested roster level", referenceNames: new[] { "r1", "r2" },
                    levelScopeVector: new ValueVector<Guid>(new[] { Guid.NewGuid(), Guid.NewGuid() })));

            csvWriterMock.Setup(
                x => x.WriteData(Moq.It.IsAny<string>(), Moq.It.IsAny<IEnumerable<string[]>>(), Moq.It.IsAny<string>()))
                .Callback<string, IEnumerable<string[]>, string>((filePath, data, delimiter) => { rows.Add(data); });

            readSideToTabularFormatExportService = Create.Service.ReadSideToTabularFormatExportService(csvWriter: csvWriterMock.Object,
                questionnaireExportStructure: questionnaireExportStructure);
        };

        Because of = () =>
            readSideToTabularFormatExportService.CreateHeaderStructureForPreloadingForQuestionnaire(new QuestionnaireIdentity(questionnaireId, questionnaireVersion),"");

        It should_craete_2_headers = () =>
            rows.Count.ShouldEqual(2);

        It should_add_first_header_that_corresponds_to_interview = () =>
            rows[0].First().ShouldEqual(new object[] { ServiceColumns.Id, "1","a", "ssSys_IRnd", ServiceColumns.Key });

        It should_add_second_header_that_corresponds_to_nested_roster_level_of_the_interview = () =>
            rows[1].First().ShouldEqual(new object[] { ServiceColumns.Id, "r1", "r2", "1", "a", "ParentId1", "ParentId2" });

        private static ReadSideToTabularFormatExportService readSideToTabularFormatExportService;
        private static Guid questionnaireId=Guid.Parse("11111111111111111111111111111111");
        private static long questionnaireVersion=3;
        private static Mock<ICsvWriter> csvWriterMock = new Mock<ICsvWriter>();
        private static QuestionnaireExportStructure questionnaireExportStructure;
        private static List<IEnumerable<string[]>> rows=new List<IEnumerable<string[]>>();
    }
}
