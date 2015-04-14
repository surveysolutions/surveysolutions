using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests;

namespace WB.Tests.Unit.BoundedContexts.Designer.CloneGroupTests
{
    internal class when_cloning_roster_group : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.NewGuid();

            rosterid = Guid.NewGuid();
            questionnaire = CreateQuestionnaireWithOneRosterGroup(rosterid, responsibleId);
            questionnaire.UpdateGroup(rosterid, responsibleId, "title", rosterVariableName, null, null, null, true, RosterSizeSourceType.FixedTitles, new []{new Tuple<string, string>("1", "one"), new Tuple<string, string>("2", "two")}, null);

            eventContext = new EventContext();
        };

        Because of = () => questionnaire.CloneGroup(Guid.NewGuid(), responsibleId, rosterid, 0);

        It should_not_clone_variable_name = () => eventContext.ShouldNotContainEvent<GroupCloned>(x => x.VariableName == rosterVariableName);

        static Questionnaire questionnaire;
        static Guid responsibleId;
        static EventContext eventContext;
        static Guid rosterid;
        static string rosterVariableName = "variable";
    }
}

