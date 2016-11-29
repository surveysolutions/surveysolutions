using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests.StaticText
{
    internal class when_static_text_declated_invalid: StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
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
        };

        Because of = () => statefulInterview.Apply(Create.Event.StaticTextsDeclaredInvalid(staticTextIdentity));

        It should_remember_validity_status = () => statefulInterview.IsValid(staticTextIdentity).ShouldBeFalse();

        It should_return_failed_validation_index = () => statefulInterview.GetFailedValidationMessages(staticTextIdentity).ShouldNotBeEmpty();

        It should_count_it_in_total_invalid_entities = () => statefulInterview.CountVisibleInvalidEntitiesInInterview().ShouldEqual(1);

        static StatefulInterview statefulInterview;
        static Identity staticTextIdentity;
    }
}