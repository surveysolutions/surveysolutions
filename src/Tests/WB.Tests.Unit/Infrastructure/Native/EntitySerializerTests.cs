using Machine.Specifications;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Infrastructure.Native.Storage;

namespace WB.Tests.Unit.Infrastructure.Native
{
    [TestOf(typeof(EntitySerializer<>))]
    internal class EntitySerializerTests
    {
        [Test]
        public void should_be_able_to_deserialize_legacy_format_of_linked_questions()
        {
            var options = "{\"LinkedQuestionOptions\":{\"78e523db8474ec8c2c0078f75244c712_0\":[[0.0,1.0],[0.0,2.0]],\"33437136c888ca4b32d29fc7c1d105c5\":[[0.0,1.0],[0.0,2.0]]}}";
            var entitySerializer = new EntitySerializer<InterviewLinkedQuestionOptions>();

            var interviewLinkedQuestionOptions = entitySerializer.Deserialize(options);

            Assert.That(interviewLinkedQuestionOptions.LinkedQuestionOptions.Count, Is.EqualTo(2));
        }
    }
}

