using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;
using WB.UI.Designer.Code;
using WB.UI.Designer.Models;

namespace WB.Tests.Unit.Applications.Designer.VerificationErrorsMapperTests
{
    internal class when_enriching_errors_with_empty_static_text : VerificationErrorsMapperTestContext
    {
        Establish context = () =>
        {
            mapper = CreateVerificationErrorsMapper();
            verificationErrors = CreateStaticTextVerificationError(Guid.Parse(staticTextId));
            document = CreateQuestionnaireDocumentWithStaticText(Guid.Parse(staticTextId), Guid.Parse(chapterId));
        };

        Because of = () =>
            result = mapper.EnrichVerificationErrors(verificationErrors, document);

        It should_return_1_error = () => 
            result.Length.ShouldEqual(1);

        It should_return_error_with_same_Code_as_input_error_has = () =>
            result.First().Code.ShouldEqual(verificationErrors.First().Code);
        
        It should_return_error_with_same_Message_as_input_error_has = () =>
            result.First().Message.ShouldEqual(verificationErrors.First().Message);
        
        It should_return_error_with_same_References_count_as_input_error_has = () =>
            result.First().References.Count.ShouldEqual(verificationErrors.First().References.Count());
        
        It should_return_error_that_references_static_text_with_staticTextId = () =>
            result.First().References.First().ItemId.ShouldEqual(staticTextId);

        It should_return_error_that_references_static_text_with_specified_QuestionnaireVerificationReferenceType = () =>
            result.First().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.StaticText);

        It should_return_error_that_references_static_text_with_specified_text = () =>
            result.First().References.First().Title.ShouldEqual("static text");
        
        private static IVerificationErrorsMapper mapper;
        private static QuestionnaireVerificationError[] verificationErrors;
        private static QuestionnaireDocument document;
        private static VerificationError[] result;
        private static string staticTextId = "11111111111111111111111111111111";
        private static string chapterId = "22222222222222222222222222222222";
    }
}