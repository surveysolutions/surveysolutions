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

namespace AndroidApp.EventHandlers
{
    public class CompleteQuestionnaireView
    {
        public CompleteQuestionnaireView(Guid publicKey, string title, IEnumerable<QuestionnaireNavigationPanelItem> chapters)
        {
            this.PublicKey = publicKey;
            this.Title = title;
            this.Chapters = chapters;
            Screens = new Dictionary<ItemPublicKey, IQuestionnaireViewModel>();
            Questions=new Dictionary<ItemPublicKey, QuestionViewModel>();
            Templates=new Dictionary<Guid, QuestionnaireScreenViewModel>();
        }
        public void AddScreen(List<ICompleteGroup> rout,
            ICompleteGroup group)
        {
            var key = new ItemPublicKey(group.PublicKey,
                                        group.PropagationPublicKey);


            if (group.Propagated == Propagate.None)
            {
                var screenItems = BuildItems(group, true);
                var screen = new QuestionnaireScreenViewModel(PublicKey, group.Title, Title,
                                                              key, screenItems,
                                                              BuildSiblings(rout, key),
                                                              BuildBreadCrumbs(rout, key, group.Propagated),
                                                              () => this.Chapters);
                this.Screens.Add(key, screen);
            }
            else if (group.PropagationPublicKey.HasValue)
            {
                var screenItems = BuildItems(group, true);
                var screen = new QuestionnaireScreenViewModel(PublicKey, group.Title, () => GetPropagatebleGroupTitle(group.PropagationPublicKey.Value),
                                                              key, screenItems,
                                                              () => GetSiblings(key.PublicKey),
                                                              BuildBreadCrumbs(rout, key, group.Propagated),
                                                              () => this.Chapters);
                this.Screens.Add(key, screen);
            }
            else
            {

                CreateGrid(group, rout);
            }

        }

        protected void CreateGrid(ICompleteGroup group, List<ICompleteGroup> rout)
        {
            ItemPublicKey rosterKey = new ItemPublicKey(group.PublicKey, null);
            var siblings = BuildSiblings(rout, rosterKey);
            var screenItems = BuildItems(group, false);
            var breadcrumbs = BuildBreadCrumbs(rout, rosterKey, group.Propagated);
            var template = new QuestionnaireScreenViewModel(PublicKey, group.Title, Title,
                                                            rosterKey, screenItems,
                                                            siblings,
                                                            breadcrumbs,
                                                            () => this.Chapters);
            Templates.Add(rosterKey.PublicKey, template);
            var roster = new QuestionnaireGridViewModel(PublicKey, group.Title, Title,
                                                                  rosterKey,
                                                                  siblings,
                                                                  breadcrumbs,
                                                                  () => this.Chapters,
                                                                  group.Children.OfType<ICompleteQuestion>().Select(
                                                                      BuildHeader).ToList(),
                                                                  () => CollectPropagatedScreen(rosterKey.PublicKey));
            this.Screens.Add(rosterKey, roster);
        }
        protected IList<IQuestionnaireItemViewModel> BuildItems(ICompleteGroup screen, bool updateHash)
        {
            IList<IQuestionnaireItemViewModel> result = new List<IQuestionnaireItemViewModel>();
            foreach (var children in screen.Children)
            {
                var item = CreateView(children);
                if (item == null)
                    continue;
                var question = item as QuestionViewModel;
                if (question != null && updateHash)
                    this.Questions.Add(question.PublicKey, question);
                result.Add(item);
            }
            return result;
            //  return screen.Children.Select(CreateView).Where(c => c != null);
        }
        public void PropagateGroup(Guid publicKey, Guid propagationKey)
        {
            var template = this.Templates[publicKey];
            var key = new ItemPublicKey(publicKey, propagationKey);
            var bradCrumbs = template.Breadcrumbs.ToList();

            IList<IQuestionnaireItemViewModel> items = new List<IQuestionnaireItemViewModel>();
            foreach (var questionnaireItemViewModel in template.Items)
            {
                var newItem = questionnaireItemViewModel.Clone(propagationKey);
                items.Add(newItem);
                var newQuestion = newItem as QuestionViewModel;
                if (newQuestion != null)
                    this.Questions.Add(newItem.PublicKey, newQuestion);
              
            }
            var screen = new QuestionnaireScreenViewModel(PublicKey, template.Title,
                                                          () => GetPropagatebleGroupTitle(propagationKey),
                                                          key, items,
                                                          () => GetSiblings(key.PublicKey), bradCrumbs,
                                                          () => this.Chapters);

            this.Screens.Add(key, screen);
        }

        protected IEnumerable<QuestionnaireNavigationPanelItem> GetSiblings(Guid publicKey)
        {
            return this.Screens.Where(s => s.Key.PublicKey == publicKey && s.Key.PropagationKey.HasValue).Select(
                        s => new QuestionnaireNavigationPanelItem(s.Key, s.Value.Title, 0, 0, true)).ToList();
        }

        public void RemovePropagatedGroup(Guid publicKey, Guid propagationKey)
        {
            var key = new ItemPublicKey(publicKey, propagationKey);
            var screen = this.Screens[key] as QuestionnaireScreenViewModel;
            foreach (var item in screen.Items)
            {
                var question = item as QuestionViewModel;
                if (question != null)
                    this.Questions.Remove(question.PublicKey);
            }
            this.Screens.Remove(key);
            
        }

        public IDictionary<ItemPublicKey, IQuestionnaireViewModel> Screens { get; private set; }
        public IDictionary<Guid, QuestionnaireScreenViewModel> Templates { get; private set; }
        public IDictionary<ItemPublicKey, QuestionViewModel> Questions { get; private set; }
        public IEnumerable<QuestionnaireNavigationPanelItem> Chapters { get; private set; }
        public string Title { get; private set; }
        public Guid PublicKey { get; private set; }
        protected IEnumerable<QuestionnaireScreenViewModel> CollectPropagatedScreen(Guid publicKey)
        {
            return
                this.Screens.Where(s => s.Key.PublicKey == publicKey && s.Key.PropagationKey.HasValue).Select(
                    s => s.Value).OfType<QuestionnaireScreenViewModel>().ToList();
        }

        protected string GetPropagatebleGroupTitle(Guid propagationKey)
        {
            return
                string.Concat(
                    Questions.Where(q => q.Key.PropagationKey == propagationKey && q.Value.Capital).Select(
                        q => q.Value.AnswerString));
        }
      /*  protected void AddPropagatebleBreadCrumb(IList<QuestionnaireNavigationPanelItem> baseRout, ItemPublicKey key)
        {
            var last = baseRout.Last();
            baseRout.Remove(baseRout.Last());
            baseRout.Add(new QuestionnaireNavigationPanelItem(new ItemPublicKey(key.PublicKey, null), last.Text, 0, 0, true));
            baseRout.Add(new QuestionnaireNavigationPanelItem(key, GetPropagatebleGroupTitle(key.PropagationKey.Value),
                                                              0, 0, true));
        }*/

        protected IList<QuestionnaireNavigationPanelItem> BuildBreadCrumbs(IList<ICompleteGroup> rout, ItemPublicKey key, Propagate nodeType)
        {
            return  rout.Skip(1).Select(BuildNavigationItem).ToList();
        }

        protected IList<QuestionnaireNavigationPanelItem> BuildSiblings(IList<ICompleteGroup> rout, ItemPublicKey key)
        {
            var parent = rout[rout.Count - 2];
            IEnumerable<ICompleteGroup> result =
                parent.Children.OfType<ICompleteGroup>().Distinct(new PropagatedGroupEqualityComparer());
            return result.Select(BuildNavigationItem).ToList();
        }

        protected QuestionnaireNavigationPanelItem BuildNavigationItem(ICompleteGroup g)
        {
            return new QuestionnaireNavigationPanelItem(new ItemPublicKey(g.PublicKey, g.PropagationPublicKey), g.Title, 0, 0,g.Enabled);
        }

        protected HeaderItem BuildHeader(ICompleteQuestion question)
        {
            /*  var newType = CalculateViewType(question.QuestionType);
              if (!IsTypeSelectable(newType))*/
            return new HeaderItem(question.PublicKey, question.QuestionText, question.Instructions);
            /*  return new SelectableHeaderItem(question.PublicKey, question.QuestionText, question.Instructions,
                                              question.Answers.OfType<ICompleteAnswer>().Select(
                                                  a =>
                                                  new AnswerViewModel(a.PublicKey, a.AnswerText, a.Selected)));*/
        }
       

        

        protected IQuestionnaireItemViewModel CreateView(IComposite item)
        {
            var question = item as ICompleteQuestion;

            if (question != null)
            {
                var newType = CalculateViewType(question.QuestionType);
                QuestionViewModel questionView;
                if (!IsTypeSelectable(newType))
                    questionView= new ValueQuestionViewModel(new ItemPublicKey(question.PublicKey, question.PropagationPublicKey), question.QuestionText,
                                                      newType,
                                                      question.GetAnswerString(),
                                                      question.Enabled, question.Instructions, question.Comments,
                                                      question.Valid,question.Capital, question.Mandatory);
                else
                    questionView= new SelectebleQuestionViewModel(new ItemPublicKey(question.PublicKey, question.PropagationPublicKey), question.QuestionText,
                                                           newType,
                                                           question.Answers.OfType<ICompleteAnswer>().Select(
                                                               a =>
                                                               new AnswerViewModel(a.PublicKey, a.AnswerText, a.Selected)).ToList(),
                                                           question.Enabled, question.Instructions, question.Comments,
                                                           question.Valid, question.Mandatory,question.Capital, question.GetAnswerString());
              //  questionView.PropertyChanged += questionView_PropertyChanged;
                return questionView;

            }
            var group = item as ICompleteGroup;
            if (group != null && !group.PropagationPublicKey.HasValue)
                return new QuestionnaireNavigationPanelItem(new ItemPublicKey(group.PublicKey, group.PropagationPublicKey), group.Title,0,0,
                                          group.Enabled);
            return null;
        }

      /*  void questionView_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Answered")
                return;
            QuestionViewModel question = sender as QuestionViewModel;
        }*/

        protected QuestionType CalculateViewType(QuestionType type)
        {
            if (type == QuestionType.Numeric || type == QuestionType.AutoPropagate)
                return QuestionType.Numeric;
            if (type == QuestionType.Text || type == QuestionType.Text)
                return QuestionType.Text;
            if (type == QuestionType.SingleOption || type == QuestionType.DropDownList || type == QuestionType.YesNo)
                return QuestionType.SingleOption;
            return type;
        }

        protected bool IsTypeSelectable(QuestionType type)
        {
            if (type == QuestionType.SingleOption || type == QuestionType.MultyOption)
                return true;
            return false;
        }

        private class PropagatedGroupEqualityComparer : IEqualityComparer<ICompleteGroup>
        {

            public bool Equals(ICompleteGroup b1, ICompleteGroup b2)
            {
                if (b1.PublicKey == b2.PublicKey)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }


            public int GetHashCode(ICompleteGroup bx)
            {
                return bx.PublicKey.GetHashCode();
            }

        }
    }
}