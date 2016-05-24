using System;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests.StaticText
{
    internal class when_static_text_enabled : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            var plainQuestionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(
                Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"));
            statefulInterview = Create.Other.StatefulInterview(questionnaireRepository: plainQuestionnaireRepository);

            staticTextIdentity = Create.Entity.Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), RosterVector.Empty);
        };

        Because of = () => statefulInterview.Apply(Create.Event.StaticTextsEnabled(staticTextIdentity));

        It should_enable_it = () => statefulInterview.IsEnabled(staticTextIdentity).ShouldBeTrue();

        static StatefulInterview statefulInterview;
        static Identity staticTextIdentity;
    }
}