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

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
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
                new TextQuestion() { PublicKey = questionEndsWithUnderscoreId, StataExportCaption = endsWithUnderscoreVariableName, QuestionType = QuestionType.Text },

                new TextQuestion() { PublicKey = questioncontainsConsecutiveUnderscores, StataExportCaption = containsConsecutiveUnderscoresVariableName, QuestionType = QuestionType.Text },

                new GpsCoordinateQuestion() { PublicKey = questionWith20SymbolVariableNameId , StataExportCaption = longThen20SymbolVariableName, QuestionType = QuestionType.GpsCoordinates}

            });
            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_return_7_messages = () =>
            verificationMessages.Count().ShouldEqual(7);

        It should_return_all_errors_with_code_WB0077 = () =>
            verificationMessages.ShouldEachConformTo(e => e.Code == "WB0077");

        It should_return_first_message_with_one_references = () =>
            verificationMessages.First().References.Count().ShouldEqual(1);

        It should_return_first_message_with_first_references_with_Question_type = () =>
            verificationMessages.First().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_first_message_with_first_references_with_id_equals_questionWithUnicodeVariableNameId = () =>
            verificationMessages.First().References.First().Id.ShouldEqual(questionWithUnicodeVariableNameId);

        It should_return_second_message_with_one_references = () =>
           verificationMessages.Skip(1).First().References.Count().ShouldEqual(1);

        It should_return_second_message_with_first_references_with_Question_type = () =>
            verificationMessages.Skip(1).First().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_second_message_with_first_references_with_id_equals_questionWithUnicodeVariableNameId = () =>
            verificationMessages.Skip(1).First().References.First().Id.ShouldEqual(questionStartsWithNumberVariableNameId);

        It should_return_third_message_with_one_references = () =>
           verificationMessages.Skip(2).First().References.Count().ShouldEqual(1);

        It should_return_third_message_with_first_references_with_Question_type = () =>
            verificationMessages.Skip(2).First().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_third_message_with_first_references_with_id_equals_questionWithUnicodeVariableNameId = () =>
            verificationMessages.Skip(2).First().References.First().Id.ShouldEqual(questionStartsWithLongThen32SymbolVariableNameId);

        It should_return_fourth_message_with_one_references = () =>
           verificationMessages.Skip(3).First().References.Count().ShouldEqual(1);

        It should_return_fourth_message_with_first_references_with_Question_type = () =>
            verificationMessages.Skip(3).First().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_fourth_message_with_first_references_with_id_equals_questionWithUnicodeVariableNameId = () =>
            verificationMessages.Skip(3).First().References.First().Id.ShouldEqual(questionStartsWithUnderscoreId);

        It should_return_5_message_with_one_references = () =>
           verificationMessages.Skip(4).First().References.Count().ShouldEqual(1);

        It should_return_5_message_with_first_references_with_Question_type = () =>
            verificationMessages.Skip(4).First().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_5_message_with_first_references_with_id_equals_questionWithUnicodeVariableNameId = () =>
            verificationMessages.Skip(4).First().References.First().Id.ShouldEqual(questionEndsWithUnderscoreId);

        It should_return_6_message_with_one_references = () =>
           verificationMessages.Skip(5).First().References.Count().ShouldEqual(1);

        It should_return_6_message_with_first_references_with_Question_type = () =>
            verificationMessages.Skip(5).First().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_6_message_with_first_references_with_id_equals_questionWithUnicodeVariableNameId = () =>
            verificationMessages.Skip(5).First().References.First().Id.ShouldEqual(questioncontainsConsecutiveUnderscores);

        It should_return_7_message_with_one_references = () =>
           verificationMessages.Skip(6).First().References.Count().ShouldEqual(1);

        It should_return_7_message_with_first_references_with_Question_type = () =>
            verificationMessages.Skip(6).First().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_7_message_with_first_references_with_id_equals_questionWithUnicodeVariableNameId = () =>
            verificationMessages.Skip(6).First().References.First().Id.ShouldEqual(questionWith20SymbolVariableNameId);



        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionWithUnicodeVariableNameId = Guid.NewGuid();
        private static Guid questionStartsWithNumberVariableNameId = Guid.NewGuid();
        private static Guid questionStartsWithLongThen32SymbolVariableNameId = Guid.NewGuid();

        private static Guid questionStartsWithUnderscoreId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid questionEndsWithUnderscoreId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDA");

        private static Guid questioncontainsConsecutiveUnderscores = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDB");

        private static Guid questionWith20SymbolVariableNameId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDC");

        private static string nonUnicodeVariableName = "variableЙФЪ";
        private static string startsWithNumberVariableName = "1variable";
        private static string startsWithUnderscoreVariableName = "_variable";
        private static string endsWithUnderscoreVariableName = "variable_";
        private static string longThen32SymbolVariableName = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

        private static string longThen20SymbolVariableName = "a23456789012345678901";

        private static string containsConsecutiveUnderscoresVariableName = "vari__able";
    }
}
