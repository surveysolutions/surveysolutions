using System;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.LookupTables;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.LookupTables
{
    internal class when_updating_lookup_table_which_doesnt_exists : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: responsibleId);

            updateLookupTable = Create.Command.UpdateLookupTable(questionnaireId, lookupTableId, responsibleId, lookupTableName);

            BecauseOf();
        }

        private void BecauseOf() =>
            questionnaire.UpdateLookupTable(updateLookupTable);

        [NUnit.Framework.Test] public void should_create_new_lookup_table () =>
            questionnaire.QuestionnaireDocument.LookupTables.SingleOrDefault(x => x.Key == lookupTableId).Value.TableName.ShouldEqual(lookupTableName);

        private static UpdateLookupTable updateLookupTable;
        private static Questionnaire questionnaire;
        private static readonly Guid responsibleId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid lookupTableId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly string lookupTableName = "lookuptbl";
    }
}