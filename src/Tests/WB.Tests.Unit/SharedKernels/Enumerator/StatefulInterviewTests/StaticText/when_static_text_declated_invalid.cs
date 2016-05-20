using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests.StaticText
{
    internal class when_static_text_declated_invalid: StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            staticTextIdentity = Create.Other.Identity(Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB"), RosterVector.Empty);

            var questionnaireId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var questionnaire = Create.Other.PlainQuestionnaire(Create.Other.QuestionnaireDocument(questionnaireId,
                Create.Other.Group(children: new List<IComposite>()
                {
                    Create.Other.StaticText(staticTextIdentity.Id)
                })));

            var plainQuestionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);
            statefulInterview = Create.Other.StatefulInterview(questionnaireRepository: plainQuestionnaireRepository);
        };

        Because of = () => statefulInterview.Apply(Create.Event.StaticTextsDeclaredInvalid(staticTextIdentity));

        It should_remember_validity_status = () => statefulInterview.IsValid(staticTextIdentity).ShouldBeFalse();

        It should_return_failed_validation_index = () => statefulInterview.GetFailedValidationConditions(staticTextIdentity).ShouldNotBeEmpty();

        It should_count_it_in_total_invalid_entities = () => statefulInterview.CountInvalidEntitiesInInterview().ShouldEqual(1);

        static StatefulInterview statefulInterview;
        static Identity staticTextIdentity;
    }
}