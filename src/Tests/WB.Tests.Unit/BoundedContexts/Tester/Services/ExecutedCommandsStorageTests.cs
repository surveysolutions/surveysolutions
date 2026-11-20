using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Tester.Services
{
    [TestFixture]
    [TestOf(typeof(ExecutedCommandsStorage))]
    public class ExecutedCommandsStorageTests
    {
        [Test]
        public void when_commands_are_added_concurrently_should_return_a_stable_snapshot()
        {
            var storage = new ExecutedCommandsStorage();
            var interviewId = Guid.NewGuid();
            Parallel.For(0, 100, i => storage.Add(interviewId,
                Create.Command.AnswerNumericIntegerQuestionCommand(interviewId: interviewId, answer: i)));

            var snapshot = storage.Get(interviewId);
            storage.Add(interviewId, Create.Command.AnswerNumericIntegerQuestionCommand(interviewId: interviewId));

            snapshot.Should().HaveCount(100);
            storage.Get(interviewId).Should().HaveCount(101);
        }
    }
}
