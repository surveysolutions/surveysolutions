using System;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Comments
{
    public class when_comment_answer_has_whitespace
    {
        [Test]
        public void should_trim_trailing_and_leading_spaces_from_comment()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(children: Create.Entity.TextQuestion(Id.gA));
            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);

            using (var eventContext = new EventContext())
            {
                interview.CommentAnswer(Guid.NewGuid(), Id.gA, RosterVector.Empty, DateTimeOffset.UtcNow, "  Thank you very much  ");

                var comment = eventContext.GetEvent<AnswerCommented>().Comment;
                Assert.That(comment, Is.EqualTo("Thank you very much"));
            }
        }
    }
}
