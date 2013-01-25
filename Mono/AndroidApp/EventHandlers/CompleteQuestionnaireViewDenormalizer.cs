using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidApp.ViewModel.QuestionnaireDetails;
using AndroidApp.ViewModel.QuestionnaireDetails.GridItems;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.Events.Questionnaire.Completed;
using Main.DenormalizerStorage;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Restoring.EventStapshoot;
using Newtonsoft.Json;

namespace AndroidApp.EventHandlers
{
    public class CompleteQuestionnaireViewDenormalizer:/* IEventHandler<NewCompleteQuestionnaireCreated>, */
        IEventHandler<ConditionalStatusChanged>,
    IEventHandler<CommentSet>, 
                                                     IEventHandler<SnapshootLoaded>,
                                                     /*IEventHandler<CompleteQuestionnaireDeleted>, */
                                                     IEventHandler<AnswerSet>, 
                                                     IEventHandler<PropagatableGroupAdded>, 
                                                     IEventHandler<PropagatableGroupDeleted>/*, 
                                                     IEventHandler<QuestionnaireAssignmentChanged>, 
                                                     IEventHandler<QuestionnaireStatusChanged>*/
    {
        /// <summary>
        /// The _document storage.
        /// </summary>
        private readonly IDenormalizerStorage<CompleteQuestionnaireView> _documentStorage;
        #region Implementation of IEventHandler<in SnapshootLoaded>

        public CompleteQuestionnaireViewDenormalizer(IDenormalizerStorage<CompleteQuestionnaireView> documentStorage)
        {
            _documentStorage = documentStorage;
        }

        public void Handle(IPublishedEvent<SnapshootLoaded> evnt)
        {
            var document = evnt.Payload.Template.Payload as CompleteQuestionnaireDocument;
            if (document == null)
                return;
            var view = new CompleteQuestionnaireView(document);

            _documentStorage.Store(view, document.PublicKey);
        }


        #endregion

        #region Implementation of IEventHandler<in AnswerSet>

        public void Handle(IPublishedEvent<AnswerSet> evnt)
        {
            var doc = _documentStorage.GetByGuid(evnt.EventSourceId);
            doc.SetAnswer(new ItemPublicKey(evnt.Payload.QuestionPublicKey, evnt.Payload.PropogationPublicKey),
                          evnt.Payload.AnswerKeys, evnt.Payload.AnswerString);
        }

        #endregion

        #region Implementation of IEventHandler<in CommentSeted>

        public void Handle(IPublishedEvent<CommentSet> evnt)
        {
            var doc = _documentStorage.GetByGuid(evnt.EventSourceId);
            doc.SetComment(new ItemPublicKey(evnt.Payload.QuestionPublickey, evnt.Payload.PropagationPublicKey),
                           evnt.Payload.Comments);
        }

        #endregion

        #region Implementation of IEventHandler<in ConditionalStatusChanged>

        public void Handle(IPublishedEvent<ConditionalStatusChanged> evnt)
        {
            var doc = _documentStorage.GetByGuid(evnt.EventSourceId);
            foreach (var item in evnt.Payload.ResultQuestionsStatus)
            {
                if (!item.Value.HasValue)
                    continue;
                doc.SetQuestionStatus(ParseCrap(item.Key), item.Value.Value);
            }
        }
        #endregion
        private ItemPublicKey ParseCrap(string key)
        {
            Guid publicKey;
            if (Guid.TryParse(key, out publicKey))
                return new ItemPublicKey(publicKey, null);
            var pkString = key.Substring(0, key.Length/2);
            var prKey = key.Substring(key.Length/2);
            return new ItemPublicKey(Guid.Parse(pkString), Guid.Parse(prKey));
        }

        

        #region Implementation of IEventHandler<in PropagatableGroupAdded>

        public void Handle(IPublishedEvent<PropagatableGroupAdded> evnt)
        {
            var doc = _documentStorage.GetByGuid(evnt.EventSourceId);
                doc.PropagateGroup(evnt.Payload.PublicKey, evnt.Payload.PropagationKey);
            //   doc.AddScreen(rout, current);
        }

        #endregion

        #region Implementation of IEventHandler<in PropagatableGroupDeleted>

        public void Handle(IPublishedEvent<PropagatableGroupDeleted> evnt)
        {
            var doc = _documentStorage.GetByGuid(evnt.EventSourceId);
            doc.RemovePropagatedGroup(evnt.Payload.PublicKey, evnt.Payload.PropagationKey);
        }

        #endregion
    }
}