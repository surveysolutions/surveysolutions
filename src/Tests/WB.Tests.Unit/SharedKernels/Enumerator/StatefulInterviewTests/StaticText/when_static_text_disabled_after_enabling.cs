using System;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests.StaticText
{
    internal class when_static_text_disabled_after_enabling : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            staticTextIdentity = Create.Entity.Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), RosterVector.Empty);

            var plainQuestionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(
                questionnaire: Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                {
                    Create.Entity.StaticText(staticTextIdentity.Id)
                }));

            statefulInterview = Create.AggregateRoot.StatefulInterview(questionnaireRepository: plainQuestionnaireRepository);
            statefulInterview.Apply(Create.Event.StaticTextsEnabled(staticTextIdentity));
        };

        Because of = () => statefulInterview.Apply(Create.Event.StaticTextsDisabled(staticTextIdentity));

        It should_disable_it = () => statefulInterview.IsEnabled(staticTextIdentity).ShouldBeFalse();

        static StatefulInterview statefulInterview;
        static Identity staticTextIdentity;
    }
}