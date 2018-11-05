using System;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;


namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests
{
    class when_verifying_questionnaire_that_has_lookup_table_with_empty_content : QuestionnaireVerifierTestsContext
    {
        [Test]
        public void should_return_error_when_0_rows_provided()
        {
            Guid tableId = Guid.Parse("11111111111111111111111111111111");
            Guid questionnaireId = Guid.Parse("10000000000000000000000000000000");

            var questionnaire = Create.QuestionnaireDocument(questionnaireId, Create.TextQuestion(variable: "var"));
            questionnaire.LookupTables.Add(tableId, Create.LookupTable("hello"));

            Mock<ILookupTableService> lookupTableServiceMock = new Mock<ILookupTableService>();
            lookupTableServiceMock
                .Setup(x => x.GetLookupTableContent(questionnaireId, tableId))
                .Returns((LookupTableContent)null);

            var verifier = CreateQuestionnaireVerifier(lookupTableService: lookupTableServiceMock.Object);

            // Act
            var verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire)).ToList();

            // Assert
            verificationMessages.Should().HaveCount(1);

            var singleVerificationError = verificationMessages.Single();

            singleVerificationError.Code.Should().Be("WB0048");
            singleVerificationError.MessageLevel.Should().Be(VerificationMessageLevel.Critical);
            singleVerificationError.References.Should().HaveCount(1);
            singleVerificationError.References.ElementAt(0).Type.Should().Be(QuestionnaireVerificationReferenceType.LookupTable);
            singleVerificationError.References.ElementAt(0).Id.Should().Be(tableId);
        }

        [Test]
        public void should_return_error_when_1_row_without_variables_provided()
        {
            Guid tableId = Guid.Parse("11111111111111111111111111111111");
            Guid questionnaireId = Guid.Parse("10000000000000000000000000000000");

            var questionnaire = Create.QuestionnaireDocument(questionnaireId, Create.TextQuestion(variable: "var"));
            questionnaire.LookupTables.Add(tableId, Create.LookupTable("hello"));

            Mock<ILookupTableService> lookupTableServiceMock = new Mock<ILookupTableService>();
            lookupTableServiceMock
                .Setup(x => x.GetLookupTableContent(questionnaireId, tableId))
                .Returns(new LookupTableContent
                {
                    VariableNames = Array.Empty<string>(),
                    Rows = new[]{Create.LookupTableRow(1, Array.Empty<decimal?>())}
                });

            var verifier = CreateQuestionnaireVerifier(lookupTableService: lookupTableServiceMock.Object);

            // Act
            var verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire)).ToList();

            // Assert
            verificationMessages.Should().HaveCount(1);

            var singleVerificationError = verificationMessages.Single();

            singleVerificationError.Code.Should().Be("WB0048");
            singleVerificationError.MessageLevel.Should().Be(VerificationMessageLevel.Critical);
            singleVerificationError.References.Should().HaveCount(1);
            singleVerificationError.References.ElementAt(0).Type.Should().Be(QuestionnaireVerificationReferenceType.LookupTable);
            singleVerificationError.References.ElementAt(0).Id.Should().Be(tableId);
        }
    }
}
