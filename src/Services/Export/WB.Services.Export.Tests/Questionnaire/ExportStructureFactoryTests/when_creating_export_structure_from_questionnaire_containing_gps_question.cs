using System;
using System.Linq;
using FluentAssertions;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Tests.Questionnaire.ExportStructureFactoryTests
{
    internal class when_creating_export_structure_from_questionnaire_containing_gps_question: ExportViewFactoryTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            gpsQuestionId = Guid.Parse("BBF000AAA111EE2DD2EE111AAA000AAA");

            questionnaireDocument =
                Create.QuestionnaireDocument(children: Create.GpsCoordinateQuestion(questionId: gpsQuestionId, variable:"gps", label:"gps label"));

            QuestionnaireExportStructureFactory = CreateExportViewFactory();
            BecauseOf();
        }

        public void BecauseOf() =>
            gpsExportedQuestionHeaderItem = QuestionnaireExportStructureFactory.CreateQuestionnaireExportStructure(questionnaireDocument).HeaderToLevelMap[new ValueVector<Guid>()].HeaderItems[gpsQuestionId] as ExportedQuestionHeaderItem;

        [NUnit.Framework.Test] public void should_create_header_with_5_columns_wich_corresponds_to_gps_properties () =>
            gpsExportedQuestionHeaderItem.ColumnHeaders.Select(x=> x.Name).Should().BeEquivalentTo(new[] { "gps__Latitude", "gps__Longitude", "gps__Accuracy", "gps__Altitude" , "gps__Timestamp"});

        [NUnit.Framework.Test] public void should_create_header_with_5_columns_with_valid_labels () =>
            gpsExportedQuestionHeaderItem.ColumnHeaders.Select(x=> x.Title).Should().BeEquivalentTo(new[] { "gps label: Latitude", "gps label: Longitude", "gps label: Accuracy", "gps label: Altitude", "gps label: Timestamp" });

        private static QuestionnaireExportStructureFactory QuestionnaireExportStructureFactory;
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid gpsQuestionId;
        private static ExportedQuestionHeaderItem gpsExportedQuestionHeaderItem;
    }
}
