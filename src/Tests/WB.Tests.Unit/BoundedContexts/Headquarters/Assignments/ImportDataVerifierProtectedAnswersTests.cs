using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.Assignments
{
    [TestOf(typeof(ImportDataVerifier))]
    public class ImportDataVerifierProtectedAnswersTests
    {
        [Test]
        public void should_not_allow_preloading_of_file_without_variable_name()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneQuestion();

            var verifier = Create.Service.ImportDataVerifier();

            List<PanelImportVerificationError> errors = 
                verifier.VerifyAndParseProtectedVariables("test", 
                    new List<string[]>{ new []{"bla"}}, Create.Entity.PlainQuestionnaire(questionnaire), out _);

            Assert.That(errors, Has.Count.EqualTo(1));
            Assert.That(errors.FirstOrDefault(), Has.Property(nameof(PanelImportVerificationError.Code)).EqualTo("PL0047"));
        }

        [Test]
        public void should_not_allow_to_protect_variable_that_is_not_in_questionnaire()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneQuestion();

            var verifier = Create.Service.ImportDataVerifier();

            List<PanelImportVerificationError> errors = 
                verifier.VerifyAndParseProtectedVariables("test", 
                    new List<string[]>{ new []{ServiceColumns.ProtectedVariableNameColumn}, new []{"m1"}}, Create.Entity.PlainQuestionnaire(questionnaire), out _);

            Assert.That(errors, Has.Count.EqualTo(1));
            Assert.That(errors.FirstOrDefault(), Has.Property(nameof(PanelImportVerificationError.Code)).EqualTo("PL0048"));
        }

        [Test]
        public void should_return_list_of_protected_variables()
        {
            var variableName = "multiple";
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultipleOptionsQuestion(variable: variableName));

            var verifier = Create.Service.ImportDataVerifier();

            List<PanelImportVerificationError> errors = 
                verifier.VerifyAndParseProtectedVariables("test", 
                    new List<string[]>{ new []{ServiceColumns.ProtectedVariableNameColumn}, new []{variableName}}, Create.Entity.PlainQuestionnaire(questionnaire),
                    out List<string> protectedVariables);

            Assert.That(protectedVariables, Has.Count.EqualTo(1));
            Assert.That(protectedVariables.FirstOrDefault(), Is.EqualTo(variableName));
            Assert.That(errors, Is.Empty);
        }
    }
}
