using FluentAssertions;
using Main.Core.Documents;
using WB.Infrastructure.Native.Storage;

namespace WB.Tests.Unit.GenericSubdomains.Utils.NewtonJsonUtilsTests
{
    internal class when_deserializing_questionnaire_document_with_json_utils 
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            BecauseOf();
        }

        public void BecauseOf() =>
            result = _jsonSerializer.Deserialize<QuestionnaireDocument>(serializedQuestionnaire);

        [NUnit.Framework.Test] public void should_return_not_null_result () =>
            result.Should().NotBeNull();

        private const string serializedQuestionnaire = @"{""$type"":""Main.Core.Documents.QuestionnaireDocument, Main.Core"",""Children"":[],""CreationDate"":""2015-04-22T15:40:21.559966-04:00"",""LastEntryDate"":""2015-04-22T15:40:21.5834289-04:00"",""PublicKey"":""3708184d-f61a-4e93-849d-b308fb92697e""}";
        private static NewtonJsonSerializer _jsonSerializer = new NewtonJsonSerializer();
        private static QuestionnaireDocument result;
    }
}
