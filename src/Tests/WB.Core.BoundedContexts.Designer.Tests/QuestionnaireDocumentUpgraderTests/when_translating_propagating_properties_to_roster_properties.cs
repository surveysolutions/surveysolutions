using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Implementation.Services;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireDocumentUpgraderTests
{
    internal class when_translating_propagating_properties_to_roster_properties : QuestionnaireDocumentUpgraderTestsContext
    {
        Establish context = () =>
        {
            initialDocument = CreateQuestionnaireDocument();
            upgrader = CreateQuestionnaireDocumentUpgrader();
        };

        Because of = () =>
            resultDocument = upgrader.TranslatePropagatePropertiesToRosterProperties(initialDocument);

        It should_return_cloned_copy_of_document = () =>
            resultDocument.ShouldNotBeTheSameAs(initialDocument);

        private static QuestionnaireDocumentUpgrader upgrader;
        private static QuestionnaireDocument initialDocument;
        private static QuestionnaireDocument resultDocument;
    }
}