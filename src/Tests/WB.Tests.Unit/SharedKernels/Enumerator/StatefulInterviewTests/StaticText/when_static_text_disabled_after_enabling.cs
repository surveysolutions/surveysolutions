using System;
using FluentAssertions;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests.StaticText
{
    internal class when_static_text_disabled_after_enabling : StatefulInterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            staticTextIdentity = Create.Entity.Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), RosterVector.Empty);

            var plainQuestionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(
                questionnaire: Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                {
                    Create.Entity.StaticText(staticTextIdentity.Id)
                }));

            statefulInterview = Create.AggregateRoot.StatefulInterview(questionnaireRepository: plainQuestionnaireRepository);
            statefulInterview.Apply(Create.Event.StaticTextsEnabled(staticTextIdentity));

            BecauseOf();
        }

        private void BecauseOf() => statefulInterview.Apply(Create.Event.StaticTextsDisabled(staticTextIdentity));

        [NUnit.Framework.Test] public void should_disable_it () => statefulInterview.IsEnabled(staticTextIdentity).Should().BeFalse();

        static StatefulInterview statefulInterview;
        static Identity staticTextIdentity;
    }
}
