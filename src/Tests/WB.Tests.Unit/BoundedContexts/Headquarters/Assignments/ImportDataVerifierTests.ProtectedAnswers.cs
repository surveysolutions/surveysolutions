using System.Linq;
using FluentAssertions;
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
        private readonly string preloadedFileName = $"{ServiceFiles.ProtectedVariables}.tab";

        [Test]
        public void should_not_allow_to_protect_variable_that_is_not_in_questionnaire()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneQuestion();

            var verifier = Create.Service.ImportDataVerifier();

            var errors =
                verifier.VerifyProtectedVariables(
                        preloadedFileName,
                        Create.Entity.PreloadedFile(preloadedFileName,
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
                verifier.VerifyProtectedVariables(preloadedFileName,Create.Entity.PreloadedFile(preloadedFileName,
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

            var preloadedFile = Create.Entity.PreloadedFile(preloadedFileName,
                Create.Entity.PreloadingRow(Create.Entity.PreloadingValue(ServiceColumns.ProtectedVariableNameColumn, variableName)));
            var errors =
                verifier.VerifyProtectedVariables(preloadedFileName,preloadedFile,
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
                verifier.VerifyProtectedVariables(preloadedFileName,
                        Create.Entity.PreloadedFile(preloadedFileName,
                            Create.Entity.PreloadingRow(Create.Entity.PreloadingValue(ServiceColumns.ProtectedVariableNameColumn, variableName))),
                        Create.Entity.PlainQuestionnaire(questionnaire))
                    .ToList();

            Assert.That(errors, Has.Count.EqualTo(1));
            Assert.That(errors.FirstOrDefault(), Has.Property(nameof(PanelImportVerificationError.Code)).EqualTo("PL0049"));
        }

        [Test]
        public void should_not_allow_preloading_without_variable__name_column()
        {
            var verifier = Create.Service.ImportDataVerifier();

            var preloadedFile = Create.Entity.PreloadedFile(ServiceFiles.ProtectedVariables);
            preloadedFile.FileInfo = Create.Entity.PreloadedFileInfo(new[] {"c1"});

            var errors =
                verifier.VerifyProtectedVariables(
                        preloadedFileName, 
                        preloadedFile,
                        Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneQuestion()))
                    .ToList();

            errors.Should().Contain(x => x.Code == "PL0047");
        }

        [Test]
        public void when_protected_variable_in_different_caps()
        {
            var variableName = "multi";
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultyOptionsQuestion(variable: variableName));

            var verifier = Create.Service.ImportDataVerifier();

            var preloadingValue = Create.Entity.PreloadingValue(
                ServiceColumns.ProtectedVariableNameColumn, variableName.ToUpper());

            var preloadingRow = Create.Entity.PreloadingRow(preloadingValue);

            var preloadedFile = Create.Entity.PreloadedFile(preloadedFileName, preloadingRow);

            var errors = verifier.VerifyProtectedVariables(preloadedFileName, preloadedFile,
                        Create.Entity.PlainQuestionnaire(questionnaire))
                    .ToList();

            Assert.That(errors, Is.Empty);
        }
    }
}
