using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_more_than_200_questions_added_to_one_group : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            IComposite[] questions = Enumerable.Range(0, 201).Select(index => Create.Question(variable: "numeric" + index)).ToArray();
            questionnaire = Create.QuestionnaireDocumentWithOneChapter(children: questions);
            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () => errors = verifier.Verify(Create.QuestionnaireView(questionnaire));

        It should_return_warning = () => errors.Where(x => x.MessageLevel == VerificationMessageLevel.Warning).ShouldNotBeEmpty();

        It should_return_WB0201_warning = () => errors.Where(x => x.MessageLevel == VerificationMessageLevel.Warning)
                                                      .Select(x => x.Code)
                                                      .ShouldContain("WB0201");

        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationMessage> errors;
    }
}