using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.UI.Designer.Code;
using WB.UI.Designer.Models;

namespace WB.Tests.Unit.Applications.VerificationErrorsMapperTests
{
    internal class when_enriching_errors_with_titles_from_questionnaire : VerificationErrorsMapperTestContext
    {
        Establish context = () =>
        {
            mapper = CreateVerificationErrorsMapper();
            verificationMessages = CreateQuestionnaireVerificationErrors(Guid.Parse(questionId), Guid.Parse(groupId));
            document = CreateQuestionnaireDocument(Guid.Parse(questionId), Guid.Parse(groupId), groupTitle, questionTitle);
        };

        Because of = () =>
            result = mapper.EnrichVerificationErrors(verificationMessages, document);

        It should_return_2_errors = () => 
            result.Length.ShouldEqual(3);

        It should_return_first_error_with_same_Code_as_input_error_has = () =>
            result.ElementAt(0).Code.ShouldEqual(verificationMessages.ElementAt(0).Code);

        It should_return_first_error_with_same_Message_as_input_error_has = () =>
            result.ElementAt(0).Message.ShouldEqual(verificationMessages.ElementAt(0).Message);

        It should_return_first_error_with_same_References_count_as_input_error_has = () =>
            result.ElementAt(0).References.Count.ShouldEqual(2);

        It should_return_first_error_that_references_question_with_questionId = () =>
            result.ElementAt(0).References.ElementAt(0).ItemId.ShouldEqual(questionId);

        It should_return_first_error_that_references_group_with_groupId = () =>
            result.ElementAt(0).References.ElementAt(1).ItemId.ShouldEqual(groupId);

        It should_return_first_error_that_references_question_with_questionTitle = () =>
            result.ElementAt(0).References.ElementAt(0).Title.ShouldEqual(questionTitle);

        It should_return_first_error_with_IsGroupOfErrors_field_set_in_true = () =>
            result.ElementAt(0).IsGroupedMessage.ShouldBeTrue();

        It should_return_last_error_with_same_Code_as_input_error_has = () =>
            result.ElementAt(1).Code.ShouldEqual(verificationMessages.ElementAt(1).Code);

        It should_return_last_error_with_same_Message_as_input_error_has = () =>
            result.ElementAt(1).Message.ShouldEqual(verificationMessages.ElementAt(1).Message);

        It should_return_last_error_with_same_References_count_as_input_error_has = () =>
            result.ElementAt(1).References.Count.ShouldEqual(verificationMessages.ElementAt(1).References.Count());

        It should_return_last_error_that_references_question_with_questionId = () =>
            result.ElementAt(1).References.ElementAt(0).ItemId.ShouldEqual(groupId);

        It should_return_last_error_that_references_question_with_groupTitle = () =>
            result.ElementAt(1).References.ElementAt(0).Title.ShouldEqual(groupTitle);

        It should_return_last_error_with_IsGroupOfErrors_field_set_in_true = () =>
            result.ElementAt(1).IsGroupedMessage.ShouldBeTrue();

        It should_return_third_error_with_same_Code_as_input_error_has = () =>
            result.ElementAt(2).Code.ShouldEqual(verificationMessages.ElementAt(2).Code);

        It should_return_third_error_with_same_Message_as_input_error_has = () =>
            result.ElementAt(2).Message.ShouldEqual(verificationMessages.ElementAt(2).Message);

        It should_return_third_error_with_same_References_count_as_input_error_has = () =>
            result.ElementAt(2).References.Count.ShouldEqual(2);

        It should_return_third_error_that_references_question_with_questionId = () =>
            result.ElementAt(2).References.ElementAt(0).ItemId.ShouldEqual(questionId);

        It should_return_third_error_that_references_group_with_groupId = () =>
            result.ElementAt(2).References.ElementAt(1).ItemId.ShouldEqual(groupId);

        It should_return_third_error_that_references_question_with_questionTitle = () =>
            result.ElementAt(2).References.ElementAt(0).Title.ShouldEqual(questionTitle);

        It should_return_third_error_with_IsGroupOfErrors_field_set_in_true = () =>
            result.ElementAt(2).IsGroupedMessage.ShouldBeFalse();


        private static IVerificationErrorsMapper mapper;
        private static QuestionnaireVerificationMessage[] verificationMessages;
        private static QuestionnaireDocument document;
        private static VerificationMessage[] result;
        private static string questionId = "11111111111111111111111111111111";
        private static string groupId = "22222222222222222222222222222222";
        private static string groupTitle = "Group Title";
        private static string questionTitle = "Question Title";
    }
}