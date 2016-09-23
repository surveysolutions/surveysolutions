using System;
using Machine.Specifications;
using Main.Core.Documents;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto.LookupTables;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class when_handling_LookupTableUpdated_event : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            evnt = Create.Event.LookupTableUpdated(questionnaireId, entityId, name, fileName);
            
            questionnaire = Create.QuestionnaireDocument(questionnaireId);
            
            denormalizer = CreateQuestionnaireDenormalizer(questionnaire: questionnaire);
            denormalizer.AddMacro(Create.Event.MacroAdded(questionnaireId, entityId));
        };

        Because of = () => denormalizer.UpdateLookupTable(evnt);

        It should_not_add_extra_lookup_table = () =>
            questionnaire.LookupTables.Count.ShouldEqual(1);

        It should_update_lookup_table_with_specified_name = () =>
            questionnaire.LookupTables[entityId].TableName.ShouldEqual(name);

        It should_update_lookup_table_with_specifiedy_fileName = () =>
            questionnaire.LookupTables[entityId].FileName.ShouldEqual(fileName);

        private static Guid questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid entityId = Guid.Parse("11111111111111111111111111111111");
        private static readonly string name = "macros";
        private static readonly string fileName = "macros fileName";
        private static Questionnaire denormalizer;
        private static QuestionnaireDocument questionnaire;
        private static LookupTableUpdated evnt;
    }
}