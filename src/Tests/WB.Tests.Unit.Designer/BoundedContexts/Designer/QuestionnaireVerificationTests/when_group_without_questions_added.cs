using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_group_without_questions_added : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            groupId = Guid.NewGuid();
            questionnaire = Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.TextQuestion(),
                Create.Group(groupId: groupId)
            });
            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () => errors = verifier.Verify(Create.QuestionnaireView(questionnaire));

        It should_return_warning = () => errors.Where(x => x.MessageLevel == VerificationMessageLevel.Warning).ShouldNotBeEmpty();

        It should_return_WB0202_warning_with_reference_to_empty_group = () =>
        {
            var warning = errors
                .Where(x => x.MessageLevel == VerificationMessageLevel.Warning)
                .SingleOrDefault(x => x.Code == "WB0202");

            warning.ShouldNotBeNull();
            warning.References.First().Id.ShouldEqual(groupId);
        };


        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationMessage> errors;
        static Guid groupId;
    }
}