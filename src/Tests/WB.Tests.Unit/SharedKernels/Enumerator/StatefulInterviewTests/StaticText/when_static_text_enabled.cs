using System;
using FluentAssertions;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests.StaticText
{
    internal class when_static_text_enabled : StatefulInterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            staticTextIdentity = Create.Entity.Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), RosterVector.Empty);

            var plainQuestionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(
                questionnaire: Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                {
                    Create.Entity.StaticText(staticTextIdentity.Id)
                }));

            statefulInterview = Create.AggregateRoot.StatefulInterview(questionnaireRepository: plainQuestionnaireRepository);

            BecauseOf();
        }

        private void BecauseOf() => statefulInterview.Apply(Create.Event.StaticTextsEnabled(staticTexts:staticTextIdentity));

        [NUnit.Framework.Test] public void should_enable_it () => statefulInterview.IsEnabled(staticTextIdentity).Should().BeTrue();

        static StatefulInterview statefulInterview;
        static Identity staticTextIdentity;
    }
}
