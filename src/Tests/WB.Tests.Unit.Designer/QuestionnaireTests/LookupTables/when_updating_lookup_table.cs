using System;
using System.Linq;
using FluentAssertions;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.LookupTables;
using WB.Core.GenericSubdomains.Portable;

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
            questionnaire.QuestionnaireDocument.LookupTables.Count.Should().Be(1);

        [NUnit.Framework.Test] public void should_contain_lookuptable_with_EntityId_specified () =>
            questionnaire.QuestionnaireDocument.LookupTables.Keys.Should().Contain(lookupTableId);

        [NUnit.Framework.Test] public void should_contain_lookuptable_with_tablename_specified () =>
            questionnaire.QuestionnaireDocument.LookupTables.SingleOrDefault(t => t.Key == lookupTableId && t.Value.TableName == "newtable").Should().NotBeNull();

        private static UpdateLookupTable updateLookupTable;
        private static Questionnaire questionnaire;
        private static readonly Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid lookupTableId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly Guid oldLookupTableId = Guid.Parse("1AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }
}
