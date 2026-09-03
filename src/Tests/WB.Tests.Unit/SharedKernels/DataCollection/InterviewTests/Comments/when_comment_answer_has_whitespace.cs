using System;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Comments
{
    public class when_comment_answer_has_whitespace
    {
        [Test]
        public void should_trim_trailing_and_leading_spaces_from_comment()
        {
            var command = new CommentAnswerCommand(
                interviewId: Guid.NewGuid(),
                userId: Guid.NewGuid(),
                questionId: Id.gA,
                rosterVector: RosterVector.Empty,
                comment: "  Thank you very much  ");

            Assert.That(command.Comment, Is.EqualTo("Thank you very much"));
        }
    }
}
