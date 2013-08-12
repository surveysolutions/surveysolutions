using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails.GridItems;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails.Validation;
using Cirrious.MvvmCross.ViewModels;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;

using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;

namespace CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails
{
    public class CompleteQuestionnaireView : MvxViewModel, IView
    {
        public CompleteQuestionnaireView(string publicKey)
        {
            this.PublicKey = Guid.Parse(publicKey); ;
        }

        public CompleteQuestionnaireView(CompleteQuestionnaireDocument document)
        {
            this.PublicKey = document.PublicKey;
            this.Title = string.Format("{0} - {1}", document.Title,
                                       string.Concat(
                                           (IEnumerable<string>)
                                           document.Find<ICompleteQuestion>(q => q.Featured).Select(
                                               q => q.GetAnswerString()+" ")));
            this.Status = document.Status;
            this.Screens = new Dictionary<ItemPublicKey, IQuestionnaireViewModel>();
            this.Questions = new Dictionary<ItemPublicKey, QuestionViewModel>();
            this.Templates = new TemplateCollection();

            FillQuestionnairePostOrder(document);
            this.Chapters =
                Enumerable.OfType<QuestionnaireScreenViewModel>(document.Children.OfType<ICompleteGroup>().Select(
                    c => this.Screens[new ItemPublicKey(c.PublicKey, c.PropagationPublicKey)])).ToList();

         
        }

        protected void FillQuestionnairePostOrder(ICompleteGroup document)
        {
            List<ICompleteGroup> rout = new List<ICompleteGroup>();
            List<ICompleteGroup> propagatedPostOrder = new List<ICompleteGroup>();
            rout.Add(document);
            Stack<ICompleteGroup> queue = new Stack<ICompleteGroup>(document.Children.OfType<ICompleteGroup>());
            while (queue.Count > 0)
            {
                var current = queue.Pop();

                while (rout.Count > 0 && !rout[rout.Count - 1].Children.Contains(current))
                {
                    this.AddScreen(rout,propagatedPostOrder, rout.Last());
                    rout.RemoveAt(rout.Count - 1);
                }
                rout.Add(current);
                foreach (ICompleteGroup child in current.Children.OfType<ICompleteGroup>())
                {
                    queue.Push(child);
                }
            }
            var last = rout.Last();
            while (!(last is CompleteQuestionnaireDocument))
            {
                AddScreen(rout,propagatedPostOrder, last);
                rout.Remove(last);
                last = rout.Last();
            }

            CreateNextPrevious();
            CreatePropagatedGroupd(propagatedPostOrder);
        }
        protected void CreatePropagatedGroupd(List<ICompleteGroup> propagatedPostOrder)
        {
            foreach (var completeGroup in propagatedPostOrder)
            {
                var key = new ItemPublicKey(completeGroup.PublicKey, completeGroup.PropagationPublicKey.Value);
                var screenItems = BuildItems(completeGroup, true);
                var template = this.Templates[completeGroup.PublicKey];
                var screen = template.Clone(completeGroup.PropagationPublicKey.Value, screenItems);
                this.Screens.Add(key, screen);
                screen.PropertyChanged += screen_PropertyChanged;
                UpdateGrid(completeGroup.PublicKey);
                UpdatePropagatedGroupScreenName(completeGroup.PropagationPublicKey.Value);
            }
        }

        protected void CreateNextPrevious()
        {
            var templates = Templates.Select(t => t.ScreenId.PublicKey).ToList();
            while (templates.Count > 0)
            {
                var first = templates[0];
                var scope = Templates.GetScopeByItem(first).ToArray();
                templates.RemoveAll(t => scope.Any(s => s == t));
                for (int i = 0; i < scope.Length; i++)
                {
                    var target = this.Templates[scope[i]];
                    IQuestionnaireItemViewModel next = null;
                    IQuestionnaireItemViewModel previous = null;
                    if (i > 0)
                    {
                        var item = this.Templates[scope[i - 1]];
                        previous = new QuestionnaireNavigationPanelItem(item.ScreenId, item);
                    }
                    if (i < scope.Length - 1)
                    {
                        var item = this.Templates[scope[i + 1]];
                        next = new QuestionnaireNavigationPanelItem(item.ScreenId, item);
                    }
                    target.AddNextPrevious(next, previous);
                }
            }
        }

        #region fields

        public Guid PublicKey { get; private set; }

        public string Title { get; private set; }
        public SurveyStatus Status { get; set; }
        public IDictionary<ItemPublicKey, IQuestionnaireViewModel> Screens { get; protected set; }
        public IList<QuestionnaireScreenViewModel> Chapters { get; protected set; }

        protected TemplateCollection Templates { get; set; }
        protected IDictionary<ItemPublicKey, QuestionViewModel> Questions { get;  set; }


        #endregion


        #region public methods



        public void PropagateGroup(Guid publicKey, Guid propagationKey)
        {
            var template = this.Templates[publicKey];
            var screen = template.Clone(propagationKey);
            foreach (var question in screen.Items.OfType<QuestionViewModel>())
            {
                UpdateQuestionHash(question);
            }
            screen.PropertyChanged += screen_PropertyChanged;
            this.Screens.Add(screen.ScreenId, screen);
            UpdateGrid(publicKey);
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
            UpdateGrid(publicKey);

        }
        public IEnumerable<IQuestionnaireViewModel> RestoreBreadCrumbs(IEnumerable<ItemPublicKey> breadcrumbs)
        {
            return breadcrumbs.Select(b => this.Screens[b]);
        }

        public void SetAnswer(ItemPublicKey key, object  answer)
        {
            var question =
                this.Questions[key];
            question.SetAnswer(answer);
        }

        public void SetComment(ItemPublicKey key, string comment)
        {
            var question =
                this.Questions[key];
            question.SetComment(comment);
        }

        public void SetQuestionStatus(ItemPublicKey key, bool enebled)
        {
            if (!this.Questions.ContainsKey(key))
                return;

            var question =
                this.Questions[key];
            question.SetEnabled(enebled);
        }
        public void SetQuestionValidity(ItemPublicKey key, bool valid)
        {
            if (!this.Questions.ContainsKey(key))
                return;

            var question =
                this.Questions[key];
            question.SetValid(valid);
        }
        public void SetScreenStatus(ItemPublicKey key, bool enebled)
        {
            var screen =
                this.Screens[key];
            screen.SetEnabled(enebled);
        }

        public IEnumerable<QuestionViewModel> FindQuestion(Func<QuestionViewModel, bool> filter)
        {
            return this.Questions.Select(q => q.Value).Where(filter);
        }

       

        #endregion

        #region event handlers

        private void question_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "AnswerString")
                return;
            var question = sender as QuestionViewModel;
            if (question == null || !question.Capital || !question.PublicKey.PropagationKey.HasValue)
                return;
           
            UpdatePropagatedGroupScreenName(question.PublicKey.PropagationKey.Value);
        }

        private void UpdatePropagatedGroupScreenName(Guid propagationKey)
        {
            var screens =
                Screens.Select(
                    q => q.Value).OfType<QuestionnairePropagatedScreenViewModel>().Where(
                        q => q.ScreenId.PropagationKey == propagationKey);
            var newTitle = string.Concat(
                Questions.Where(q => q.Key.PropagationKey == propagationKey && q.Value.Capital).
                    Select(
                        q => q.Value.AnswerString));
            foreach (var screen in screens)
            {
                screen.UpdateScreenName(newTitle);
            }
        }

        private void screen_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Answered" && e.PropertyName != "Total")
                return;
            var propagatedScreen = sender as QuestionnaireScreenViewModel;
            if (propagatedScreen == null)
                return;
            if (!propagatedScreen.ScreenId.PropagationKey.HasValue)
                return;
            UpdateGrid(propagatedScreen.ScreenId.PublicKey);
        }

        #endregion

        #region protected helper methods
        protected void AddScreen(List<ICompleteGroup> rout,List<ICompleteGroup> propagatedPostOrder,
                            ICompleteGroup group)
        {
            var key = new ItemPublicKey(group.PublicKey,
                                        group.PropagationPublicKey);


            if (group.Propagated == Propagate.None)
            {
                var screenItems = BuildItems(group, true);
                var screen = new QuestionnaireScreenViewModel(PublicKey, group.Title, Title, group.Enabled,
                                                              key, screenItems,
                                                              BuildSiblingsForNonPropagatedGroups(rout, key),
                                                              BuildBreadCrumbs(rout, key));
                this.Screens.Add(key, screen);
            }
            else if (group.PropagationPublicKey.HasValue)
            {
                propagatedPostOrder.Add(group);
       
            }
            else
            {
                var gridKey = new ItemPublicKey(group.PublicKey, null);
                if (!this.Screens.ContainsKey(gridKey))
                {
                    CreateGrid(group, rout);
                }
            }

        }

        protected void UpdateQuestionHash(QuestionViewModel question)
        {
            this.Questions.Add(question.PublicKey, question);
            if (question.Capital && question.PublicKey.PropagationKey.HasValue)
            {
                question.PropertyChanged += question_PropertyChanged;
            }
        }

        protected void CreateGrid(ICompleteGroup group, List<ICompleteGroup> rout)
        {
            ItemPublicKey rosterKey = new ItemPublicKey(group.PublicKey, null);
            var siblings = BuildSiblingsForNonPropagatedGroups(rout, rosterKey);
            var screenItems = BuildItems(group, false);
            var breadcrumbs = BuildBreadCrumbs(rout, rosterKey);

            var roster = new QuestionnaireGridViewModel(PublicKey, group.Title, Title,
                                                        rosterKey, group.Enabled,
                                                        siblings,
                                                        breadcrumbs,
                                                        // this.Chapters,
                                                        Enumerable.ToList<HeaderItem>(@group.Children
                                                                                            .OfType<ICompleteQuestion>()
                                                                                            .Where(
                                                                                                q =>
                                                                                                q.QuestionScope ==
                                                                                                QuestionScope
                                                                                                    .Interviewer)
                                                                                            .Select(
                                                                                                BuildHeader)),
                                                        () => CollectPropagatedScreen(rosterKey.PublicKey));

            breadcrumbs = breadcrumbs.Union(new ItemPublicKey[1] {roster.ScreenId}).ToList();
            var template = new QuestionnairePropagatedScreenViewModel(PublicKey, group.Title, group.Enabled,
                                                                      rosterKey, screenItems,
                                                                      GetSiblings,
                                                                      breadcrumbs);
            Templates.Add(rosterKey.PublicKey, template);
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
                    UpdateQuestionHash(question);
                result.Add(item);
            }
            return result;
            //  return screen.Children.Select(CreateView).Where(c => c != null);
        }

        protected IEnumerable<ItemPublicKey> GetSiblings(Guid publicKey)
        {
            return
                this.Screens.Where(s => s.Key.PublicKey == publicKey && s.Key.PropagationKey.HasValue).Select(
                    s => new ItemPublicKey(publicKey, s.Key.PropagationKey)).ToList();
        }

        protected void UpdateGrid(Guid key)
        {
            var gridkey = new ItemPublicKey(key, null);
            var grid = this.Screens[gridkey] as QuestionnaireGridViewModel;
            if (grid != null)
                grid.UpdateCounters();
        }

        protected IEnumerable<QuestionnairePropagatedScreenViewModel> CollectPropagatedScreen(Guid publicKey)
        {
            return
                this.Screens.Select(
                    s => s.Value).OfType<QuestionnairePropagatedScreenViewModel>().Where(s => s.ScreenId.PublicKey == publicKey).ToList();
        }

        protected IList<ItemPublicKey> BuildBreadCrumbs(IList<ICompleteGroup> rout, ItemPublicKey key)
        {
            return
                rout.Skip(1).TakeWhile(r => r.PublicKey != key.PublicKey).Select(
                    r => new ItemPublicKey(r.PublicKey, r.PropagationPublicKey)).ToList();
        }

        protected IEnumerable<ItemPublicKey> BuildSiblingsForNonPropagatedGroups(IList<ICompleteGroup> rout,
                                                                                 ItemPublicKey key)
        {
            var parent = rout[rout.Count - 2];
            return
                Enumerable.ToList<ItemPublicKey>(parent.Children.OfType<ICompleteGroup>().Distinct(new PropagatedGroupEqualityComparer()).Select(
                        g => new ItemPublicKey(g.PublicKey, g.PropagationPublicKey)));
        }

    
        protected HeaderItem BuildHeader(ICompleteQuestion question)
        {
            return new HeaderItem(question.PublicKey, question.QuestionText, question.Instructions);
        }
        protected string BuildComments(IEnumerable<CommentDocument> comments)
        {
            if (comments == null)
                return string.Empty;
            return comments.Any() ? comments.Last().Comment : string.Empty;
        }

        protected IQuestionnaireItemViewModel CreateView(IComposite item)
        {
            var question = item as ICompleteQuestion;

            if (question != null)
            {
                if (question.QuestionScope != QuestionScope.Interviewer || question.Featured)
                    return null;
                var newType = CalculateViewType(question.QuestionType);
                QuestionViewModel questionView;
                if (!IsTypeSelectable(newType))
                    questionView =
                        new ValueQuestionViewModel(
                            new ItemPublicKey(question.PublicKey, question.PropagationPublicKey), question.QuestionText,
                            newType,
                            question.GetAnswerString(),
                            question.Enabled, question.Instructions, BuildComments(question.Comments),
                            question.Valid, question.Capital, question.Mandatory, question.ValidationExpression,
                            question.ValidationMessage);
                else
                    questionView =
                        new SelectebleQuestionViewModel(
                            new ItemPublicKey(question.PublicKey, question.PropagationPublicKey), question.QuestionText,
                            newType,
                            Enumerable.ToList<AnswerViewModel>(question.Answers.OfType<ICompleteAnswer>().Select(
                                    a =>
                                    new AnswerViewModel(a.PublicKey, a.AnswerText, a.AnswerValue, a.Selected,a.AnswerImage))),
                            question.Enabled, question.Instructions, BuildComments(question.Comments),
                            question.Valid, question.Mandatory, question.Capital, question.GetAnswerString(),
                            question.ValidationExpression, question.ValidationMessage);

                var trigger = question as IAutoPropagate;
                if (trigger!=null)
                {
                    Templates.AssignScope(item.PublicKey, trigger.Triggers);
                }
                //  questionView.PropertyChanged += questionView_PropertyChanged;
                return questionView;

            }
            var group = item as ICompleteGroup;
            if (group != null && !group.PropagationPublicKey.HasValue)
            {
                var key = new ItemPublicKey(group.PublicKey, group.PropagationPublicKey);
                return
                    new QuestionnaireNavigationPanelItem(
                        key, this.Screens[key]);
            }
            return null;
        }

        protected QuestionType CalculateViewType(QuestionType type)
        {
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

        #endregion

        #region comparer

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

        #endregion

    }
}