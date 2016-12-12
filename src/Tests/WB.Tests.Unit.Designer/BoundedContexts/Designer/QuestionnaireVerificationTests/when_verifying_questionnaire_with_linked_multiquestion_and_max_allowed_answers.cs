using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    class when_verifying_questionnaire_that_have_linked_multiquestion_with_max_allowed_answers : QuestionnaireVerifierTestsContext
    {

        private Establish context = () =>
        {
            multyOptionsQuestionId = Guid.Parse("10000000000000000000000000000000");
            var linkedQuestionId = Guid.Parse("20000000000000000000000000000000");
            var rosterSizeQuestion = Guid.Parse("30000000000000000000000000000000");
            var rosterGroup = Guid.Parse("40000000000000000000000000000000");
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                    new NumericQuestion("Roster Size Question")
                    {
                        PublicKey = rosterSizeQuestion,
                        StataExportCaption = "var1",
                        IsInteger = true
                    },
                    new Group("Roster Group")
                    {
                        PublicKey = rosterGroup,
                        IsRoster = true,
                        VariableName = "a",
                        RosterSizeSource = RosterSizeSourceType.Question,
                        RosterSizeQuestionId = rosterSizeQuestion,
                        Children = new List<IComposite>()
                        {
                            new TextQuestion("TextQuestion")
                            {
                                StataExportCaption = "var2",
                                QuestionType = QuestionType.Text,
                                PublicKey = linkedQuestionId
                            }
                        }.ToReadOnlyCollection()
                    },
                    new MultyOptionsQuestion()
                    {
                        StataExportCaption = "var3",
                        PublicKey = multyOptionsQuestionId,
                        MaxAllowedAnswers = 3,
                        LinkedToQuestionId = linkedQuestionId
                    });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () => 
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_return_no_errors = () =>
             verificationMessages.ShouldBeEmpty();

        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;

        private static Guid multyOptionsQuestionId;
    }
}
