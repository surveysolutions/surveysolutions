using System;
using System.Collections.Generic;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Views.Interview.Overview;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.OverviewTests
{
    [TestOf(typeof(OverviewStaticText))]
    public class OverviewStaticTextTests
    {
        private StatefulInterview interview;
        private readonly Guid staticTextId = Id.g1;

        [SetUp]
        public void Setup()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.StaticText(publicKey: staticTextId, validationConditions: new List<ValidationCondition>()
                {
                    Create.Entity.ValidationCondition()
                })
            );

            interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
        }

        [Test]
        public void should_set_state_to_answered()
        {
            var staticText = new OverviewStaticText(interview.GetStaticText(Create.Identity(staticTextId)), interview);

            Assert.That(staticText, Has.Property(nameof(staticText.State)).EqualTo(OverviewNodeState.Answered));
        }

        [Test]
        public void should_set_state_to_invalid_when_static_text_invalid()
        {
            var identity = Create.Identity(staticTextId);
            interview.ApplyEvent(Create.Event.StaticTextsDeclaredInvalid(identity));

            var staticText = new OverviewStaticText(interview.GetStaticText(identity), interview);

            Assert.That(staticText, Has.Property(nameof(staticText.State)).EqualTo(OverviewNodeState.Invalid));
        }
    }
}
