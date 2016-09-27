using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainQuestionnaireTests
{
    internal class when_getting_roster_structures_for_old_multi_options : PlainQuestionnaireTestsContext
    {
        Establish context = () =>
        {
            var rosterTitleQuestion = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var triggerId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            Guid rosterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(id: triggerId),
                Create.Entity.Roster(rosterId: rosterId,
                    rosterSizeSourceType: RosterSizeSourceType.Question,
                    rosterSizeQuestionId: triggerId,
                    rosterTitleQuestionId: rosterTitleQuestion,
                    children: new IComposite[]
                    {
                        Create.Entity.MultyOptionsQuestion(id: rosterTitleQuestion,
                        options: new List<Answer>
                        {
                            Create.Entity.Answer("one", 1)
                        })
                    }));

            plainQuestionnaire = Create.Entity.PlainQuestionnaire(questionnaire);
        };

        Because of = () => { rosterStructures = plainQuestionnaire.GetRosterScopes(); };

        It should_build_some_roster_scopes = () => rosterStructures.Count.ShouldEqual(1);

        static PlainQuestionnaire plainQuestionnaire;
        static Dictionary<ValueVector<Guid>, RosterScopeDescription> rosterStructures;
    }
}