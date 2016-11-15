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
    class when_verifying_questionnaire_that_has_two_lookup_tables_with_same_names : QuestionnaireVerifierTestsContext
    {

        Establish context = () =>
        {
            questionnaire = Create.QuestionnaireDocument(questionId, Create.TextQuestion(variable: "var"));
            questionnaire.LookupTables.Add(table1Id, Create.LookupTable("hello"));
            questionnaire.LookupTables.Add(table2Id, Create.LookupTable("hello"));

            lookupTableServiceMock
                .Setup(x => x.GetLookupTableContent(Moq.It.IsAny<Guid>(), Moq.It.IsAny<Guid>()))
                .Returns(lookupTableContent);

            verifier = CreateQuestionnaireVerifier(lookupTableService: lookupTableServiceMock.Object);
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_return_1_message = () =>
            verificationMessages.Count().ShouldEqual(1);

        It should_return_message_with_code__WB0026 = () =>
            verificationMessages.Single().Code.ShouldEqual("WB0026");

        It should_return_message_with_Critical_level = () =>
            verificationMessages.Single().MessageLevel.ShouldEqual(VerificationMessageLevel.Critical);

        It should_return_message_with_1_reference = () =>
            verificationMessages.Single().References.Count().ShouldEqual(2);

        It should_return_message_reference_with_type_LookupTable = () =>
            verificationMessages.Single().References.ShouldEachConformTo(reference => reference.Type == QuestionnaireVerificationReferenceType.LookupTable);

        It should_return_message_reference_with_id_of_table1 = () =>
            verificationMessages.Single().References.ElementAt(0).Id.ShouldEqual(table1Id);

        It should_return_message_reference_with_id_of_table2 = () =>
            verificationMessages.Single().References.ElementAt(1).Id.ShouldEqual(table2Id);

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
        private static readonly Guid table2Id = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid questionId = Guid.Parse("10000000000000000000000000000000");

    }
}