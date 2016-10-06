using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.UI.Designer.Code;
using WB.UI.Designer.Models;

namespace WB.Tests.Unit.Designer.Applications.VerificationErrorsMapperTests
{
    internal class when_enriching_errors_with_compilation_of_condition_on_questions : VerificationErrorsMapperTestContext
    {
        Establish context = () =>
        {
            mapper = CreateVerificationErrorsMapper();
            verificationMessages = new []
            {
                Create.VerificationError("aaa", "aaaa", new []{ "compile error 1", "compile error 2" }, new QuestionnaireNodeReference(QuestionnaireVerificationReferenceType.Question, Guid.Parse(questionId1))),
                Create.VerificationError("aaa", "aaaa", new []{ "compile error 3" }, new QuestionnaireNodeReference(QuestionnaireVerificationReferenceType.Question, Guid.Parse(questionId2))),
                Create.VerificationError("aaa", "bbbb", new []{ "compile error 3" }, new QuestionnaireNodeReference(QuestionnaireVerificationReferenceType.Question, Guid.Parse(questionId2))),

            };
            document = CreateQuestionnaireDocumentWith2TextQuestions(Guid.Parse(questionId1), Guid.Parse(questionId2), Guid.Parse(groupId));
        };

        Because of = () =>
            result = mapper.EnrichVerificationErrors(verificationMessages, document);

        It should_return_2_errors = () => 
            result.Length.ShouldEqual(2);

        It should_return_first_error_with_same_Code_as_input_error_has = () =>
        {
            result.ElementAt(0).Code.ShouldEqual(verificationMessages.ElementAt(0).Code);
            result.ElementAt(0).Code.ShouldEqual(verificationMessages.ElementAt(1).Code);
        };

        It should_return_first_error_with_same_Message_as_input_error_has = () =>
        {
            result.ElementAt(0).Message.ShouldEqual(verificationMessages.ElementAt(0).Message);
            result.ElementAt(0).Message.ShouldEqual(verificationMessages.ElementAt(1).Message);
        };

        It should_return_first_error_with_same_References_count_as_input_error_has = () =>
            result.ElementAt(0).Errors.SelectMany(e => e.References).Count().ShouldEqual(2);

        It should_return_first_error_that_references_question_with_questionId = () =>
        {
            result.ElementAt(0).Errors.First().References.ElementAt(0).ItemId.ShouldEqual(questionId1);
            result.ElementAt(0).Errors.Second().References.ElementAt(0).ItemId.ShouldEqual(questionId2);
        };

        It should_return_first_error_with_IsGroupOfErrors_field_set_in_true = () =>
            result.ElementAt(0).IsGroupedMessage.ShouldBeTrue();

        It should_return_2_errors_in_first_error_group = () =>
            result.ElementAt(0).Errors.Count.ShouldEqual(2);

        It should_return_last_error_with_same_Code_as_input_error_has = () =>
            result.ElementAt(1).Code.ShouldEqual(verificationMessages.ElementAt(2).Code);

        It should_return_last_error_with_same_Message_as_input_error_has = () =>
            result.ElementAt(1).Message.ShouldEqual(verificationMessages.ElementAt(2).Message);

        It should_return_last_error_with_same_References_count_as_input_error_has = () =>
            result.ElementAt(1).Errors.First().References.Count.ShouldEqual(verificationMessages.ElementAt(2).References.Count());

        It should_return_last_error_that_references_question_with_questionId = () =>
            result.ElementAt(1).Errors.First().References.ElementAt(0).ItemId.ShouldEqual(questionId2);

        It should_return_last_error_with_IsGroupOfErrors_field_set_in_true = () =>
            result.ElementAt(1).IsGroupedMessage.ShouldBeTrue();

        It should_return_1_error_in_second_error_group = () =>
            result.ElementAt(1).Errors.Count.ShouldEqual(1);


        private static IVerificationErrorsMapper mapper;
        private static QuestionnaireVerificationMessage[] verificationMessages;
        private static QuestionnaireDocument document;
        private static VerificationMessage[] result;
        private static string questionId1 = "11111111111111111111111111111111";
        private static string questionId2 = "22222222222222222222222222222222";
        private static string groupId = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
    }
}