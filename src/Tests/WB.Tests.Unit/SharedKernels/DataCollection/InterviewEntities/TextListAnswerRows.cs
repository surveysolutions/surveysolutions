using System;
using System.Linq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewEntities
{
    internal class TextListAnswerTests
    {
        [Test]
        public void should_remove_control_characters_from_strings()
        {
            var answer = TextListAnswer.FromTupleArray(new []
            {
                Tuple.Create(1m, "hi\u0000")
            });
            Assert.That(answer.Rows.First().Text, Is.EqualTo("hi"));
        }
    }
}