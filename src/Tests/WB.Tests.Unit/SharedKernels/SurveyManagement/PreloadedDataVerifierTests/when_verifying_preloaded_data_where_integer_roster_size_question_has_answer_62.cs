using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class when_verifying_preloaded_data_where_integer_roster_size_question_has_answer_62 : PreloadedDataVerifierTestContext
    {
        private Establish context = () =>
        {
            questionnaireId = Guid.Parse("11111111111111111111111111111111");
            numericQuestionId = Guid.Parse("21111111111111111111111111111111");
             
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(chapterChildren:
            new IComposite[]
            {
                Create.Entity.NumericIntegerQuestion(numericQuestionId, "num"),
                Create.Entity.Roster(rosterSizeQuestionId: numericQuestionId, rosterSizeSourceType: RosterSizeSourceType.Question, children: new IComposite[]
                {
                     Create.Entity.Roster(rosterSizeQuestionId: numericQuestionId, rosterSizeSourceType: RosterSizeSourceType.Question)
                })
            });

            questionnaire.Title = "questionnaire";
            preloadedDataByFile = CreatePreloadedDataByFile(new[] { "Id", "num" },
                new[] { new[] { "1", "62" } },
                "questionnaire.csv");

            var preloadedDataService = Create.Service.PreloadedDataService(questionnaire);

            importDataVerifier = CreatePreloadedDataVerifier(questionnaire, preloadedDataService);
        };

        Because of = () =>
            importDataVerifier.VerifyPanelFiles(questionnaireId, 1, new[] { preloadedDataByFile }, status);

        It should_result_has_1_error = () =>
            status.VerificationState.Errors.Count().ShouldEqual(1);

        It should_return_single_PL0029_error = () =>
            status.VerificationState.Errors.First().Code.ShouldEqual("PL0029");

        It should_return_reference_with_Cell_type = () =>
            status.VerificationState.Errors.First().References.First().Type.ShouldEqual(PreloadedDataVerificationReferenceType.Cell);

        private static ImportDataVerifier importDataVerifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionnaireId;
        private static Guid numericQuestionId;
        private static PreloadedDataByFile preloadedDataByFile;
    }
}