﻿using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.EventHandler.WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using QuestionnaireDeleted = WB.Core.SharedKernels.DataCollection.Events.Questionnaire.QuestionnaireDeleted;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionnaireQuestionsInfoDenormalizerTests
{
    internal class when_questionnaire_deleted_event_recived : QuestionnaireQuestionsInfoDenormalizerTestContext
    {
        Establish context = () =>
        {
            questionnaireQuestionsInfoWriter = new Mock<IReadSideKeyValueStorage<QuestionnaireQuestionsInfo>>();
            denormalizer = CreateQuestionnaireQuestionsInfoDenormalizer(questionnaireQuestionsInfoWriter.Object);
            evnt = CreateQuestionnaireDeletedEvent(questionnaireId, 2);
        };

        Because of = () =>
            denormalizer.Handle(evnt);

        It should_view_with_id_equal_to_combination_of_questionnaireid_and_version_be_removed = () =>
            questionnaireQuestionsInfoWriter.Verify(x => x.Remove(RepositoryKeysHelper.GetVersionedKey(evnt.EventSourceId, evnt.Payload.QuestionnaireVersion)));

        static Guid questionnaireId = Guid.Parse("33332222111100000000111122223333");
        static QuestionnaireQuestionsInfoDenormalizer denormalizer;
        static IPublishedEvent<QuestionnaireDeleted> evnt;
        static Mock<IReadSideKeyValueStorage<QuestionnaireQuestionsInfo>> questionnaireQuestionsInfoWriter;
    }
}
