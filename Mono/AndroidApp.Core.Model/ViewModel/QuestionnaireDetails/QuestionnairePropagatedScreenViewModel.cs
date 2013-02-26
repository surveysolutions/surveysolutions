using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;

namespace AndroidApp.Core.Model.ViewModel.QuestionnaireDetails
{
    public class QuestionnairePropagatedScreenViewModel : QuestionnaireScreenViewModel
    {
        [JsonConstructor]
        public QuestionnairePropagatedScreenViewModel(Guid questionnaireId, string screenName, string title,
                                                      bool enabled,
                                                      ItemPublicKey screenId,
                                                      IEnumerable<IQuestionnaireItemViewModel> items,
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
                                                      IEnumerable<IQuestionnaireItemViewModel> items,
                                                      Func<IEnumerable<ItemPublicKey>> sibligs,
                                                      IEnumerable<ItemPublicKey> breadcrumbs)
            : this(questionnaireId,  title, enabled, screenId, items,sibligs, breadcrumbs, null, null)
        {
        }
        public QuestionnairePropagatedScreenViewModel(Guid questionnaireId, string title,
                                                     bool enabled,
                                                     ItemPublicKey screenId,
                                                     IEnumerable<IQuestionnaireItemViewModel> items,
                                                     Func<IEnumerable<ItemPublicKey>> sibligs,
                                                     IEnumerable<ItemPublicKey> breadcrumbs, IQuestionnaireItemViewModel next, IQuestionnaireItemViewModel previous)
            : this(questionnaireId, title, title, enabled, screenId, items, breadcrumbs, 0, 0, next, previous)
        {

            if (screenId.PropagationKey.HasValue)
            {
                this.sibligsValue = sibligs;
                this.ScreenName = string.Empty;
            }
        }
        public void AddNextPrevious(IQuestionnaireItemViewModel next, IQuestionnaireItemViewModel previous)
        {
            this.Next = next;
            this.Previous = previous;
        }

        private readonly Func<IEnumerable<ItemPublicKey>> sibligsValue;

        public void UpdateScreenName(string screenName)
        {
            this.ScreenName = screenName;
            RaisePropertyChanged("ScreenName");
        }
        [JsonIgnore]
        public override IEnumerable<ItemPublicKey> Siblings
        {
            get { return sibligsValue(); }
        }

        public IQuestionnaireItemViewModel Next { get; private set; }
        public IQuestionnaireItemViewModel Previous { get; private set; }
    }
}