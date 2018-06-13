using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewEntities
{
    internal class TextAnswerTests
    {
        [Test]
        public void when_creating_answer_from_string_Should_remove_non_printable_chars()
        {
            var answer = TextAnswer.FromString("hi\u0000");
            Assert.That(answer.Value, Is.EqualTo("hi"));
        }
    }
}