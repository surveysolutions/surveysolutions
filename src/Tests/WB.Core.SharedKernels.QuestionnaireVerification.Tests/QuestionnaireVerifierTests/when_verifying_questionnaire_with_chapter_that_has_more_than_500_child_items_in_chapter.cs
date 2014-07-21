using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.QuestionnaireVerification.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.Core.SharedKernels.QuestionnaireVerification.Tests.QuestionnaireVerifierTests
{
    class when_verifying_questionnaire_with_chapter_that_has_more_than_500_child_items_in_chapter : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            var chapter = new Group("Chapter")
            {
                Children = new List<IComposite>()
                {
                    new NumericQuestion("first") { StataExportCaption = "first" },
                    new NumericQuestion("second") { StataExportCaption = "second" },
                    new Group("Group")
                }
            };

            for (int i = 0; i < 497; i++)
            {
                chapter.Children.Add(new NumericQuestion("question" + i)
                {
                    StataExportCaption = "question" + i
                });
            }

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(chapter);
            verifier = CreateQuestionnaireVerifier();
        };


        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_provide_one_error = () => resultErrors.Count().ShouldEqual(1);

        It should_provide_error_with_expected_code = () => resultErrors.First().Code.ShouldEqual("WB0072");
            
        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationError> resultErrors;
    }
}