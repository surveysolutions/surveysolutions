using Machine.Specifications;

using Main.Core.Documents;

using WB.Core.GenericSubdomains.Utils.Rest;

namespace WB.Tests.Unit.GenericSubdomains.Utils.NewtonJsonUtilsTests
{
    internal class when_deserializing_questionnaire_document_with_json_utils : NewtonJsonUtilsTestContext
    {
        Establish context = () =>
        {
            jsonUtils = CreateNewtonJsonUtils();
        };

        Because of = () =>
            result = jsonUtils.Deserialize<QuestionnaireDocument>(serializedQuestionnaire);

        It should_return_not_null_result = () =>
            result.ShouldNotBeNull();

        private const string serializedQuestionnaire = @"{""$type"":""Main.Core.Documents.QuestionnaireDocument, Main.Core"",""Children"":[],""CreationDate"":""2015-04-22T15:40:21.559966-04:00"",""LastEntryDate"":""2015-04-22T15:40:21.5834289-04:00"",""PublicKey"":""3708184d-f61a-4e93-849d-b308fb92697e""}";
        private static NewtonJsonUtils jsonUtils;
        private static QuestionnaireDocument result;
    }
}