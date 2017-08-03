using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.UI.Designer.Code;
using WB.UI.Designer.Models;

namespace WB.Tests.Unit.Designer.Applications.VerificationErrorsMapperTests
{
    internal class when_enriching_errors_with_empty_static_text : VerificationErrorsMapperTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            mapper = CreateVerificationErrorsMapper();
            verificationMessages = CreateStaticTextVerificationError(Guid.Parse(staticTextId));
            document = CreateQuestionnaireDocumentWithStaticText(Guid.Parse(staticTextId), Guid.Parse(chapterId));
            BecauseOf();
        }

        private void BecauseOf() =>
            result = mapper.EnrichVerificationErrors(verificationMessages, document);

        [NUnit.Framework.Test] public void should_return_1_error () => 
            result.Length.ShouldEqual(1);

        [NUnit.Framework.Test] public void should_return_error_with_same_Code_as_input_error_has () =>
            result.First().Code.ShouldEqual(verificationMessages.First().Code);
        
        [NUnit.Framework.Test] public void should_return_error_with_same_Message_as_input_error_has () =>
            result.First().Message.ShouldEqual(verificationMessages.First().Message);
        
        [NUnit.Framework.Test] public void should_return_error_with_same_References_count_as_input_error_has () =>
            result.First().Errors.First().References.Count.ShouldEqual(verificationMessages.First().References.Count());

        [NUnit.Framework.Test] public void should_return_error_with_IsGroupOfErrors_field_set_in_true () =>
            result.First().IsGroupedMessage.ShouldBeTrue();
        
        [NUnit.Framework.Test] public void should_return_error_that_references_static_text_with_staticTextId () =>
            result.First().Errors.First().References.First().ItemId.ShouldEqual(staticTextId);

        [NUnit.Framework.Test] public void should_return_error_that_references_static_text_with_specified_QuestionnaireVerificationReferenceType () =>
            result.First().Errors.First().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.StaticText);

        [NUnit.Framework.Test] public void should_return_error_that_references_static_text_with_specified_text () =>
            result.First().Errors.First().References.First().Title.ShouldEqual("static text");
        
        private static IVerificationErrorsMapper mapper;
        private static QuestionnaireVerificationMessage[] verificationMessages;
        private static QuestionnaireDocument document;
        private static VerificationMessage[] result;
        private static string staticTextId = "11111111111111111111111111111111";
        private static string chapterId = "22222222222222222222222222222222";
    }
}