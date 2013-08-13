using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails
{
    public class QuestionnairePropagatedScreenViewModel : QuestionnaireScreenViewModel
    {
        [JsonConstructor]
        public QuestionnairePropagatedScreenViewModel(Guid questionnaireId, string screenName, string title,
                                                      bool enabled,
                                                      ItemPublicKey screenId,
                                                      IList<IQuestionnaireItemViewModel> items,
                                                      IEnumerable<ItemPublicKey> breadcrumbs, int total, int answered,
            IQuestionnaireItemViewModel next, IQuestionnaireItemViewModel previous
            )
            : base(questionnaireId, screenName, title, enabled, screenId, items, breadcrumbs,  total,  answered)
        {
            this.Next = next;
            this.Previous = previous;
        }
        public QuestionnairePropagatedScreenViewModel(Guid questionnaireId, string title,
                                                      bool enabled,
                                                      ItemPublicKey screenId,
                                                      IList<IQuestionnaireItemViewModel> items,
                                                      Func<Guid, IEnumerable<ItemPublicKey>> sibligs,
                                                      IEnumerable<ItemPublicKey> breadcrumbs)
            : this(questionnaireId,  title, enabled, screenId, items,sibligs, breadcrumbs, null, null)
        {
        }
        protected QuestionnairePropagatedScreenViewModel(Guid questionnaireId, string title,
                                                     bool enabled,
                                                     ItemPublicKey screenId,
                                                     IList<IQuestionnaireItemViewModel> items,
                                                     Func<Guid, IEnumerable<ItemPublicKey>> sibligs,
                                                     IEnumerable<ItemPublicKey> breadcrumbs, IQuestionnaireItemViewModel next, IQuestionnaireItemViewModel previous)
            : this(questionnaireId, title, title, enabled, screenId, items, breadcrumbs, 0, 0, next, previous)
        {
            this.sibligsValue = sibligs;
            if (!screenId.IsTopLevel)
            {

                this.ScreenName = string.Empty;
            }
        }

        public QuestionnairePropagatedScreenViewModel Clone(int[] propagationVector,
                                                            IList<IQuestionnaireItemViewModel> items)
        {
            if (!ScreenId.IsTopLevel)
                throw new InvalidOperationException("only template can mutate in that way");
            var key = new ItemPublicKey(this.ScreenId.PublicKey, propagationVector);
            var bradCrumbs = this.Breadcrumbs.ToList();
            return new QuestionnairePropagatedScreenViewModel(this.QuestionnaireId,
                                                              this.Title, true,
                                                              key, items,
                                                              sibligsValue, bradCrumbs,
                                                              this.Next != null
                                                                  ? this.Next.Clone(propagationVector)
                                                                  : null,
                                                              this.Previous != null
                                                                  ? this.Previous.Clone(propagationVector)
                                                                  : null);
        }

        public QuestionnairePropagatedScreenViewModel Clone(int[] propagationVector)
        {

            IList<IQuestionnaireItemViewModel> items = this.Items.Select(questionnaireItemViewModel => questionnaireItemViewModel.Clone(propagationVector)).ToList();
            return Clone(propagationVector, items);
        }

        public void AddNextPrevious(IQuestionnaireItemViewModel next, IQuestionnaireItemViewModel previous)
        {
            if (!ScreenId.IsTopLevel)
                throw new InvalidOperationException("only template can mutate in that way");
            this.Next = next;
            this.Previous = previous;
        }

        private readonly Func<Guid, IEnumerable<ItemPublicKey>> sibligsValue;

        public void UpdateScreenName(string screenName)
        {
            this.ScreenName = screenName;
            RaisePropertyChanged("ScreenName");
        }
        [JsonIgnore]
        public override IEnumerable<ItemPublicKey> Siblings
        {
            get { return sibligsValue(this.ScreenId.PublicKey); }
        }

        public IQuestionnaireItemViewModel Next { get; private set; }
        public IQuestionnaireItemViewModel Previous { get; private set; }
    }
}