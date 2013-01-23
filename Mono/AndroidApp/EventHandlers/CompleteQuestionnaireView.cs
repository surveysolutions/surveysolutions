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
        }
        public void AddScreen(List<ICompleteGroup> rout,
            ICompleteGroup group)
        {
            var key = new ItemPublicKey(group.PublicKey,
                                        group.PropagationPublicKey);


            if (group.Propagated == Propagate.None || group.PropagationPublicKey.HasValue)
            {
                var screenItems = BuildItems(group, true);
                var screen = new QuestionnaireScreenViewModel(PublicKey, group.Title, Title,
                                                              key, screenItems,
                                                              BuildSiblings(rout, key),
                                                              BuildBreadCrumbs(rout, key, group.Propagated),
                                                              () => this.Chapters);
                this.Screens.Add(key, screen);
                if (key.PropagationKey.HasValue)
                {
                    QuestionnaireGridViewModel roster;
                    ItemPublicKey rosterKey = new ItemPublicKey(key.PublicKey, null);
                    if (Screens.ContainsKey(rosterKey))
                        roster = Screens[rosterKey] as QuestionnaireGridViewModel;
                    else
                    {
                        roster = new QuestionnaireGridViewModel(PublicKey, group.Title, Title,
                                                                key,
                                                                BuildSiblings(rout, rosterKey),
                                                                BuildBreadCrumbs(rout, rosterKey, group.Propagated),
                                                                () => this.Chapters,
                                                                group.Children.OfType<ICompleteQuestion>().Select(
                                                                    BuildHeader).ToList());
                        this.Screens.Add(rosterKey, roster);
                    }
                    roster.Rows.Add(new RosterItem(key, screenItems,
                                                   () => GetPropagatebleGroupTitle(key.PropagationKey.Value)));
                }
            }

        }


        public IDictionary<ItemPublicKey, IQuestionnaireViewModel> Screens { get; private set; }
        public IDictionary<ItemPublicKey, QuestionViewModel> Questions { get; private set; }
        public IEnumerable<QuestionnaireNavigationPanelItem> Chapters { get; private set; }
        public string Title { get; private set; }
        public Guid PublicKey { get; private set; }

        protected string GetPropagatebleGroupTitle(Guid propagationKey)
        {
            return
                string.Concat(
                    Questions.Where(q => q.Key.PropagationKey == propagationKey && q.Value.Capital).Select(
                        q => q.Value.AnswerString));
        }

        protected IList<QuestionnaireNavigationPanelItem> BuildBreadCrumbs(IList<ICompleteGroup> rout, ItemPublicKey key, Propagate nodeType)
        {
            var baseRout = rout.Skip(1).Select(BuildNavigationItem).ToList();
            if (nodeType == Propagate.None || !key.PropagationKey.HasValue)
                return baseRout;
            var last = baseRout.Last();
            baseRout.Remove(baseRout.Last());
            baseRout.Add(new QuestionnaireNavigationPanelItem(new ItemPublicKey(key.PublicKey, null), last.Title, 0, 0));
            baseRout.Add(new QuestionnaireNavigationPanelItem(key, GetPropagatebleGroupTitle(key.PropagationKey.Value),
                                                              0, 0));
            return
                baseRout;

        }

        protected IList<QuestionnaireNavigationPanelItem> BuildSiblings(IList<ICompleteGroup> rout, ItemPublicKey key)
        {
            var parent = rout[rout.Count - 2];
            IEnumerable<ICompleteGroup> result;
            if (key.PropagationKey.HasValue)
                result = parent.Children.OfType<ICompleteGroup>().Where(
                    c => c.PropagationPublicKey.HasValue && c.PublicKey == key.PublicKey);
            else

                result =
                    parent.Children.OfType<ICompleteGroup>().Distinct(new PropagatedGroupEqualityComparer());
            return result.Select(BuildNavigationItem).ToList();
        }
        
        protected QuestionnaireNavigationPanelItem BuildNavigationItem(ICompleteGroup g)
        {
            return new QuestionnaireNavigationPanelItem(new ItemPublicKey(g.PublicKey, g.PropagationPublicKey), g.Title, 0, 0);
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
                return new GroupViewModel(new ItemPublicKey(group.PublicKey, group.PropagationPublicKey), group.Title,
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
            if (type == QuestionType.Text || type == QuestionType.Percentage || type == QuestionType.Text)
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