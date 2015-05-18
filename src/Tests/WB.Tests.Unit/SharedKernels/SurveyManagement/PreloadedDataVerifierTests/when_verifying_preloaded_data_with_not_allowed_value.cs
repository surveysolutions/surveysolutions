using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.PreloadedData;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class when_verifying_preloaded_data_with_not_allowed_value : PreloadedDataVerifierTestContext
    {
        private Establish context = () =>
        {
            questionnaireId = Guid.Parse("11111111111111111111111111111111");
            questionId = Guid.Parse("21111111111111111111111111111111");
            var question = new NumericQuestion()
            {
                StataExportCaption = "q1",
                PublicKey = questionId,
                IsInteger = true,
                QuestionType = QuestionType.Numeric
            };
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(question);
            questionnaire.Title = "questionnaire";
            preloadedDataByFile = CreatePreloadedDataByFile(new[] { "Id", "q1"},
                new string[][] { new string[] { "1", "text"} },
                "questionnaire.csv");

            preloadedDataServiceMock = new Mock<IPreloadedDataService>();
            preloadedDataServiceMock.Setup(x => x.FindLevelInPreloadedData(Moq.It.IsAny<string>()))
                .Returns(new HeaderStructureForLevel()
                {
                    HeaderItems =
                        new Dictionary<Guid, ExportedHeaderItem>
                        {
                            { Guid.NewGuid(), new ExportedHeaderItem() { VariableName = "q1", ColumnNames = new[] { "q1" } } }
                        }
                });

            preloadedDataServiceMock.Setup(x => x.GetColumnIndexesGoupedByQuestionVariableName(preloadedDataByFile))
                .Returns(new Dictionary<string, int[]> { { "q1", new[] { 1 } } });

            KeyValuePair<Guid, object> outValue;

            preloadedDataServiceMock.Setup(
                x => x.ParseQuestion(Moq.It.IsAny<string>(), Moq.It.IsAny<IQuestion>(), out outValue)).Returns(ValueParsingResult.ParsedValueIsNotAllowed);
            preloadedDataServiceMock.Setup(x => x.GetColumnIndexByHeaderName(preloadedDataByFile, Moq.It.IsAny<string>())).Returns(-1);

            preloadedDataVerifier = CreatePreloadedDataVerifier(questionnaire, null, preloadedDataServiceMock.Object);
        };

        private Because of =
            () =>
                result = preloadedDataVerifier.VerifyPanel(questionnaireId, 1, new[] { preloadedDataByFile });

        private It should_result_has_1_error = () =>
            result.Errors.Count().ShouldEqual(1);

        private It should_return_single_PL0014_error = () =>
            result.Errors.First().Code.ShouldEqual("PL0014");

        private It should_return_reference_with_Cell_type = () =>
            result.Errors.First().References.First().Type.ShouldEqual(PreloadedDataVerificationReferenceType.Cell);

        private static PreloadedDataVerifier preloadedDataVerifier;
        private static VerificationStatus result;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionnaireId;
        private static Guid questionId;
        private static PreloadedDataByFile preloadedDataByFile;

        private static Mock<IPreloadedDataService> preloadedDataServiceMock;
    }
}
