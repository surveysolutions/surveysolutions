using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.PreloadedData;
using It = Machine.Specifications.It;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class when_verifying_preloaded_data_and_data_has_all_service_variables : PreloadedDataVerifierTestContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter();
            questionnaire.Title = "questionnaire";
            questionnaireId = Guid.Parse("11111111111111111111111111111111");

            List<string> serviseNames = ServiceColumns.SystemVariables.Select(x => x.VariableExportColumnName).ToList();
            serviseNamesCount = serviseNames.Count();

            serviseNames.Add("Id");

            preloadedDataByFile = CreatePreloadedDataByFile(serviseNames.ToArray(), null, QuestionnaireCsvFileName);

            preloadedDataServiceMock=new Mock<IPreloadedDataService>();
            preloadedDataServiceMock.Setup(x => x.FindLevelInPreloadedData(QuestionnaireCsvFileName)).Returns(new HeaderStructureForLevel());
            preloadedDataVerifier = CreatePreloadedDataVerifier(questionnaire, preloadedDataServiceMock.Object);
        };

        Because of =
            () =>
                result =
                    preloadedDataVerifier.VerifyPanel(questionnaireId, 1, new[] { preloadedDataByFile });

        It should_result_has_serviseNamesCount_error = () =>
            result.Errors.Count().ShouldEqual(serviseNamesCount);

        It should_return_single_PL0003_error = () =>
            result.Errors.First().Code.ShouldEqual("PL0003");

        It should_return_reference_with_Column_type = () =>
            result.Errors.First().References.First().Type.ShouldEqual(PreloadedDataVerificationReferenceType.Column);

        private static PreloadedDataVerifier preloadedDataVerifier;
        private static VerificationStatus result;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionnaireId;
        private static PreloadedDataByFile preloadedDataByFile;
        private static Mock<IPreloadedDataService> preloadedDataServiceMock;
        private const string QuestionnaireCsvFileName = "questionnaire.csv";
        private static int serviseNamesCount;
    }
}
