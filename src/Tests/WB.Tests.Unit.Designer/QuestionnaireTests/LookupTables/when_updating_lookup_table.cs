using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.LookupTables;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.LookupTables
{
    internal class when_updating_lookup_table : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: responsibleId);
            questionnaire.AddLookupTable(Create.Command.AddLookupTable(questionnaireId, oldLookupTableId, responsibleId));

            updateLookupTable = Create.Command.UpdateLookupTable(questionnaireId, lookupTableId, responsibleId, "newtable", oldLookupTableId);
            BecauseOf();
        }

        private void BecauseOf() => questionnaire.UpdateLookupTable(updateLookupTable);

        [NUnit.Framework.Test] public void should_contain_one_lookuptable () =>
            questionnaire.QuestionnaireDocument.LookupTables.Count.ShouldEqual(1);

        [NUnit.Framework.Test] public void should_contain_lookuptable_with_EntityId_specified () =>
            questionnaire.QuestionnaireDocument.LookupTables.ShouldContain(t => t.Key == lookupTableId);

        [NUnit.Framework.Test] public void should_contain_lookuptable_with_tablename_specified () =>
            questionnaire.QuestionnaireDocument.LookupTables.ShouldContain(t => t.Key == lookupTableId && t.Value.TableName == "newtable");

        private static UpdateLookupTable updateLookupTable;
        private static Questionnaire questionnaire;
        private static readonly Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid lookupTableId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly Guid oldLookupTableId = Guid.Parse("1AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }
}
