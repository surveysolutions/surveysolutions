using System;
using System.Collections.Generic;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Tests.Unit.SharedKernels.DataCollection.ValidityChangesTests
{
    [Subject(typeof(ValidityChanges))]
    internal class when_creating_with_single_invalid_question
    {
        Establish context = () =>
        {
            invalidQuestions = new List<Identity> { Create.Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), RosterVector.Empty) };
        };

        Because of = () => validityChanges = new ValidityChanges(new List<Identity>(), invalidQuestions);

        It should_create_a_list_of_failed_conditions_with_single_item = () =>
        {
            validityChanges.FailedValidationConditions.Count.ShouldEqual(1);
            validityChanges.FailedValidationConditions[invalidQuestions[0]][0].FailedConditionIndex.ShouldEqual(0);
        };

        static List<Identity> invalidQuestions;
        static ValidityChanges validityChanges;
    }
}