using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using NHibernate.Util;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.ExportViewFactoryTests
{
    internal class when_creating_export_structure_from_questionnaire_containing_gps_question: ExportViewFactoryTestsContext
    {
        Establish context = () =>
        {
            gpsQuestionId = Guid.Parse("BBF000AAA111EE2DD2EE111AAA000AAA");

            questionnaireDocument =
                Create.QuestionnaireDocument(children: Create.GpsCoordinateQuestion(questionId: gpsQuestionId, variableName:"gps"));

            exportViewFactory = CreateExportViewFactory();
        };

        Because of = () =>
            questionnaireExportStructure = exportViewFactory.CreateQuestionnaireExportStructure(questionnaireDocument, 1);

        It should_create_header_with_5_columns_wich_corresponds_to_gps_properties = () =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>()].HeaderItems[gpsQuestionId].ColumnNames.ShouldEqual(new[] { "gps_Latitude", "gps_Longitude", "gps_Accuracy", "gps_Altitude" , "gps_Timestamp"});

        private static QuestionnaireExportStructure questionnaireExportStructure;
        private static ExportViewFactory exportViewFactory;
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid gpsQuestionId;
    }
}