using System;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Main.Core.Events.Questionnaire.Completed;
using Main.Core.Events.User;
using Ncqrs.Eventing.ServiceModel.Bus;

namespace CapiDataGenerator
{
    public class CustomInProcessEventBus : InProcessEventBus
    {
        public CustomInProcessEventBus(bool useTransactionScope)
            : base(useTransactionScope) {}

        protected override Action<PublishedEvent> DoActionForHandler<TEvent>(IEventHandler<TEvent> handler)
        {
            return evnt =>
            {
                bool isCapiHandler = handler.GetType().ToString().Contains("CAPI");

                Func<object, bool> isSupervisorEvent =
                (o) => o is NewUserCreated || o is NewCompleteQuestionnaireCreated ||
                       o is TemplateImported || o is QuestionnaireAssignmentChanged ||
                       (o is QuestionnaireStatusChanged &&
                        (((QuestionnaireStatusChanged)o).Status.PublicId == SurveyStatus.Unassign.PublicId ||
                         ((QuestionnaireStatusChanged)o).Status.PublicId == SurveyStatus.Initial.PublicId));

                Func<object, bool> isCapiEvent =
                    (o) =>
                        !(o is NewCompleteQuestionnaireCreated || o is TemplateImported ||
                          o is QuestionnaireAssignmentChanged ||
                          (o is QuestionnaireStatusChanged &&
                           (((QuestionnaireStatusChanged)o).Status.PublicId == SurveyStatus.Unassign.PublicId ||
                            ((QuestionnaireStatusChanged)o).Status.PublicId == SurveyStatus.Initial.PublicId)));

                if ((isSupervisorEvent(evnt.Payload) && !isCapiHandler) ||
                    (isCapiEvent(evnt.Payload) && isCapiHandler))
                {
                    handler.Handle((IPublishedEvent<TEvent>) evnt);
                }
            };
        }
    }
}