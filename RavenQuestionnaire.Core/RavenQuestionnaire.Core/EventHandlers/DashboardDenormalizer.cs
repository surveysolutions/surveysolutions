using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ncqrs.Eventing.ServiceModel.Bus;
using RavenQuestionnaire.Core.Commands.Questionnaire;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Commands.Questionnaire.Group;
using RavenQuestionnaire.Core.Denormalizers;
using RavenQuestionnaire.Core.Documents.Statistics;
using RavenQuestionnaire.Core.Events;
using RavenQuestionnaire.Core.Events.Questionnaire;
using RavenQuestionnaire.Core.Events.Questionnaire.Completed;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Grouped;

namespace RavenQuestionnaire.Core.EventHandlers
{
    public class DashboardDenormalizer : IEventHandler<NewCompleteQuestionnaireCreated>,
        IEventHandler<AnswerSet>,
        IEventHandler<PropagatableGroupAdded>,
        IEventHandler<PropagatableGroupDeleted>,
        IEventHandler<NewQuestionnaireCreated>
    {
        private IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemStore;
        private IDenormalizerStorage<CQGroupItem> documentGroupSession;
        public DashboardDenormalizer(IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemStore, IDenormalizerStorage<CQGroupItem> documentGroupSession)
        {
            this.documentItemStore = documentItemStore;
            this.documentGroupSession = documentGroupSession;
        }

        #region Implementation of IEventHandler<in CreateCompleteQuestionnaireCommand>

        public void Handle(IPublishedEvent<NewCompleteQuestionnaireCreated> evnt)
        {
            this.documentItemStore.Store(new CompleteQuestionnaireBrowseItem(
                                   evnt.Payload.CompletedQuestionnaireId.ToString(),evnt.Payload.QuestionnaireId.ToString(),
                                   "", DateTime.Now,
                                   DateTime.Now, null, 0, 0, null), evnt.Payload.CompletedQuestionnaireId);

          //  this.storage.Commit();
        }

        #endregion

        #region Implementation of IEventHandler<in SetAnswerCommand>

        public void Handle(IPublishedEvent<AnswerSet> evnt)
        {
            var items = this.documentItemStore.Query().Where(q => q.CompleteQuestionnaireId == evnt.Payload.CompletedQuestionnaireId.ToString());
            foreach (CompleteQuestionnaireBrowseItem item in items)
            {
                item.AnsweredQuestionCouont++;
            }
         
         //   this.storage.Store(item);
     //       this.storage.Commit();
        }

        #endregion

        #region Implementation of IEventHandler<in AddPropagatableGroupCommand>

        public void Handle(IPublishedEvent<PropagatableGroupAdded> evnt)
        {
          //  throw new NotImplementedException();
        }

        #endregion

        #region Implementation of IEventHandler<in DeletePropagatableGroupCommand>

        public void Handle(IPublishedEvent<PropagatableGroupDeleted> evnt)
        {
          //  throw new NotImplementedException();
        }

        #endregion

        #region Implementation of IEventHandler<in CreateQuestionnaireCommand>

        public void Handle(IPublishedEvent<NewQuestionnaireCreated> evnt)
        {
           /* var questionnaire = this.storage.GetByGuid<CQGroupItem>(evnt.Payload.PublicKey);
            if (questionnaire == null)
            {*/
                var questionnaire = new CQGroupItem(0, 100, 100, evnt.Payload.Title, evnt.Payload.PublicKey.ToString());
                this.documentGroupSession.Store(questionnaire, evnt.Payload.PublicKey);
           /* }
            grid.Groups.Add(new CQGroupItem(0, 100, 100, evnt.Payload.Title, evnt.Payload.PublicKey.ToString()));*/
        //    this.storage.Commit();
        }

        #endregion
    }
}
