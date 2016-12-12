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
    class when_verifying_questionnaire_with_question_that_has_two_question_with_same_variable_names : QuestionnaireVerifierTestsContext
    {

        private Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(new Group("Chapter")
            {
                Children = new List<IComposite>()
                {
                    new NumericQuestion("first")
                    {
                        PublicKey = firstQuestionId,
                        StataExportCaption = variableName
                    },
                    new NumericQuestion("second")
                    {
                        PublicKey = secondQuestionId,
                        StataExportCaption = variableName
                    }
                }.ToReadOnlyCollection()
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_return_WB0026_error = () =>
            verificationMessages.ShouldContainCritical("WB0026");

        It should_return_message_with_level_critical = () =>
            verificationMessages.GetCritical("WB0026").MessageLevel.ShouldEqual(VerificationMessageLevel.Critical);

        It should_return_message_with_1_reference = () =>
            verificationMessages.GetCritical("WB0026").References.Count().ShouldEqual(2);

        It should_return_message_reference_with_type_Question = () =>
            verificationMessages.GetCritical("WB0026").References.ShouldEachConformTo(reference => reference.Type == QuestionnaireVerificationReferenceType.Question);

        It should_return_message_reference_with_first_and_second_question_Id = () =>
            verificationMessages.GetCritical("WB0026").References.Select(x => x.Id).ShouldContainOnly(firstQuestionId, secondQuestionId);

        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;

        private static Guid firstQuestionId = Guid.Parse("a1111111111111111111111111111111");
        private static Guid secondQuestionId = Guid.Parse("b1111111111111111111111111111111");
        private static string variableName = "same";

    }
}
