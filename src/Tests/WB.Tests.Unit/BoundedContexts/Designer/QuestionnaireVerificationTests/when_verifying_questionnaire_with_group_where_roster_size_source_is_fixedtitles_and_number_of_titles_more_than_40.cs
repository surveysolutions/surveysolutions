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

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVerificationTests
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

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(new Group()
            {
                PublicKey = rosterGroupId,
                IsRoster = true,
                RosterSizeSource = RosterSizeSourceType.FixedTitles,
                VariableName = "a",
                RosterFixedTitles = fixedTitles.ToArray(),
                Children = new List<IComposite>() { new TextQuestion(){StataExportCaption = "var"} }
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_errors = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_first_error_with_code__WB0038 = () =>
            resultErrors.First().Code.ShouldEqual("WB0038");

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid rosterGroupId;
    }
}
