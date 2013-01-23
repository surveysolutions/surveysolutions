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
                                                     IEventHandler<CommentSeted>, 
                                                     IEventHandler<SnapshootLoaded>,
                                                     /*IEventHandler<CompleteQuestionnaireDeleted>, */
                                                     IEventHandler<AnswerSet>/*, 
                                                     IEventHandler<PropagatableGroupAdded>, 
                                                     IEventHandler<PropagatableGroupDeleted>, 
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
            var view = new CompleteQuestionnaireView(document.PublicKey, document.Title, BuildChapters(document));
            List<ICompleteGroup> rout = new List<ICompleteGroup>();
            rout.Add(document);
            Stack<ICompleteGroup> queue = new Stack<ICompleteGroup>(document.Children.OfType<ICompleteGroup>());
            while (queue.Count > 0)
            {
                var current = queue.Pop();

                while (rout.Count > 0 && !rout[rout.Count - 1].Children.Contains(current))
                {
                    rout.RemoveAt(rout.Count - 1);
                }
                rout.Add(current);
                view.AddScreen(rout, current);
               
                foreach (ICompleteGroup child in current.Children.OfType<ICompleteGroup>())
                {
                    queue.Push(child);
                }
            }
            _documentStorage.Store(view, document.PublicKey);
        }

        protected IEnumerable<QuestionnaireNavigationPanelItem> BuildChapters(CompleteQuestionnaireDocument root)
        {
            return
                root.Children.OfType<ICompleteGroup>().Select(BuildNavigationItem).ToList();
        }
        protected QuestionnaireNavigationPanelItem BuildNavigationItem(ICompleteGroup g)
        {
            return new QuestionnaireNavigationPanelItem(new ItemPublicKey(g.PublicKey, null), g.Title, 0, 0);
        }
        #endregion

        #region Implementation of IEventHandler<in AnswerSet>

        public void Handle(IPublishedEvent<AnswerSet> evnt)
        {
            var doc = _documentStorage.Query().First();
            var question =
                doc.Questions[new ItemPublicKey(evnt.Payload.QuestionPublicKey, evnt.Payload.PropogationPublicKey)];
            question.SetAnswer(evnt.Payload.AnswerKeys, evnt.Payload.AnswerString);
        }

        #endregion

        #region Implementation of IEventHandler<in CommentSeted>

        public void Handle(IPublishedEvent<CommentSeted> evnt)
        {
            var doc = _documentStorage.Query().First();
            var question =
                doc.Questions[new ItemPublicKey(evnt.Payload.QuestionPublickey, evnt.Payload.PropogationPublicKey)];
            question.SetComment(evnt.Payload.Comments);
        }

        #endregion
    }
}