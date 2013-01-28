using System;
using System.Collections.Generic;
using System.Linq;
using AndroidApp.ViewModel.QuestionnaireDetails.GridItems;
using AndroidApp.ViewModel.QuestionnaireDetails.Validation;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;

namespace AndroidApp.ViewModel.QuestionnaireDetails
{
    public class CompleteQuestionnaireView
    {
        public CompleteQuestionnaireView(CompleteQuestionnaireDocument document)
        {
            this.PublicKey = document.PublicKey;
            this.Title = document.Title;
            Screens = new Dictionary<ItemPublicKey, IQuestionnaireViewModel>();
            Questions = new Dictionary<ItemPublicKey, QuestionViewModel>();
            Templates = new Dictionary<Guid, QuestionnaireScreenViewModel>();

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
                this.AddScreen(rout, current);

                foreach (ICompleteGroup child in current.Children.OfType<ICompleteGroup>())
                {
                    queue.Push(child);
                }
            }
            this.Chapters =
                document.Children.OfType<ICompleteGroup>().Select(
                    c => this.Screens[new ItemPublicKey(c.PublicKey, c.PropagationPublicKey)]).OfType<QuestionnaireScreenViewModel>().ToList();
            /*    this.Screens.Where(s => document.Children.Any(c => c.PublicKey == s.Key.PublicKey)).Select(s => s.Value)
                    .OfType<QuestionnaireScreenViewModel>().ToList();*/
           
            this.validator = new QuestionnaireValidationExecutor(this);
        }
        public void SetAnswer(ItemPublicKey key, List<Guid> answerKeys, string answerString )
        {
            var question =
              this.Questions[key];
            question.SetAnswer(answerKeys, answerString);
            this.validator.Execute();
        }
        public void SetComment(ItemPublicKey key, string comment)
        {
            var question =
              this.Questions[key];
            question.SetComment(comment);
        }
        public void SetQuestionStatus(ItemPublicKey key, bool enebled)
        {
            var question =
                this.Questions[key];
            question.SetEnabled(enebled);
        }
        public IEnumerable<QuestionViewModel> FindQuestion(Func<QuestionViewModel, bool> filter)
        {
            return this.Questions.Select(q => q.Value).Where(filter);
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
                                                              BuildSiblingsForNonPropagatedGroups(rout, key),
                                                              BuildBreadCrumbs(rout, key),
                                                              () => this.Chapters,true);
                this.Screens.Add(key, screen);
            }
            else if (group.PropagationPublicKey.HasValue)
            {
                var screenItems = BuildItems(group, true);
                var screen = new QuestionnaireScreenViewModel(PublicKey, () => GetPropagatebleGroupTitle(group.PropagationPublicKey.Value), group.Title,
                                                              key, screenItems,
                                                              () => GetSiblings(key.PublicKey),
                                                              BuildBreadCrumbs(rout, key),
                                                              () => this.Chapters,true);
                this.Screens.Add(key, screen);
            }
            else
            {

                CreateGrid(group, rout);
            }

        }
        protected void UpdateQuestionHash(QuestionViewModel question)
        {
            this.Questions.Add(question.PublicKey, question);
            question.PropertyChanged += question_PropertyChanged;
        }

        void question_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            
            if (e.PropertyName != "Status")
                return;
            var question = sender as QuestionViewModel;
            if(question==null)
                return;
            var screen =
                this.Screens.Select(s => s.Value).OfType<QuestionnaireScreenViewModel>().FirstOrDefault(s => s.Items.Any(i => i.PublicKey == question.PublicKey));
            if(screen==null)
                return;
            var breadcrumbs = screen.Breadcrumbs.ToList();
            for (int i = breadcrumbs.Count - 1; i >= 0; i--)
            {
                breadcrumbs[i].UpdateCounters();
            }
        }
        protected void CreateGrid(ICompleteGroup group, List<ICompleteGroup> rout)
        {
            ItemPublicKey rosterKey = new ItemPublicKey(group.PublicKey, null);
            var siblings = BuildSiblingsForNonPropagatedGroups(rout, rosterKey);
            var screenItems = BuildItems(group, false);
            var breadcrumbs = BuildBreadCrumbs(rout, rosterKey);
           
            var roster = new QuestionnaireGridViewModel(PublicKey, group.Title, Title,
                                                                  rosterKey,
                                                                  siblings,
                                                                  breadcrumbs,
                                                                  () => this.Chapters,
                                                                  group.Children.OfType<ICompleteQuestion>().Select(
                                                                      BuildHeader).ToList(),
                                                                  () => CollectPropagatedScreen(rosterKey.PublicKey));

            breadcrumbs = breadcrumbs.Union(new IQuestionnaireViewModel[1] { roster }).ToList();
            var template = new QuestionnaireScreenViewModel(PublicKey, group.Title, Title,
                                                           rosterKey, screenItems,
                                                           siblings,
                                                           breadcrumbs,
                                                           () => this.Chapters, false);
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
                    UpdateQuestionHash( question);
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
                    UpdateQuestionHash( newQuestion);
              
            }
            var screen = new QuestionnaireScreenViewModel(PublicKey,
                                                          () => GetPropagatebleGroupTitle(propagationKey), template.Title,
                                                          key, items,
                                                          () => GetSiblings(key.PublicKey), bradCrumbs,
                                                          () => this.Chapters,true);

            this.Screens.Add(key, screen);
        }

        protected IEnumerable<ItemPublicKey> GetSiblings(Guid publicKey)
        {
            return
                this.Screens.Where(s => s.Key.PublicKey == publicKey && s.Key.PropagationKey.HasValue).Select(
                    s => new ItemPublicKey(publicKey, s.Key.PropagationKey)).ToList();
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
        protected IDictionary<Guid, QuestionnaireScreenViewModel> Templates { get; private set; }
        protected IDictionary<ItemPublicKey, QuestionViewModel> Questions { get; private set; }
        public IEnumerable<QuestionnaireScreenViewModel> Chapters { get; private set; }
        private readonly QuestionnaireValidationExecutor validator;
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

        protected IList<IQuestionnaireViewModel> BuildBreadCrumbs(IList<ICompleteGroup> rout, ItemPublicKey key)
        {
            return
                rout.Skip(1).TakeWhile(r => r.PublicKey != key.PublicKey).Select(
                    r => this.Screens[new ItemPublicKey(r.PublicKey, r.PropagationPublicKey)]).ToList();
        }

        protected IEnumerable<ItemPublicKey> BuildSiblingsForNonPropagatedGroups(IList<ICompleteGroup> rout, ItemPublicKey key)
        {
            var parent = rout[rout.Count - 2];
            return
                parent.Children.OfType<ICompleteGroup>().Distinct(new PropagatedGroupEqualityComparer()).Select(g => new ItemPublicKey(g.PublicKey, g.PropagationPublicKey)).ToList();
         //   return result.Select(r => Screens[new ItemPublicKey(r.PublicKey, r.PropagationPublicKey)]).ToList();
            //    return result.Select(BuildNavigationItem).ToList();
        }

        protected QuestionnaireNavigationPanelItem BuildNavigationItem(ICompleteGroup g)
        {
            var key = new ItemPublicKey(g.PublicKey, g.PropagationPublicKey);
            return new QuestionnaireNavigationPanelItem(key, g.Title, 0, 0, g.Enabled, () => this.Screens[key]);
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
                if (question.QuestionScope != QuestionScope.Interviewer)
                    return null;
                var newType = CalculateViewType(question.QuestionType);
                QuestionViewModel questionView;
                if (!IsTypeSelectable(newType))
                    questionView= new ValueQuestionViewModel(new ItemPublicKey(question.PublicKey, question.PropagationPublicKey), question.QuestionText,
                                                      newType,
                                                      question.GetAnswerString(),
                                                      question.Enabled, question.Instructions, question.Comments,
                                                      question.Valid,question.Capital, question.Mandatory,question.ValidationExpression,question.ValidationMessage);
                else
                    questionView= new SelectebleQuestionViewModel(new ItemPublicKey(question.PublicKey, question.PropagationPublicKey), question.QuestionText,
                                                           newType,
                                                           question.Answers.OfType<ICompleteAnswer>().Select(
                                                               a =>
                                                               new AnswerViewModel(a.PublicKey, a.AnswerText,a.AnswerValue, a.Selected)).ToList(),
                                                           question.Enabled, question.Instructions, question.Comments,
                                                           question.Valid, question.Mandatory, question.Capital, question.GetAnswerString(), question.ValidationExpression, question.ValidationMessage);
              //  questionView.PropertyChanged += questionView_PropertyChanged;
                return questionView;

            }
            var group = item as ICompleteGroup;
            if (group != null && !group.PropagationPublicKey.HasValue)
            {
                var key = new ItemPublicKey(group.PublicKey, group.PropagationPublicKey);
                return
                    new QuestionnaireNavigationPanelItem(
                        key, group.Title, 0, 0,
                        group.Enabled, () => this.Screens[key]);
            }
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