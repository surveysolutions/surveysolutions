using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.DenormalizerStorage;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using It = Machine.Specifications.It;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    extern alias designer;

    internal class when_handling_TemplateImported_event : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            questionnaireDocument = CreateQuestionnaireDocument();
            questionnaireDocument.Macros.Add(macrosWithBefore, new Macro() { Description = "before" });

            documentStorage = new InMemoryReadSideRepositoryAccessor<QuestionnaireDocument>();
            documentStorage.Store(questionnaireDocument, questionnaireDocument.PublicKey);

            @event = Create.Event.TemplateImportedEvent(questionnaireId: questionnaireDocument.PublicKey.FormatGuid());
            @event.Source.Macros.Add(Guid.NewGuid(), new Macro() { Description = "event"});

            denormalizer = CreateQuestionnaireDenormalizer(questionnaire: questionnaireDocument, sharedPersons: macrosWithBefore.ToEnumerable());
        };

        Because of = () =>
            denormalizer.ImportTemplate(@event);

        It should_list_of_macroses_contains_macros_from_replaced_questionnaire_only = () =>
           documentStorage.GetById(questionnaireDocument.PublicKey).Macros.Keys.ShouldContainOnly(macrosWithBefore);

        private static Questionnaire denormalizer;
        private static QuestionnaireDocument questionnaireDocument;
        private static TemplateImported @event;
        private static Guid macrosWithBefore = Guid.Parse("11111111111111111111111111111111");
        private static InMemoryReadSideRepositoryAccessor<QuestionnaireDocument> documentStorage;
    }
}
