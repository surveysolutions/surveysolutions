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
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.EventHandlers.QuestionnaireQuestionsInfoDenormalizerTests
{
    internal class when_questionnaire_deleted_event_recived : QuestionnaireQuestionsInfoDenormalizerTestContext
    {
        Establish context = () =>
        {
            questionnaireQuestionsInfoWriter = new Mock<IReadSideRepositoryWriter<QuestionnaireQuestionsInfo>>();
            denormalizer = CreateQuestionnaireQuestionsInfoDenormalizer(questionnaireQuestionsInfoWriter.Object);
            evnt = CreateQuestionnaireDeletedEvent(questionnaireId,2);
        };

        Because of = () =>
            denormalizer.Handle(evnt);

        It should_create_one_view_with_mapping_of_question_id_on_variable_name = () =>
            questionnaireQuestionsInfoWriter.Verify(x => x.Remove(RepositoryKeysHelper.GetVersionedKey(evnt.EventSourceId, evnt.Payload.QuestionnaireVersion)));

        private static Guid questionnaireId = Guid.Parse("33332222111100000000111122223333");
        private static QuestionnaireQuestionsInfoDenormalizer denormalizer;
        private static IPublishedEvent<QuestionnaireDeleted> evnt;
        private static Mock<IReadSideRepositoryWriter<QuestionnaireQuestionsInfo>> questionnaireQuestionsInfoWriter;
    }
}
