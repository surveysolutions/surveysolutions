using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    class when_verifying_questionnaire_that_has_lookup_tables_with_same_name_as_question : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            lookupTableServiceMock
                .Setup(x => x.GetLookupTableContent(Moq.It.IsAny<Guid>(), Moq.It.IsAny<Guid>()))
                .Returns(lookupTableContent);

            questionnaire = Create.QuestionnaireDocument(Guid.NewGuid(), Create.TextQuestion(questionId: questionId, variable: "var"));
            questionnaire.LookupTables.Add(table1Id, Create.LookupTable("var"));

            verifier = CreateQuestionnaireVerifier(lookupTableService: lookupTableServiceMock.Object);
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_WB0026_error () =>
           verificationMessages.ShouldContainCritical("WB0026");

        [NUnit.Framework.Test] public void should_return_message_with_Critical_level () =>
            verificationMessages.GetCritical("WB0026").MessageLevel.ShouldEqual(VerificationMessageLevel.Critical);

        [NUnit.Framework.Test] public void should_return_message_with_1_reference () =>
            verificationMessages.GetCritical("WB0026").References.Count().ShouldEqual(2);

        [NUnit.Framework.Test] public void should_return_first_message_reference_with_type_Question () =>
            verificationMessages.GetCritical("WB0026").References.ElementAt(0).Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_second_message_reference_with_type_LookupTable () =>
            verificationMessages.GetCritical("WB0026").References.ElementAt(1).Type.ShouldEqual(QuestionnaireVerificationReferenceType.LookupTable);

        [NUnit.Framework.Test] public void should_return_first_message_reference_with_id_of_question () =>
            verificationMessages.GetCritical("WB0026").References.ElementAt(0).Id.ShouldEqual(questionId);

        [NUnit.Framework.Test] public void should_return_second_message_reference_with_id_of_table () =>
            verificationMessages.GetCritical("WB0026").References.ElementAt(1).Id.ShouldEqual(table1Id);

        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;

        private static readonly Mock<ILookupTableService> lookupTableServiceMock = new Mock<ILookupTableService>();
        private static readonly LookupTableContent lookupTableContent = Create.LookupTableContent(new[] { "min", "max" },
           Create.LookupTableRow(1, new decimal?[] { 1.15m, 10 }),
           Create.LookupTableRow(2, new decimal?[] { 1, 10 }),
           Create.LookupTableRow(3, new decimal?[] { 1, 10 })
        );

        private static readonly Guid table1Id = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid questionId = Guid.Parse("10000000000000000000000000000000");
    }
}