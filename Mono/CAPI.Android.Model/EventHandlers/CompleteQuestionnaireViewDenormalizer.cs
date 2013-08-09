using System;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using Main.Core.Events.Questionnaire.Completed;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace CAPI.Android.Core.Model.EventHandlers
{
    public class CompleteQuestionnaireViewDenormalizer : IEventHandler<NewAssigmentCreated>, 
                                                         IEventHandler<ConditionalStatusChanged>, 
                                                         IEventHandler<CommentSet>,
                                                         IEventHandler<AnswerSet>,
                                                         IEventHandler<PropagatableGroupAdded>,
                                                         IEventHandler<PropagatableGroupDeleted>,
                                                         IEventHandler<QuestionnaireStatusChanged>
    {
        /// <summary>
        /// The _document storage.
        /// </summary>
        private readonly IReadSideRepositoryWriter<CompleteQuestionnaireView> _documentStorage;


        public CompleteQuestionnaireViewDenormalizer(IReadSideRepositoryWriter<CompleteQuestionnaireView> documentStorage)
        {
            _documentStorage = documentStorage;
        }
        #region Implementation of IEventHandler<in NewCompleteQuestionnaireCreated>

        public void Handle(IPublishedEvent<NewAssigmentCreated> evnt)
        {
            var document = evnt.Payload.Source;
            
            var view = new CompleteQuestionnaireView(document);

            _documentStorage.Store(view, document.PublicKey);
        }


        #endregion

        #region Implementation of IEventHandler<in AnswerSet>

        public void Handle(IPublishedEvent<AnswerSet> evnt)
        {
            var doc = GetStoredObject(evnt.EventSourceId);
            doc.SetAnswer(new ItemPublicKey(evnt.Payload.QuestionPublicKey, evnt.Payload.PropogationPublicKey),
                          evnt.Payload.AnswerKeys, evnt.Payload.AnswerString);
        }

        #endregion

        #region Implementation of IEventHandler<in CommentSeted>

        public void Handle(IPublishedEvent<CommentSet> evnt)
        {
            var doc = GetStoredObject(evnt.EventSourceId);
            doc.SetComment(new ItemPublicKey(evnt.Payload.QuestionPublickey, evnt.Payload.PropagationPublicKey),
                           evnt.Payload.Comments);
        }

        #endregion

        #region Implementation of IEventHandler<in ConditionalStatusChanged>

        public void Handle(IPublishedEvent<ConditionalStatusChanged> evnt)
        {
            var doc = GetStoredObject(evnt.EventSourceId);
            foreach (var item in evnt.Payload.ResultQuestionsStatus)
            {
                if (!item.Value.HasValue)
                    continue;
                doc.SetQuestionStatus(ParseCrap(item.Key), item.Value.Value);
            }
            foreach (var item in evnt.Payload.ResultGroupsStatus)
            {
                if (!item.Value.HasValue)
                    continue;
                doc.SetScreenStatus(ParseCrap(item.Key), item.Value.Value);
            }
        }
        #endregion
        

        #region Implementation of IEventHandler<in PropagatableGroupAdded>

        public void Handle(IPublishedEvent<PropagatableGroupAdded> evnt)
        {
            var doc = GetStoredObject(evnt.EventSourceId);
                doc.PropagateGroup(evnt.Payload.PublicKey, evnt.Payload.PropagationKey);
            //   doc.AddScreen(rout, current);
        }

        #endregion

        #region Implementation of IEventHandler<in PropagatableGroupDeleted>

        public void Handle(IPublishedEvent<PropagatableGroupDeleted> evnt)
        {
            var doc = GetStoredObject(evnt.EventSourceId);
            doc.RemovePropagatedGroup(evnt.Payload.PublicKey, evnt.Payload.PropagationKey);
        }

        #endregion

        private CompleteQuestionnaireView GetStoredObject(Guid publicKey)
        {
            var doc = _documentStorage.GetById(publicKey);
        /*    if (doc == null || doc.IsRestored)
                return doc;
            _projectionStorage.RestoreProjection(doc);*/
            return doc;
        }

        private ItemPublicKey ParseCrap(string key)
        {
            Guid publicKey;
            if (Guid.TryParse(key, out publicKey))
                return new ItemPublicKey(publicKey, null);
            var pkString = key.Substring(0, key.Length / 2);
            var prKey = key.Substring(key.Length / 2);
            return new ItemPublicKey(Guid.Parse(pkString), Guid.Parse(prKey));
        }

        #region Implementation of IEventHandler<in QuestionnaireStatusChanged>

        public void Handle(IPublishedEvent<QuestionnaireStatusChanged> evnt)
        {
            var document = GetStoredObject(evnt.EventSourceId);
            if(document==null)
                return;
            document.Status = evnt.Payload.Status;
        }

        #endregion
        
    }
}