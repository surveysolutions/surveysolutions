extern alias datacollection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

using TemplateImported = datacollection::Main.Core.Events.Questionnaire.TemplateImported;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.ReferenceInfoForLinkedQuestionsDenormalizerTests
{
    internal class when_importing_questionnaire_and_version_is_set : ReferenceInfoForLinkedQuestionsDenormalizerTestContext
    {
        Establish context = () =>
        {
            var id = Guid.Parse("33332222111100000000111122223333");
            importeDocument = CreateQuestionnaireDocumentWithRosterAndNumericQuestionAndLinedQuestionAfter(id, rosterId, linkedId);
            referenceInfoForLinkedQuestionsWriter = new Mock<IReadSideKeyValueStorage<ReferenceInfoForLinkedQuestions>>();
            denormalizer = CreateReferenceInfoForLinkedQuestionsDenormalizer(referenceInfoForLinkedQuestionsWriter.Object);
            evnt = CreateTemplateImportedEvent(importeDocument,3);
        };

        Because of = () =>
            denormalizer.Handle(evnt);

        It should_create_one_row_with_mapping_of_linked_question_on_roster_scope = () =>
            referenceInfoForLinkedQuestionsWriter.Verify(s => s.Store(
                Moq.It.Is<ReferenceInfoForLinkedQuestions>(d => d.ReferencesOnLinkedQuestions.Count == 1 && d.Version==3),
                Moq.It.Is<string>(g => g == importeDocument.PublicKey.FormatGuid() + "$3")));

        private static Guid rosterId = Guid.Parse("11111111111111111111111111111111");
        private static Guid linkedId = Guid.Parse("22222222222222222222222222222222");
        private static QuestionnaireDocument importeDocument;
        private static ReferenceInfoForLinkedQuestionsDenormalizer denormalizer;
        private static IPublishedEvent<TemplateImported> evnt;
        private static Mock<IReadSideKeyValueStorage<ReferenceInfoForLinkedQuestions>> referenceInfoForLinkedQuestionsWriter;
    }
}
