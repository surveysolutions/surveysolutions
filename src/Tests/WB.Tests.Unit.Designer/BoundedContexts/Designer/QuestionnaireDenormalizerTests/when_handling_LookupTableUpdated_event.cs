using System;

using Machine.Specifications;

using Main.Core.Documents;

using Moq;

using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire.LookupTables;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire.Macros;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class when_handling_LookupTableUpdated_event : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            evnt = Create.Event.LookupTableUpdated(questionnaireId, entityId, name, fileName);
            
            questionnaire = Create.QuestionnaireDocument(questionnaireId);
            
            var documentStorage = Mock.Of<IReadSideKeyValueStorage<QuestionnaireDocument>>(storage => storage.GetById(Moq.It.IsAny<string>()) == questionnaire);

            denormalizer = CreateQuestionnaireDenormalizer(documentStorage: documentStorage);
            denormalizer.Handle(Create.Event.MacroAdded(questionnaireId, entityId));
        };

        Because of = () => denormalizer.Handle(evnt);

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
        private static QuestionnaireDenormalizer denormalizer;
        private static QuestionnaireDocument questionnaire;
        private static IPublishedEvent<LookupTableUpdated> evnt;
    }
}