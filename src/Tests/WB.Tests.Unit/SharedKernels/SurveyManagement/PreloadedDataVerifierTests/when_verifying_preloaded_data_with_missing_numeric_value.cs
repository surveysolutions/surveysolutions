using System;
using System.Collections.Generic;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class when_verifying_preloaded_data_with_missing_numeric_value : PreloadedDataVerifierTestContext
    {
        [Test]
        public void should_result_has_0_error()
        {
            var questionnaireId = Guid.Parse("11111111111111111111111111111111");
            var questionId = Guid.Parse("21111111111111111111111111111111");
            var questionnaire =
                CreateQuestionnaireDocumentWithOneChapter(new NumericQuestion
                {
                    StataExportCaption = "q1",
                    PublicKey = questionId,
                    IsInteger = true,
                    QuestionType = QuestionType.Numeric
                });
            questionnaire.Title = "questionnaire";
            var preloadedDataByFile = CreatePreloadedDataByFile(new[] {ServiceColumns.InterviewId, "q1"},
                new[] {new[] {"1", ExportFormatSettings.MissingNumericQuestionValue}},
                "questionnaire.csv");

            var preloadedDataServiceMock = new Mock<IPreloadedDataService>();
            preloadedDataServiceMock.Setup(x => x.FindLevelInPreloadedData(It.IsAny<string>()))
                .Returns(new HeaderStructureForLevel
                {
                    LevelIdColumnName = ServiceColumns.InterviewId,
                    HeaderItems =
                        new Dictionary<Guid, IExportedHeaderItem>
                        {
                            {
                                Guid.NewGuid(),
                                new ExportedQuestionHeaderItem
                                {
                                    VariableName = "q1",
                                    ColumnHeaders = new List<HeaderColumn> {new HeaderColumn {Name = "q1"}}
                                }
                            }
                        }
                });

            preloadedDataServiceMock.Setup(x => x.GetColumnIndexByHeaderName(preloadedDataByFile, It.IsAny<string>()))
                .Returns(-1);
            var importDataVerifier = CreatePreloadedDataVerifier(questionnaire, preloadedDataServiceMock.Object);

            // Act
            importDataVerifier.VerifyPanelFiles(questionnaireId, 1, Create.Entity.PreloadedDataByFile(preloadedDataByFile), status);

            // Assert
            status.VerificationState.Errors.Should().HaveCount(0);
        }
    }
}
