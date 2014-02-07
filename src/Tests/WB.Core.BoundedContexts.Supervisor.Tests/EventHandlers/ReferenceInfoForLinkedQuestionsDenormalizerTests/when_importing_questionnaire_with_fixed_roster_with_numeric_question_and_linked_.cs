﻿using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Supervisor.EventHandler;
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Core.BoundedContexts.Supervisor.Tests.EventHandlers.ReferenceInfoForLinkedQuestionsDenormalizerTests
{

    internal class when_importing_questionnaire_with_fixed_roster_with_numeric_question_and_linked_question_after_it : ReferenceInfoForLinkedQuestionsDenormalizerTestContext
    {
        Establish context = () =>
        {
            var id = Guid.Parse("33332222111100000000111122223333");
            importeDocument = CreateQuestionnaireDocumentWithRosterAndNumericQuestionAndLinedQuestionAfter(id, rosterId, linkedId);
            referenceInfoForLinkedQuestionsWriter = new Mock<IVersionedReadSideRepositoryWriter<ReferenceInfoForLinkedQuestions>>();
            denormalizer = CreateReferenceInfoForLinkedQuestionsDenormalizer(referenceInfoForLinkedQuestionsWriter.Object);
            evnt = CreateTemplateImportedEvent(importeDocument);
        };

        Because of = () =>
            denormalizer.Handle(evnt);

        It should_create_one_row_with_mapping_of_linked_question_on_roster_scope = () =>
            referenceInfoForLinkedQuestionsWriter.Verify(s => s.Store(
                it.Is<ReferenceInfoForLinkedQuestions>(d => d.ReferencesOnLinkedQuestions.Count == 1),
                it.Is<string>(g => g == importeDocument.PublicKey.ToString())));

        It should_put_linkedId_as_key_and_value_should_contains_rosterId_in__ScopeId__field_of_created_row = () =>
            referenceInfoForLinkedQuestionsWriter.Verify(s => s.Store(
                it.Is<ReferenceInfoForLinkedQuestions>(d => 
                    d.ReferencesOnLinkedQuestions.ContainsKey(linkedId) && d.ReferencesOnLinkedQuestions[linkedId].ScopeId == (rosterId)),
                it.IsAny<string>()));

        private static Guid rosterId = Guid.Parse("11111111111111111111111111111111");
        private static Guid linkedId = Guid.Parse("22222222222222222222222222222222");
        private static QuestionnaireDocument importeDocument;
        private static ReferenceInfoForLinkedQuestionsDenormalizer denormalizer;
        private static IPublishedEvent<TemplateImported> evnt;
        private static Mock<IVersionedReadSideRepositoryWriter<ReferenceInfoForLinkedQuestions>> referenceInfoForLinkedQuestionsWriter;
    }
}
