using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_group_where_roster_size_source_is_fixedtitles_and_number_of_titles_more_than_40 :
        QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            rosterGroupId = Guid.Parse("13333333333333333333333333333333");

            var fixedTitles = new List<string>();
            for (int i = 0; i < 41; i++)
            {
                fixedTitles.Add(string.Format("Fixed Title {0}", i));
            }

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                Create.FixedRoster(rosterId: rosterGroupId,
                    variable: "a",
                    fixedTitles: fixedTitles.ToArray(),
                    children: new IComposite[]
                    {new TextQuestion() {StataExportCaption = "var"}}));

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(questionnaire);

        It should_return_error_with_code__WB0038 = () =>
            verificationMessages.ShouldContainError("WB0038");

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid rosterGroupId;
    }
}
