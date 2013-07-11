using System;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.UI.Designer.Views.Questionnaire.Pdf;

namespace WB.UI.Designer.Views.EventHandler
{
    public class PdfQuestionnaireDenormalizer : 
        IEventHandler<GroupCloned>,
        IEventHandler<GroupDeleted>,
        IEventHandler<GroupUpdated>,
        IEventHandler<NewGroupAdded>,
        IEventHandler<NewQuestionAdded>,
        IEventHandler<NewQuestionnaireCreated>,
        IEventHandler<QuestionChanged>,
        IEventHandler<QuestionCloned>,
        IEventHandler<QuestionDeleted>,
        IEventHandler<QuestionnaireDeleted>,
        IEventHandler<QuestionnaireItemMoved>,
        IEventHandler<QuestionnaireUpdated>,
        IEventHandler<TemplateImported>
    {
        private readonly IReadSideRepositoryWriter<PdfQuestionnaireView> repositoryWriter;
        private readonly ILogger logger;

        public PdfQuestionnaireDenormalizer(IReadSideRepositoryWriter<PdfQuestionnaireView> repositoryWriter, ILogger logger)
        {
            this.repositoryWriter = repositoryWriter;
            this.logger = logger;
        }

        private void HandleUpdateEvent<TEvent>(IPublishedEvent<TEvent> evnt, Func<TEvent, PdfQuestionnaireView, PdfQuestionnaireView> handle)
        {
            try
            {
                Guid questionnaireId = evnt.EventSourceId;
                PdfQuestionnaireView initialQuestionnaire = this.repositoryWriter.GetById(questionnaireId);

                PdfQuestionnaireView updatedQuestionnaire = handle(evnt.Payload, initialQuestionnaire);

                this.repositoryWriter.Store(updatedQuestionnaire, questionnaireId);
            }
            catch (Exception e)
            {
                this.logger.Error("", e); // TODO 
            }
        }

        public void Handle(IPublishedEvent<GroupCloned> evnt)
        {
            
        }

        public void Handle(IPublishedEvent<GroupDeleted> evnt)
        {
            HandleUpdateEvent(evnt, handle: (@event, questionnaire) => 
            {
                questionnaire.RemoveGroup(@event.GroupPublicKey);
                return questionnaire;
            });
        }

        public void Handle(IPublishedEvent<GroupUpdated> evnt)
        {
            HandleUpdateEvent(evnt, handle: (@event, questionnaire) =>
            {
                PdfGroupView @group = questionnaire.GetGroup(@event.GroupPublicKey);
                @group.Title = @event.GroupText;

                return questionnaire;
            });
        }

        public void Handle(IPublishedEvent<NewGroupAdded> evnt)
        {
            HandleUpdateEvent(evnt, handle: (@event, questionnaire) =>
            {
                var newGroup = new PdfGroupView
                    {
                        Title = @event.GroupText,
                        Id = @event.PublicKey,
                        Depth = questionnaire.GetEntityDepth(@event.ParentGroupPublicKey) + 1
                    };
                questionnaire.AddGroup(newGroup, @event.ParentGroupPublicKey);

                return questionnaire;
            });
        }

        public void Handle(IPublishedEvent<NewQuestionAdded> evnt)
        {
            
        }

        public void Handle(IPublishedEvent<NewQuestionnaireCreated> evnt)
        {
            
        }

        public void Handle(IPublishedEvent<QuestionChanged> evnt)
        {
            
        }

        public void Handle(IPublishedEvent<QuestionCloned> evnt)
        {
            
        }

        public void Handle(IPublishedEvent<QuestionDeleted> evnt)
        {
            
        }

        public void Handle(IPublishedEvent<QuestionnaireDeleted> evnt)
        {
            
        }

        public void Handle(IPublishedEvent<QuestionnaireItemMoved> evnt)
        {
            
        }

        public void Handle(IPublishedEvent<QuestionnaireUpdated> evnt)
        {
            
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            
        }
    }
}