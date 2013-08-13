using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails.GridItems;
using Cirrious.MvvmCross.ViewModels;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails
{
    public class CompleteQuestionnaireView : MvxViewModel, IView
    {
        public CompleteQuestionnaireView(IQuestionnaireDocument questionnarie, InterviewSynchronized interviewData)
        {
            this.PublicKey = interviewData.QuestionnaireId;
            this.Status = interviewData.Status;
            this.Screens = new Dictionary<ItemPublicKey, IQuestionnaireViewModel>();
            this.Questions = new Dictionary<ItemPublicKey, QuestionViewModel>();
            this.Templates = new TemplateCollection();
            
            FillQuestionnairePostOrder(questionnarie);
            
            foreach (var propagatedGroupInstanceCount in interviewData.PropagatedGroupInstanceCounts)
            {
                for (int i = 0; i < propagatedGroupInstanceCount.Value; i++)
                {
                    AddPropagateGroup(propagatedGroupInstanceCount.Key.PublicKey,
                                      propagatedGroupInstanceCount.Key.PropagationVector, i);
                }
            }

            foreach (var group in interviewData.DisabledGroups)
            {
                SetScreenStatus(new ItemPublicKey(group.PublicKey, group.PropagationVector), false);
            }

            foreach (var question in interviewData.DisabledQuestions)
            {
                SetQuestionStatus(new ItemPublicKey(question.PublicKey, question.PropagationVector), false);
            }

            foreach (var question in interviewData.InvalidAnsweredQuestions)
            {
                SetQuestionValidity(new ItemPublicKey(question.PublicKey, question.PropagationVector), false);
            }

            foreach (var answeredQuestion in interviewData.AnsweredQuestions)
            {
                var questionKey = new ItemPublicKey(answeredQuestion.Id, answeredQuestion.PropagationVector);
                SetAnswer(questionKey, answeredQuestion.Answer);
                SetComment(questionKey, answeredQuestion.Comments);
            }

            this.Chapters = questionnarie.Children.OfType<IGroup>().Select(
                c => this.Screens[new ItemPublicKey(c.PublicKey)]).OfType<QuestionnaireScreenViewModel>().ToList();

            this.Title = string.Format("{0} - {1}", questionnarie.Title,
                                       string.Join("",
                                                   questionnarie.Find<IQuestion>(q => q.Featured)
                                                                .Select(
                                                                    q =>
                                                                    this.Questions[new ItemPublicKey(q.PublicKey)]
                                                                        .AnswerString)));
        }

        protected void FillQuestionnairePostOrder(IGroup document)
        {
            List<IGroup> rout = new List<IGroup>();
            rout.Add(document);
            Stack<IGroup> queue = new Stack<IGroup>(document.Children.OfType<IGroup>());
            while (queue.Count > 0)
            {
                var current = queue.Pop();

                while (rout.Count > 0 && !rout[rout.Count - 1].Children.Contains(current))
                {
                    this.AddScreen(rout,rout.Last());
                    rout.RemoveAt(rout.Count - 1);
                }
                rout.Add(current);
                foreach (IGroup child in current.Children.OfType<IGroup>())
                {
                    queue.Push(child);
                }
            }
            var last = rout.Last();
            while (!(last is IQuestionnaireDocument))
            {
                AddScreen(rout,last);
                rout.Remove(last);
                last = rout.Last();
            }

            CreateNextPrevious();
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
        public InterviewStatus Status { get; set; }
        public IDictionary<ItemPublicKey, IQuestionnaireViewModel> Screens { get; protected set; }
        public IList<QuestionnaireScreenViewModel> Chapters { get; protected set; }

        protected TemplateCollection Templates { get; set; }
        protected IDictionary<ItemPublicKey, QuestionViewModel> Questions { get;  set; }
       
        #endregion


        #region public methods

        public void UpdatePropagateGroupsByTemplate(Guid publicKey, int[] outerScopePropagationVector, int count)
        {
            var propagatedGroupsCount = this.Screens.Keys.Count(id => id.PublicKey == publicKey);
            if (propagatedGroupsCount == count)
                return;
            for (int i = 0; i < Math.Abs(count - propagatedGroupsCount); i++)
            {
                if (propagatedGroupsCount > count)
                {
                    AddPropagateGroup(publicKey, outerScopePropagationVector, propagatedGroupsCount + i);
                }
                else
                {
                    RemovePropagatedGroup(publicKey, outerScopePropagationVector, propagatedGroupsCount - i);
                }
            }
        }

        private int[] BuildPropagationVectorForGroup(int[] outerScopePropagationVector, int index)
        {
            var newGroupVector = new int[outerScopePropagationVector.Length + 1];
            outerScopePropagationVector.CopyTo(newGroupVector, 0);
            newGroupVector[newGroupVector.Length - 1] = index;
            return newGroupVector;
        }

        private void AddPropagateGroup(Guid publicKey, int[] outerScopePropagationVector, int index)
        {
            var propagationVector = BuildPropagationVectorForGroup(outerScopePropagationVector,
                                                                     index);
            var template = this.Templates[publicKey];
            var screen = template.Clone(propagationVector);
            foreach (var question in screen.Items.OfType<QuestionViewModel>())
            {
                UpdateQuestionHash(question);
            }
            screen.PropertyChanged += screen_PropertyChanged;
            this.Screens.Add(screen.ScreenId, screen);
            UpdateGrid(publicKey);
        }

        private void RemovePropagatedGroup(Guid publicKey, int[] outerScopePropagationVector, int index)
        {
            var propagationVector = BuildPropagationVectorForGroup(outerScopePropagationVector,
                                                             index);
            var key = new ItemPublicKey(publicKey, propagationVector);
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
            if (question == null || !question.Capital || question.PublicKey.IsTopLevel)
                return;

            UpdatePropagatedGroupScreenName(question.PublicKey.PropagationVector);
        }

        private void UpdatePropagatedGroupScreenName(int[] propagationVector)
        {
            var screens =
                Screens.Select(
                    q => q.Value).OfType<QuestionnairePropagatedScreenViewModel>().Where(
                        q => q.ScreenId.CompareWithVector(propagationVector));
            var newTitle = string.Concat(
                Questions.Where(q => q.Key.CompareWithVector(propagationVector) && q.Value.Capital).
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
            if (!propagatedScreen.ScreenId.IsTopLevel)
                return;
            UpdateGrid(propagatedScreen.ScreenId.PublicKey);
        }

        #endregion

        #region protected helper methods

        protected void AddScreen(List<IGroup> rout, 
                            IGroup group)
        {
            var key = new ItemPublicKey(group.PublicKey);


            if (group.Propagated == Propagate.None)
            {
                var screenItems = BuildItems(group, true);
                var screen = new QuestionnaireScreenViewModel(PublicKey, group.Title, Title, true,
                                                              key, screenItems,
                                                              BuildSiblingsForNonPropagatedGroups(rout, key),
                                                              BuildBreadCrumbs(rout, key));
                this.Screens.Add(key, screen);
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
            if (question.Capital && !question.PublicKey.IsTopLevel)
            {
                question.PropertyChanged += question_PropertyChanged;
            }
        }

        protected void CreateGrid(IGroup group, List<IGroup> rout)
        {
            ItemPublicKey rosterKey = new ItemPublicKey(group.PublicKey, null);
            var siblings = BuildSiblingsForNonPropagatedGroups(rout, rosterKey);
            var screenItems = BuildItems(group, false);
            var breadcrumbs = BuildBreadCrumbs(rout, rosterKey);

            var roster = new QuestionnaireGridViewModel(PublicKey, group.Title, Title,
                                                        rosterKey, true,
                                                        siblings,
                                                        breadcrumbs,
                                                        // this.Chapters,
                                                        Enumerable.ToList<HeaderItem>(@group.Children
                                                                                            .OfType<IQuestion>()
                                                                                            .Where(
                                                                                                q =>
                                                                                                q.QuestionScope ==
                                                                                                QuestionScope
                                                                                                    .Interviewer)
                                                                                            .Select(
                                                                                                BuildHeader)),
                                                        () => CollectPropagatedScreen(rosterKey.PublicKey));

            breadcrumbs = breadcrumbs.Union(new ItemPublicKey[1] {roster.ScreenId}).ToList();
            var template = new QuestionnairePropagatedScreenViewModel(PublicKey, group.Title, true,
                                                                      rosterKey, screenItems,
                                                                      GetSiblings,
                                                                      breadcrumbs);
            Templates.Add(rosterKey.PublicKey, template);
            this.Screens.Add(rosterKey, roster);
        }

        protected IList<IQuestionnaireItemViewModel> BuildItems(IGroup screen, bool updateHash)
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
                this.Screens.Where(s => s.Key.PublicKey == publicKey && !s.Key.IsTopLevel).Select(
                    s => new ItemPublicKey(publicKey, s.Key.PropagationVector)).ToList();
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

        protected IList<ItemPublicKey> BuildBreadCrumbs(IList<IGroup> rout, ItemPublicKey key)
        {
            return
                rout.Skip(1).TakeWhile(r => r.PublicKey != key.PublicKey).Select(
                    r => new ItemPublicKey(r.PublicKey)).ToList();
        }

        protected IEnumerable<ItemPublicKey> BuildSiblingsForNonPropagatedGroups(IList<IGroup> rout,
                                                                                 ItemPublicKey key)
        {
            var parent = rout[rout.Count - 2];
            return parent.Children.OfType<IGroup>().Select(
                        g => new ItemPublicKey(g.PublicKey));
        }
    
        protected HeaderItem BuildHeader(IQuestion question)
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
            var question = item as IQuestion;

            if (question != null)
            {
                if (question.QuestionScope != QuestionScope.Interviewer || question.Featured)
                    return null;
                var newType = CalculateViewType(question.QuestionType);
                QuestionViewModel questionView;
                if (!IsTypeSelectable(newType))
                    questionView =
                        new ValueQuestionViewModel(
                            new ItemPublicKey(question.PublicKey), question.QuestionText,
                            newType,
                            null,
                            true, question.Instructions, null,
                            true, question.Capital, question.Mandatory,
                            question.ValidationMessage);
                else
                    questionView =
                        new SelectebleQuestionViewModel(
                            new ItemPublicKey(question.PublicKey), question.QuestionText,
                            newType, question.Answers.Select(
                                a =>
                                new AnswerViewModel(a.PublicKey, a.AnswerText, a.AnswerValue, false, a.AnswerImage))
                                             .ToList(),
                            true, question.Instructions, null,
                            true, question.Mandatory, question.Capital, null, question.ValidationMessage);

                var trigger = question as IAutoPropagateQuestion;
                if (trigger != null)
                {
                    Templates.AssignScope(item.PublicKey, trigger.Triggers);
                }
                return questionView;

            }

            var group = item as IGroup;
            if (group != null)
            {
                var key = new ItemPublicKey(group.PublicKey);
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
    }
}