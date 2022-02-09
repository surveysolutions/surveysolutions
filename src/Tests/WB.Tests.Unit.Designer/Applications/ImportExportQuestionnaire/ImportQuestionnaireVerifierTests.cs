using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ImportExport;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.Applications.ImportExportQuestionnaire
{
    [TestOf(typeof(ImportQuestionnaireVerifier))]
    public class ImportQuestionnaireVerifierTests
    {
        [Test]
        public void check_DuplicatePublicId()
        {
            var questionnaireDocument = Create.QuestionnaireDocument(children: new IComposite[]
            {
                Create.TextListQuestion(Id.g1),
                Create.Roster(Id.g1)
            });
            ShouldContainError(questionnaireDocument, "QM0006");
        }

        [Test]
        public void check_DuplicateVariableName()
        {
            var questionnaireDocument = Create.QuestionnaireDocument(children: new IComposite[]
            {
                Create.TextListQuestion(Id.g1, variable: "var"),
                Create.Roster(Id.g2, variable: "var")
            });
            ShouldContainError(questionnaireDocument, "QM0007");
        }

        private void ShouldContainError(QuestionnaireDocument questionnaireDocument, string errorCode)
        {
            var verifier = new ImportQuestionnaireVerifier();
            var verify = verifier.Verify(
                new ReadOnlyQuestionnaireDocument(questionnaireDocument));
            verify.ShouldContainError(errorCode);
        }
    }
}
