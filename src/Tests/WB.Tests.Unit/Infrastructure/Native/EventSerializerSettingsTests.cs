using Newtonsoft.Json;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Infrastructure.Native.Storage;

namespace WB.Tests.Unit.Infrastructure.Native
{
    public class EventSerializerSettingsTests
    {
        [Test]
        public void when_deserializing_event_without_origin_date_Should_keep_it_null()
        {
            var json = @"{
                        'answer':'2018-05-23T00:00:00',
                        'userId':'29c67b43-0525-41cb-b34a-75edbde8ac12',
                        'questionId':'a59480bd-9a2b-7ef2-a49b-a7ea6043b41e',
                        'rosterVector':[
                        0.0,
                        2.0
                            ],
                        'answerTimeUtc':'2018-05-23T12:15:24.607068Z'
                    }";

            // Act
            var deserializedValue =  JsonConvert.DeserializeObject<DateTimeQuestionAnswered>(json, 
                EventSerializerSettings.BackwardCompatibleJsonSerializerSettings);

            // Assert
            Assert.That(deserializedValue, Has.Property(nameof(deserializedValue.OriginDate)).Null);
        }
    }
}
