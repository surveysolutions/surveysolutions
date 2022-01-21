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
    [TestOf(typeof(ExportQuestionnaireVerifier ))]
    public class ExportQuestionnaireVerifierTests
    {
        [Test]
        public void check_LinkedToQuestionMustHaveVariable()
        {
            var questionnaireDocument = Create.QuestionnaireDocument(children: new []
            {
                Create.Question(Id.g1, variable: ""),
                Create.SingleQuestion(Id.g2, linkedToQuestionId: Id.g1)
            });
            ShouldContainError(questionnaireDocument, "QM0001");
        }

        [Test]
        public void check_LinkedToRosterMustHaveVariable()
        {
            var questionnaireDocument = Create.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Roster(Id.g1, variable: ""),
                Create.SingleQuestion(Id.g2, linkedToRosterId: Id.g1)
            });
            ShouldContainError(questionnaireDocument, "QM0002");
        }

        [Test]
        public void check_CascadeFromQuestionMustHaveVariable()
        {
            var questionnaireDocument = Create.QuestionnaireDocument(children: new IComposite[]
            {
                Create.SingleQuestion(Id.g1, variable: ""),
                Create.SingleQuestion(Id.g2, cascadeFromQuestionId: Id.g1)
            });
            ShouldContainError(questionnaireDocument, "QM0003");
        }
        
        [Test]
        public void check_RosterTriggerMustHaveVariable()
        {
            var questionnaireDocument = Create.QuestionnaireDocument(children: new IComposite[]
            {
                Create.TextListQuestion(Id.g1, variable: ""),
                Create.Roster(Id.g2,  rosterSizeQuestionId: Id.g1)
            });
            ShouldContainError(questionnaireDocument, "QM0004");
        }
        
        [Test]
        public void check_RosterTitleMustHaveVariable()
        {
            var questionnaireDocument = Create.QuestionnaireDocument(children: new IComposite[]
            {
                Create.TextListQuestion(Id.g1, variable: ""),
                Create.Roster(Id.g2,  rosterTitleQuestionId: Id.g1)
            });
            ShouldContainError(questionnaireDocument, "QM0005");
        }

        private void ShouldContainError(QuestionnaireDocument questionnaireDocument, string errorCode)
        {
            var verifier = new ExportQuestionnaireVerifier();
            var verify = verifier.Verify(new ReadOnlyQuestionnaireDocument(questionnaireDocument));
            verify.ShouldContainError(errorCode);
        }
    }
}
