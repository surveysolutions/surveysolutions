using System;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    class when_completing_interview_with_invalid_static_text : StatefulInterviewTestsContext
    {
        [Test]
        public void should_declare_interview_as_invalid()
        {
            Identity invalidQuestionIdentity = Id.Identity1;
            Identity invalidStaticTextdentity = Id.Identity2;

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextQuestion(questionId: invalidQuestionIdentity.Id),
                Create.Entity.StaticText(invalidStaticTextdentity.Id),
            });

            var interview = SetUp.StatefulInterview(questionnaireDocument: questionnaire);
            interview.Apply(Create.Event.InterviewStatusChanged(status: InterviewStatus.InterviewerAssigned));
            interview.Apply(Create.Event.StaticTextsDeclaredInvalid(invalidStaticTextdentity));

            using (var eventContext = new EventContext())
            {
                // Act
                interview.Complete(Guid.NewGuid(), string.Empty, DateTime.UtcNow, null);

                // Assert
                eventContext.ShouldContainEvent<InterviewDeclaredInvalid>();
                eventContext.ShouldNotContainEvent<InterviewDeclaredValid>();
            }
        }
    }
}
