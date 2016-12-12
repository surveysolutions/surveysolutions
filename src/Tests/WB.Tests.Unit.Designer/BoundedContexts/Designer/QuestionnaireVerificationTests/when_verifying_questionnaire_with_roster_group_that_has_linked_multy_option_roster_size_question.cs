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
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_roster_group_that_has_linked_multy_option_roster_size_question : QuestionnaireVerifierTestsContext
    {
        private Establish context = () =>
        {
            rosterGroupId = Guid.Parse("10000000000000000000000000000000");
            rosterSizeQuestionId = Guid.Parse("13333333333333333333333333333333");
            referencedQuestionId = Guid.Parse("12222222222222222222222222222222");
            questionnaire = CreateQuestionnaireDocument(new IComposite[]
            {
                new MultyOptionsQuestion("question 1")
                {
                    PublicKey = rosterSizeQuestionId,
                    StataExportCaption = "var1",
                    LinkedToQuestionId = referencedQuestionId,
                    QuestionType = QuestionType.MultyOption,
                    Answers =
                    {
                        new Answer() {AnswerValue = "1", AnswerText = "opt 1"},
                        new Answer() {AnswerValue = "2", AnswerText = "opt 2"}
                    }
                },
                new Group()
                {
                    PublicKey = rosterGroupId,
                    VariableName = "a",
                    IsRoster = true,
                    RosterSizeQuestionId = rosterSizeQuestionId,

                    Children = new IComposite[]
                    {
                        new NumericQuestion()
                        {
                            PublicKey = referencedQuestionId,
                            QuestionType = QuestionType.Numeric,
                            StataExportCaption = "var2"
                        }
                    }.ToReadOnlyCollection()
                }
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_return_1_message = () =>
            verificationMessages.Count().ShouldEqual(1);

        It should_return_message_with_code__WB0023__ = () =>
            verificationMessages.Single().Code.ShouldEqual("WB0023");

        It should_return_message_with_1_references = () =>
            verificationMessages.Single().References.Count().ShouldEqual(1);

        It should_return_message_reference_with_type_Roster = () =>
            verificationMessages.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Roster);

        It should_return_message_reference_with_id_of_rosterGroupId = () =>
            verificationMessages.Single().References.First().Id.ShouldEqual(rosterGroupId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid rosterGroupId;
        private static Guid rosterSizeQuestionId;
        private static Guid referencedQuestionId;
    }
}
