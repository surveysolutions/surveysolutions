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
            staticTextIdentity = Create.Entity.Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), RosterVector.Empty);

            var plainQuestionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(
                questionnaire: Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                {
                    Create.Entity.StaticText(staticTextIdentity.Id)
                }));

            statefulInterview = Create.AggregateRoot.StatefulInterview(questionnaireRepository: plainQuestionnaireRepository);
        };

        Because of = () => statefulInterview.Apply(Create.Event.StaticTextsEnabled(staticTextIdentity));

        It should_enable_it = () => statefulInterview.IsEnabled(staticTextIdentity).ShouldBeTrue();

        static StatefulInterview statefulInterview;
        static Identity staticTextIdentity;
    }
}