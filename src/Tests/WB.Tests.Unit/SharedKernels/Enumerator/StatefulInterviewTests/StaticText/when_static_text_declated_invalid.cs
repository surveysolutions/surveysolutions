using System;
using System.Collections.Generic;
using FluentAssertions;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests.StaticText
{
    internal class when_static_text_declated_invalid: StatefulInterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            staticTextIdentity = Create.Entity.Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), RosterVector.Empty);

            var questionnaireId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var plainQuestionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(
                questionnaire: Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                {
                    Create.Entity.StaticText(staticTextIdentity.Id,
                        validationConditions:
                            new List<Core.SharedKernels.QuestionnaireEntities.ValidationCondition>()
                            {
                                new ValidationCondition("1=1", "invalid")
                            })
                }));
            statefulInterview = Create.AggregateRoot.StatefulInterview(questionnaireRepository: plainQuestionnaireRepository);
            BecauseOf();
        }

        private void BecauseOf() => statefulInterview.Apply(Create.Event.StaticTextsDeclaredInvalid(staticTextIdentity));

        [NUnit.Framework.Test] public void should_remember_validity_status () => statefulInterview.IsEntityValid(staticTextIdentity).Should().BeFalse();

        [NUnit.Framework.Test] public void should_return_failed_validation_index () => statefulInterview.GetFailedValidationMessages(staticTextIdentity, "Error").Should().NotBeEmpty();

        [NUnit.Framework.Test] public void should_count_it_in_total_invalid_entities () => statefulInterview.CountInvalidEntitiesInInterview().Should().Be(1);

        static StatefulInterview statefulInterview;
        static Identity staticTextIdentity;
    }
}
