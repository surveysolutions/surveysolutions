using System;
using FluentAssertions;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests.StaticText
{
    internal class when_static_text_declated_valid: StatefulInterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            staticTextIdentity = Create.Entity.Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), RosterVector.Empty);

            var plainQuestionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(
                questionnaire: Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                {
                    Create.Entity.StaticText(staticTextIdentity.Id)
                }));
            statefulInterview = Create.AggregateRoot.StatefulInterview(questionnaireRepository: plainQuestionnaireRepository);
            statefulInterview.Apply(Create.Event.StaticTextsDeclaredInvalid(staticTextIdentity));
            statefulInterview.Apply(Create.Event.StaticTextsDeclaredValid(staticTextIdentity));
            BecauseOf();
        }

        private void BecauseOf() => isValid = statefulInterview.IsEntityValid(staticTextIdentity);

        [NUnit.Framework.Test] public void should_remember_validity_status () => isValid.Should().BeTrue();

        static StatefulInterview statefulInterview;
        static Identity staticTextIdentity;
        static bool isValid;
    }
}
