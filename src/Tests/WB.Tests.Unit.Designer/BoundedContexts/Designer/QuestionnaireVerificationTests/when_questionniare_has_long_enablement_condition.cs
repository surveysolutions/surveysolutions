using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_questionniare_has_long_enablement_condition : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            groupId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                Create.Group(groupId: groupId,
                    enablementCondition: new string('*', 501)),
                Create.Question(
                    questionId: questionId,
                    enablementCondition: new string('*', 501))
                );

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () => errors = verifier.Verify(Create.QuestionnaireView(questionnaire));

        It should_produce_WB0205_warning = () => errors.Count(x => x.Code == "WB0209").ShouldEqual(2);

        It should_reference_wrong_question = () => 
            errors.Where(x => x.Code == "WB0209").First().References.ShouldContain(Create.VerificationReference(id: groupId, type: QuestionnaireVerificationReferenceType.Group));

        It should_reference_wrong_group = () =>
            errors.Where(x => x.Code == "WB0209").Second().References.ShouldContain(Create.VerificationReference(id: questionId));

        static Guid questionId;
        static QuestionnaireVerifier verifier;
        static QuestionnaireDocument questionnaire;
        static IEnumerable<QuestionnaireVerificationMessage> errors;
        static Guid groupId;
    }
}