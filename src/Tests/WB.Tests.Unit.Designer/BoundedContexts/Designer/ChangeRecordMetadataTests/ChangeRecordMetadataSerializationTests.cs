using Newtonsoft.Json;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.ChangeRecordMetadataTests
{
    class ChangeRecordMetadataSerializationTests
    {
        [Test]
        public void should_be_able_to_deserialize_metdata_fields_to_comply_with_backward_compatibility()
        {
            // Please note that QuestionnaireChangeRecordMetadata stored as JSON in Designer DB,
            // So any changes to QuestionnaireChangeRecordMetadata should be backward compatible

            var json = "{\"Hq\":{\"HostName\":\"HostName\",\"UserName\":\"UserName\",\"Version\":\"Version\"," +
                "\"Build\":\"Build\",\"Comment\":\"Comment\",\"ImporterLogin\":\"ImporterLogin\"," +
                "\"QuestionnaireVersion\":42,\"TimeZoneMinutesOffset\":180.0},\"Comment\":\"Comment\"}";

            var metadata = JsonConvert.DeserializeObject<QuestionnaireChangeRecordMetadata>(json);

            Assert.That(metadata.Comment, Is.EqualTo("Comment"));
            Assert.That(metadata.Hq.Build, Is.EqualTo("Build"));
            Assert.That(metadata.Hq.Comment, Is.EqualTo("Comment"));
            Assert.That(metadata.Hq.HostName, Is.EqualTo("HostName"));
            Assert.That(metadata.Hq.ImporterLogin, Is.EqualTo("ImporterLogin"));
            Assert.That(metadata.Hq.QuestionnaireVersion, Is.EqualTo(42));
            Assert.That(metadata.Hq.TimeZoneMinutesOffset, Is.EqualTo(180.0));
            Assert.That(metadata.Hq.UserName, Is.EqualTo("UserName"));
            Assert.That(metadata.Hq.Version, Is.EqualTo("Version"));
        }
    }
}
