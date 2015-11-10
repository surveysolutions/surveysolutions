using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DataExport.ReadSideToTabularFormatExportServiceTests
{
    internal class when_creating_template_for_preloading_from_questionnaire_export_structure : ReadSideToTabularFormatExportServiceTestContext
    {
        Establish context = () =>
        {
            questionnaireExportStructure = CreateQuestionnaireExportStructure(CreateHeaderStructureForLevel("main level"),
                CreateHeaderStructureForLevel("nested roster level", referenceNames: new[] { "r1", "r2" },
                    levelScopeVector: new ValueVector<Guid>(new[] { Guid.NewGuid(), Guid.NewGuid() })));
            csvWriterServiceTestable=new CsvWriterServiceTestable();
            readSideToTabularFormatExportService = CreateReadSideToTabularFormatExportService(csvWriterService: csvWriterServiceTestable, questionnaireExportStructure: questionnaireExportStructure);
        };

        Because of = () =>
            readSideToTabularFormatExportService.CreateHeaderStructureForPreloadingForQuestionnaire(new QuestionnaireIdentity(questionnaireId, questionnaireVersion),"");

        It should_craete_2_headers = () =>
            csvWriterServiceTestable.Rows.Count.ShouldEqual(2);

        It should_add_first_header_that_corresponds_to_interview = () =>
            csvWriterServiceTestable.Rows[0].ShouldEqual(new object[] { "Id", "1","a", "ssSys_IRnd"});

        It should_add_second_header_that_corresponds_to_nested_roster_level_of_the_interview = () =>
            csvWriterServiceTestable.Rows[1].ShouldEqual(new object[] { "Id", "r1", "r2", "1", "a", "ParentId1", "ParentId2" });

        private static ReadSideToTabularFormatExportService readSideToTabularFormatExportService;
        private static Guid questionnaireId=Guid.Parse("11111111111111111111111111111111");
        private static long questionnaireVersion=3;
        private static CsvWriterServiceTestable csvWriterServiceTestable;
        private static QuestionnaireExportStructure questionnaireExportStructure;
    }
}
