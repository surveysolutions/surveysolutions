using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.QuestionnaireVerification.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.Core.SharedKernels.QuestionnaireVerification.Tests.QuestionnaireVerifierTests
{
    internal class when_verifying_questionnaire_with_roster_with_nested_plain_group : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            rosterGroupId = Guid.Parse("10000000000000000000000000000000");
            nestedGroupId = Guid.Parse("30000000000000000000000000000000");
            questionnaire = CreateQuestionnaireDocument();

            var rosterSizeQiestionId = Guid.Parse("20000000000000000000000000000000");

            questionnaire.Children.Add(new NumericQuestion() { PublicKey = rosterSizeQiestionId, IsInteger = true, MaxValue = 5, StataExportCaption = "var" });
            questionnaire.Children.Add(new Group()
            {
                PublicKey = rosterGroupId,
                IsRoster = true,
                VariableName = "a",
                RosterSizeQuestionId = rosterSizeQiestionId,
                Children = new List<IComposite>()
                {
                    new Group("nested field")
                    {
                        PublicKey = Guid.NewGuid()
                    }
                }
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_0_error = () =>
            resultErrors.Count().ShouldEqual(0);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid rosterGroupId;
        private static Guid nestedGroupId;
    }
}
