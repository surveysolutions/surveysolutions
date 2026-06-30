using FluentAssertions;
using Main.Core.Documents;
using NUnit.Framework;
using WB.Infrastructure.Native.Storage;
using WB.Tests.Abc;

namespace WB.Tests.Unit.GenericSubdomains.Utils.NewtonJsonUtilsTests
{
    [TestOf(typeof(QuestionnaireDocument))]
    internal class when_deserializing_questionnaire_document_with_null_category_item
    {
        [OneTimeSetUp]
        public void context()
        {
            result = jsonSerializer.Deserialize<QuestionnaireDocument>(serializedQuestionnaire);
        }

        [Test]
        public void should_remove_null_item_from_categories_list()
        {
            result.Categories.Should().HaveCount(2);
            result.Categories[0].Id.Should().Be(Id.g1);
            result.Categories[1].Id.Should().Be(Id.g2);
        }

        private const string serializedQuestionnaire =
            @"{""$type"":""Main.Core.Documents.QuestionnaireDocument, Main.Core"",""Children"":[],""Categories"":[{""Id"":""11111111-1111-1111-1111-111111111111"",""Name"":""Category 1""},null,{""Id"":""22222222-2222-2222-2222-222222222222"",""Name"":""Category 2""},null],""CreationDate"":""2015-04-22T15:40:21.559966-04:00"",""LastEntryDate"":""2015-04-22T15:40:21.5834289-04:00"",""PublicKey"":""3708184d-f61a-4e93-849d-b308fb92697e""}";

        private static readonly NewtonJsonSerializer jsonSerializer = new NewtonJsonSerializer();
        private static QuestionnaireDocument result;
    }
}
