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
    class when_verifying_questionnaire_that_has_lookup_table_with_11_headers : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = Create.QuestionnaireDocument(questionnaireId, Create.TextQuestion(variable: "var"));
            questionnaire.LookupTables.Add(tableId, Create.LookupTable("hello"));

            lookupTableServiceMock
                .Setup(x => x.GetLookupTableContent(questionnaireId, tableId))
                .Returns(lookupTableContent);

            verifier = CreateQuestionnaireVerifier(lookupTableService: lookupTableServiceMock.Object);
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_return_1_message = () =>
            verificationMessages.Count().ShouldEqual(1);

        It should_return_message_with_code__WB0043 = () =>
            verificationMessages.Single().Code.ShouldEqual("WB0043");

        It should_return_message_with_Critical_level = () =>
            verificationMessages.Single().MessageLevel.ShouldEqual(VerificationMessageLevel.Critical);

        It should_return_message_with_1_reference = () =>
            verificationMessages.Single().References.Count().ShouldEqual(1);

        It should_return_message_reference_with_type_LookupTable = () =>
            verificationMessages.Single().References.ElementAt(0).Type.ShouldEqual(QuestionnaireVerificationReferenceType.LookupTable);

        It should_return_message_reference_with_id_of_table = () =>
            verificationMessages.Single().References.ElementAt(0).Id.ShouldEqual(tableId);

        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static readonly Mock<ILookupTableService> lookupTableServiceMock = new Mock<ILookupTableService>();
        private static readonly LookupTableContent lookupTableContent = Create.LookupTableContent(new[] { "header1", "header2", "header3", "header4", "header5", "header6", "header7", "header8", "header9", "header10", "header11" },
            Create.LookupTableRow(1, new decimal?[] { 1.15m, 10 }),
            Create.LookupTableRow(2, new decimal?[] { 1, 10 }),
            Create.LookupTableRow(3, new decimal?[] { 1, 10 })
            );

        private static readonly Guid tableId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid questionnaireId = Guid.Parse("10000000000000000000000000000000");
    }
}