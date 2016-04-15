using System;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests.StaticText
{
    internal class when_static_text_declated_invalid: StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            staticTextIdentity = Create.Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), RosterVector.Empty);

            var plainQuestionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(
                Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"));
            statefulInterview = Create.StatefulInterview(questionnaireRepository: plainQuestionnaireRepository);
           
        };

        Because of = () => statefulInterview.Apply(Create.Event.StaticTextsDeclaredInvalid(staticTextIdentity));

        It should_remember_validity_status = () => statefulInterview.IsValid(staticTextIdentity).ShouldBeFalse();

        It should_return_failed_validation_index = () => statefulInterview.GetFailedValidationConditions(staticTextIdentity).ShouldNotBeEmpty();

        static StatefulInterview statefulInterview;
        static Identity staticTextIdentity;
    }
}