using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Assignments
{
    [TestOf(typeof(ImportDataVerifier))]
    public class ImportDataVerifierProtectedAnswersTests
    {
        private string preloadedFileName = $"{ServiceFiles.ProtectedVariables}.tab";

        [Test]
        public void should_not_allow_to_protect_variable_that_is_not_in_questionnaire()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneQuestion();

            var verifier = Create.Service.ImportDataVerifier();

            var errors = 
                verifier.VerifyProtectedVariables(Create.Entity.PreloadedFile(preloadedFileName,
                            Create.Entity.PreloadingRow(Create.Entity.PreloadingValue(ServiceColumns.ProtectedVariableNameColumn, "bla"))), 
                        Create.Entity.PlainQuestionnaire(questionnaire))
                    .ToList();

            Assert.That(errors, Has.Count.EqualTo(1));
            Assert.That(errors.FirstOrDefault(), Has.Property(nameof(PanelImportVerificationError.Code)).EqualTo("PL0048"));
        }

        [Test]
        public void when_question_is_not_supporting_protection([Values] QuestionType questionType)
        {
            QuestionType[] typesThatSupportProtection = new[]
            {
                QuestionType.MultyOption, QuestionType.Numeric, QuestionType.TextList
            };

            if (typesThatSupportProtection.Contains(questionType)) return;

            var variableName = "myVariable";
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.Question(variable: variableName, questionType: questionType));

            var verifier = Create.Service.ImportDataVerifier();

            var errors = 
                verifier.VerifyProtectedVariables(Create.Entity.PreloadedFile(preloadedFileName,
                            Create.Entity.PreloadingRow(Create.Entity.PreloadingValue(ServiceColumns.ProtectedVariableNameColumn, variableName))), 
                        Create.Entity.PlainQuestionnaire(questionnaire))
                    .ToList();

            Assert.That(errors, Has.Count.EqualTo(1));
            Assert.That(errors.FirstOrDefault(), Has.Property(nameof(PanelImportVerificationError.Code)).EqualTo("PL0049"));
        }

        [Test]
        public void should_not_allow_protect_non_integer_numeric_question()
        {
            var variableName = "myVariable";
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericRealQuestion(variable: variableName));

            var verifier = Create.Service.ImportDataVerifier();

            var errors = 
                verifier.VerifyProtectedVariables(Create.Entity.PreloadedFile(preloadedFileName,
                            Create.Entity.PreloadingRow(Create.Entity.PreloadingValue(ServiceColumns.ProtectedVariableNameColumn, variableName))), 
                        Create.Entity.PlainQuestionnaire(questionnaire))
                    .ToList();

            Assert.That(errors, Has.Count.EqualTo(1));
            Assert.That(errors.FirstOrDefault(), Has.Property(nameof(PanelImportVerificationError.Code)).EqualTo("PL0049"));
        }

        [Test]
        public void should_not_allow_protect_numeric_question_with_special_values()
        {
            var variableName = "myVariable";
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(variable: variableName, specialValues: Create.Entity.Answer("one", 1).ToEnumerable()));

            var verifier = Create.Service.ImportDataVerifier();

            var errors = 
                verifier.VerifyProtectedVariables(Create.Entity.PreloadedFile(preloadedFileName,
                            Create.Entity.PreloadingRow(Create.Entity.PreloadingValue(ServiceColumns.ProtectedVariableNameColumn, variableName))), 
                        Create.Entity.PlainQuestionnaire(questionnaire))
                    .ToList();

            Assert.That(errors, Has.Count.EqualTo(1));
            Assert.That(errors.FirstOrDefault(), Has.Property(nameof(PanelImportVerificationError.Code)).EqualTo("PL0049"));
        }
    }
}
