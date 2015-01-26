extern alias datacollection;

using System;
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
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

using TemplateImported = datacollection::Main.Core.Events.Questionnaire.TemplateImported;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.ReferenceInfoForLinkedQuestionsDenormalizerTests
{

    internal class when_importing_questionnaire_with_fixed_roster_with_numeric_question_and_linked_question_after_it : ReferenceInfoForLinkedQuestionsDenormalizerTestContext
    {
        Establish context = () =>
        {
            var id = Guid.Parse("33332222111100000000111122223333");
            importeDocument = CreateQuestionnaireDocumentWithRosterAndNumericQuestionAndLinedQuestionAfter(id, rosterId, linkedId);
            referenceInfoForLinkedQuestionsWriter = new Mock<IReadSideKeyValueStorage<ReferenceInfoForLinkedQuestions>>();
            denormalizer = CreateReferenceInfoForLinkedQuestionsDenormalizer(referenceInfoForLinkedQuestionsWriter.Object);
            evnt = CreateTemplateImportedEvent(importeDocument);
        };

        Because of = () =>
            denormalizer.Handle(evnt);

        It should_create_one_row_with_mapping_of_linked_question_on_roster_scope = () =>
            referenceInfoForLinkedQuestionsWriter.Verify(s => s.Store(
                it.Is<ReferenceInfoForLinkedQuestions>(d => d.ReferencesOnLinkedQuestions.Count == 1),
                it.Is<string>(g => g == importeDocument.PublicKey.FormatGuid() + "$0")));

        It should_put_linkedId_as_key_and_value_should_contains_rosterId_in__ScopeId__field_of_created_row = () =>
            referenceInfoForLinkedQuestionsWriter.Verify(s => s.Store(
                it.Is<ReferenceInfoForLinkedQuestions>(d => 
                    d.ReferencesOnLinkedQuestions.ContainsKey(linkedId) && d.ReferencesOnLinkedQuestions[linkedId].ReferencedQuestionRosterScope.Last() == (rosterId)),
                it.IsAny<string>()));

        private static Guid rosterId = Guid.Parse("11111111111111111111111111111111");
        private static Guid linkedId = Guid.Parse("22222222222222222222222222222222");
        private static QuestionnaireDocument importeDocument;
        private static ReferenceInfoForLinkedQuestionsDenormalizer denormalizer;
        private static IPublishedEvent<TemplateImported> evnt;
        private static Mock<IReadSideKeyValueStorage<ReferenceInfoForLinkedQuestions>> referenceInfoForLinkedQuestionsWriter;
    }
}
