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

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_that_has_question_with_variable_name_which_contains_unicode_symbols : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocument(new IComposite[]
            {
                new TextQuestion() { PublicKey = questionWithUnicodeVariableNameId, StataExportCaption = nonUnicodeVariableName, QuestionType = QuestionType.Text },
                new TextQuestion() { PublicKey = questionStartsWithNumberVariableNameId, StataExportCaption = startsWithNumberVariableName, QuestionType = QuestionType.Text },
                new TextQuestion() { PublicKey = questionStartsWithLongThen32SymbolVariableNameId, StataExportCaption = longThen32SymbolVariableName, QuestionType = QuestionType.Text },
                new TextQuestion() { PublicKey = questionStartsWithUnderscoreId, StataExportCaption = startsWithUnderscoreVariableName, QuestionType = QuestionType.Text },
                new TextQuestion() { PublicKey = questionEndsWithUnderscoreId, StataExportCaption = endsWithUnderscoreVariableName, QuestionType = QuestionType.Text }

            });
            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_3_errors = () =>
            resultErrors.Count().ShouldEqual(5);

        It should_return_all_errors_with_code_WB0077 = () =>
            resultErrors.ShouldEachConformTo(e => e.Code == "WB0077");

        It should_return_first_error_with_one_references = () =>
            resultErrors.First().References.Count().ShouldEqual(1);

        It should_return_first_error_with_first_references_with_Question_type = () =>
            resultErrors.First().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_first_error_with_first_references_with_id_equals_questionWithUnicodeVariableNameId = () =>
            resultErrors.First().References.First().Id.ShouldEqual(questionWithUnicodeVariableNameId);

        It should_return_second_error_with_one_references = () =>
           resultErrors.Skip(1).First().References.Count().ShouldEqual(1);

        It should_return_second_error_with_first_references_with_Question_type = () =>
            resultErrors.Skip(1).First().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_second_error_with_first_references_with_id_equals_questionWithUnicodeVariableNameId = () =>
            resultErrors.Skip(1).First().References.First().Id.ShouldEqual(questionStartsWithNumberVariableNameId);

        It should_return_third_error_with_one_references = () =>
           resultErrors.Skip(2).First().References.Count().ShouldEqual(1);

        It should_return_third_error_with_first_references_with_Question_type = () =>
            resultErrors.Skip(2).First().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_third_error_with_first_references_with_id_equals_questionWithUnicodeVariableNameId = () =>
            resultErrors.Skip(2).First().References.First().Id.ShouldEqual(questionStartsWithLongThen32SymbolVariableNameId);

        It should_return_fourth_error_with_one_references = () =>
           resultErrors.Skip(3).First().References.Count().ShouldEqual(1);

        It should_return_fourth_error_with_first_references_with_Question_type = () =>
            resultErrors.Skip(3).First().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_fourth_error_with_first_references_with_id_equals_questionWithUnicodeVariableNameId = () =>
            resultErrors.Skip(3).First().References.First().Id.ShouldEqual(questionStartsWithUnderscoreId);

        It should_return_last_error_with_one_references = () =>
           resultErrors.Last().References.Count().ShouldEqual(1);

        It should_return_last_error_with_first_references_with_Question_type = () =>
            resultErrors.Last().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_last_error_with_first_references_with_id_equals_questionWithUnicodeVariableNameId = () =>
            resultErrors.Last().References.First().Id.ShouldEqual(questionEndsWithUnderscoreId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionWithUnicodeVariableNameId = Guid.NewGuid();
        private static Guid questionStartsWithNumberVariableNameId = Guid.NewGuid();
        private static Guid questionStartsWithLongThen32SymbolVariableNameId = Guid.NewGuid();

        private static Guid questionStartsWithUnderscoreId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid questionEndsWithUnderscoreId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDA");

        private static string nonUnicodeVariableName = "variableЙФЪ";
        private static string startsWithNumberVariableName = "1variable";
        private static string startsWithUnderscoreVariableName = "_variable";
        private static string endsWithUnderscoreVariableName = "variable_";
        private static string longThen32SymbolVariableName = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    }
}
