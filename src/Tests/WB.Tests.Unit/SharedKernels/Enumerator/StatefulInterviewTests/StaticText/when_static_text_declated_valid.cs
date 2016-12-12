using System;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests.StaticText
{
    internal class when_static_text_declated_valid: StatefulInterviewTestsContext
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
            statefulInterview.Apply(Create.Event.StaticTextsDeclaredInvalid(staticTextIdentity));
            statefulInterview.Apply(Create.Event.StaticTextsDeclaredValid(staticTextIdentity));
        };

        Because of = () => isValid = statefulInterview.IsValid(staticTextIdentity);

        It should_remember_validity_status = () => isValid.ShouldBeTrue();

        static StatefulInterview statefulInterview;
        static Identity staticTextIdentity;
        static bool isValid;
    }
}