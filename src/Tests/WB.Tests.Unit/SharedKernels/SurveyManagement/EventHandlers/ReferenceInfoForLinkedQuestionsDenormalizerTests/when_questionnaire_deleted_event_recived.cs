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
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

using QuestionnaireDeleted = WB.Core.SharedKernels.DataCollection.Events.Questionnaire.QuestionnaireDeleted;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.ReferenceInfoForLinkedQuestionsDenormalizerTests
{
    internal class when_questionnaire_deleted_event_recived : ReferenceInfoForLinkedQuestionsDenormalizerTestContext
    {
        Establish context = () =>
        {
            questionnaireId = Guid.Parse("33332222111100000000111122223333");
            referenceInfoForLinkedQuestionsWriter = new Mock<IReadSideKeyValueStorage<ReferenceInfoForLinkedQuestions>>();
            denormalizer = CreateReferenceInfoForLinkedQuestionsDenormalizer(referenceInfoForLinkedQuestionsWriter.Object);
            evnt = CreateQuestionnaireDeletedEvent(questionnaireId,2);
        };

        Because of = () =>
            denormalizer.Handle(evnt);

        It should_remove_view_from_read_side = () =>
            referenceInfoForLinkedQuestionsWriter.Verify(s => s.Remove(questionnaireId.FormatGuid() + "$" + 2), Times.Once);

        private static Guid rosterId = Guid.Parse("11111111111111111111111111111111");
        private static Guid linkedId = Guid.Parse("22222222222222222222222222222222");
        private static ReferenceInfoForLinkedQuestionsDenormalizer denormalizer;
        private static IPublishedEvent<QuestionnaireDeleted> evnt;
        private static Mock<IReadSideKeyValueStorage<ReferenceInfoForLinkedQuestions>> referenceInfoForLinkedQuestionsWriter;
        private static Guid questionnaireId;
    }
}
