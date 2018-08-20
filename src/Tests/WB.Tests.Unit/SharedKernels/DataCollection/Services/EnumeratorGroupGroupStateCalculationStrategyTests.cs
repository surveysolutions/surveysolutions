using System;
using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.Services
{
    [TestOf(typeof(EnumeratorGroupGroupStateCalculationStrategy))]
    public class EnumeratorGroupGroupStateCalculationStrategyTests
    {
        [Test]
        public void should_mark_group_as_started_when_there_is_one_answered_and_one_unanswered()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId: Id.gA,
                children: new IComposite[]
                {
                    Create.Entity.TextQuestion(Id.g1),
                    Create.Entity.TextQuestion(Id.g2)
                });

            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            
            // Act
            interview.AnswerTextQuestion(Id.gC, Id.g1, RosterVector.Empty, DateTimeOffset.UtcNow, "a");
            GroupStatus calculateDetailedStatus = new EnumeratorGroupGroupStateCalculationStrategy().CalculateDetailedStatus(Create.Identity(Id.gA), interview);

            // Assert
            Assert.That(calculateDetailedStatus, Is.EqualTo(GroupStatus.Started));
        }
    }
}
