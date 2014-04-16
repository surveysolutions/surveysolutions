using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;
using WB.UI.Designer.Code;
using WB.UI.Designer.Models;

namespace WB.UI.Designer.Tests.VerificationErrorsMapperTests
{
    internal class when_enriching_errors_with_titles_from_questionnaire : VerificationErrorsMapperTestContext
    {
        Establish context = () =>
        {
            mapper = CreateVerificationErrorsMapper();
            verificationErrors = CreateQuestionnaireVerificationErrors(questionId, groupId);
            document = CreateQuestionnaireDocument(questionId, groupId, groupTitle, questionTitle);
        };

        Because of = () =>
            result = mapper.EnrichVerificationErrors(verificationErrors, document);

        It should_return_2_errors = () => 
            result.Length.ShouldEqual(2);

        It should_return_first_error_with_same_Code_as_input_error_has = () =>
            result.First().Code.ShouldEqual(verificationErrors.First().Code);

        It should_return_last_error_with_same_Code_as_input_error_has = () =>
            result.Last().Code.ShouldEqual(verificationErrors.Last().Code);

        It should_return_first_error_with_same_Message_as_input_error_has = () =>
            result.First().Message.ShouldEqual(verificationErrors.First().Message);

        It should_return_last_error_with_same_Message_as_input_error_has = () =>
            result.Last().Message.ShouldEqual(verificationErrors.Last().Message);

        It should_return_first_error_with_same_References_count_as_input_error_has = () =>
            result.First().References.Count.ShouldEqual(verificationErrors.First().References.Count());

        It should_return_last_error_with_same_References_count_as_input_error_has = () =>
            result.Last().References.Count.ShouldEqual(verificationErrors.Last().References.Count());

        It should_return_first_error_that_references_question_with_questionId = () =>
            result.First().References.First().Id.ShouldEqual(questionId);

        It should_return_last_error_that_references_question_with_questionId = () =>
            result.Last().References.First().Id.ShouldEqual(groupId);

        It should_return_first_error_that_references_question_with_questionTitle = () =>
            result.First().References.First().Title.ShouldEqual(questionTitle);

        It should_return_last_error_that_references_question_with_groupTitle = () =>
            result.Last().References.First().Title.ShouldEqual(groupTitle);


        private static IVerificationErrorsMapper mapper;
        private static QuestionnaireVerificationError[] verificationErrors;
        private static QuestionnaireDocument document;
        private static VerificationError[] result;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid groupId = Guid.Parse("22222222222222222222222222222222");
        private static string groupTitle = "Group Title";
        private static string questionTitle = "Question Title";
    }
}