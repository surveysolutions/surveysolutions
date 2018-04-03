using System;
using System.Collections.Generic;
using FluentAssertions;
using WB.Core.SharedKernels.DataCollection;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.ValidityChangesTests
{
    internal class when_creating_with_single_invalid_question
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            invalidQuestions = new List<Identity> { Create.Entity.Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), RosterVector.Empty) };
            BecauseOf();
        }

        public void BecauseOf() => validityChanges = new ValidityChanges(new List<Identity>(), invalidQuestions);

        [NUnit.Framework.Test] public void should_create_a_list_of_failed_conditions_with_single_item () 
        {
            validityChanges.FailedValidationConditions.Count.Should().Be(1);
            validityChanges.FailedValidationConditions[invalidQuestions[0]][0].FailedConditionIndex.Should().Be(0);
        }

        static List<Identity> invalidQuestions;
        static ValidityChanges validityChanges;
    }
}
