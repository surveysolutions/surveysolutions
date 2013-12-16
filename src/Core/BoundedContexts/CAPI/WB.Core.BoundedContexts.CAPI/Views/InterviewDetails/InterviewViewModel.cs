using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Utility;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails.GridItems;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Capi.Views.InterviewDetails
{
    public class InterviewViewModel : MvxViewModel, IView
    {
        protected InterviewViewModel(Guid id)
        {
            this.PublicKey = id;
            this.Screens = new Dictionary<InterviewItemId, IQuestionnaireViewModel>();
            this.Questions = new Dictionary<InterviewItemId, QuestionViewModel>();
            this.FeaturedQuestions = new Dictionary<InterviewItemId, QuestionViewModel>();
        }

        public InterviewViewModel(Guid id, IQuestionnaireDocument questionnaire, QuestionnaireRosterStructure rosterStructure, InterviewSynchronizationDto interview)
            : this(id, questionnaire, rosterStructure)
        {
            #region interview data initialization
            this.Status = interview.Status;
            this.PropagateGroups(interview);
            this.SetAnswers(interview);
            this.DisableInterviewElements(interview);
            this.MarkAnswersAsInvalid(interview);
            #endregion
        }

        public InterviewViewModel(Guid id, IQuestionnaireDocument questionnaire, QuestionnaireRosterStructure rosterStructure)
            : this(id)
        {
            #region interview structure initialization

            this.rosterStructure = rosterStructure;

            this.BuildInterviewStructureFromTemplate(questionnaire);

            this.BuildHeadQuestionsInsidePropagateGroupsStructure(questionnaire);

            this.referencedQuestionToLinkedQuestionsMap = questionnaire
                .Find<IQuestion>(question => question.LinkedToQuestionId != null)
                .GroupBy(question => question.LinkedToQuestionId.Value)
                .ToDictionary(
                    keySelector: grouping => grouping.Key,
                    elementSelector: grouping => grouping.Select(question => question.PublicKey).ToArray());

            this.instancesOfAnsweredQuestionsUsableAsLinkedQuestionsOptions = new Dictionary<Guid, HashSet<InterviewItemId>>();

            this.SubscribeToQuestionAnswersForQuestionsWithSubstitutionReferences(this.GetAllQuestionsWithSubstitution());

            #endregion

            #region interview data initialization

            this.CreateInterviewChapters(questionnaire);

            this.CreateInterviewTitle(questionnaire);

            #endregion
        }
        
        private Dictionary<QuestionViewModel, IList<QuestionViewModel>> questionsParticipationInSubstitutionReferences =
            new Dictionary<QuestionViewModel, IList<QuestionViewModel>>();
        

        private void SubscribeToQuestionAnswersForQuestionsWithSubstitutionReferences(IEnumerable<QuestionViewModel> questionsToSubscribe)
        {
            foreach (var questionsWithSubstitution in questionsToSubscribe)
            {
                if (!questionsWithSubstitution.SubstitutionReferences.Any())
                    continue;

                foreach (var substitutionReference in questionsWithSubstitution.SubstitutionReferences)
                {
                    var referencedQuestion = this.FindReferencedQuestion(substitutionReference, questionsWithSubstitution);
                    if(referencedQuestion ==null)
                        continue;

                    if (this.questionsParticipationInSubstitutionReferences.ContainsKey(referencedQuestion))
                        this.questionsParticipationInSubstitutionReferences[referencedQuestion].Add(questionsWithSubstitution);
                    else
                        this.questionsParticipationInSubstitutionReferences.Add(referencedQuestion, new List<QuestionViewModel> { questionsWithSubstitution });

                    referencedQuestion.PropertyChanged += this.ReferencedQuestionPropertyChanged;
                    if (!string.IsNullOrEmpty(referencedQuestion.AnswerString))
                    {
                        this.ReferencedQuestionPropertyChanged(referencedQuestion, new PropertyChangedEventArgs("AnswerString"));
                    }
                }
            }
        }

        private void ReferencedQuestionPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != "AnswerString")
                return;

            var vm = sender as QuestionViewModel;
            if (vm == null)
                return;

            if (!this.questionsParticipationInSubstitutionReferences.ContainsKey(vm))
                return;

            foreach (var participationQuestion in this.questionsParticipationInSubstitutionReferences[vm])
            {
                participationQuestion.SubstituteQuestionText(vm);
            }
        }

        private QuestionViewModel FindReferencedQuestion(string variableName, QuestionViewModel subscribedQuestion)
        {
            var preFilledQuestion = this.FeaturedQuestions.Values.FirstOrDefault(q => q.Variable == variableName);
            if (preFilledQuestion != null)
                return preFilledQuestion;

            return this.Questions.Values.FirstOrDefault(question => question.Variable == variableName &&
                this.PropagationVectorIsTheSameOrHigher(subscribedQuestion, question));
        }

#warning nastya_k "subscribedQuestion.PublicKey.PropagationVector.Length > referencedQuestion.PublicKey.PropagationVector.Length" is not correct check for higher propagation level
        private bool PropagationVectorIsTheSameOrHigher(QuestionViewModel subscribedQuestion, QuestionViewModel referencedQuestion)
        {
            if (referencedQuestion.PublicKey.CompareWithVector(subscribedQuestion.PublicKey.PropagationVector))
                return true;
            return subscribedQuestion.PublicKey.PropagationVector.Length > referencedQuestion.PublicKey.PropagationVector.Length;
        }

        private IEnumerable<QuestionViewModel> GetAllQuestionsWithSubstitution()
        {
            return this.Questions.Values.Where(x => x.SubstitutionReferences.Any());
        }

        private void BuildHeadQuestionsInsidePropagateGroupsStructure(IQuestionnaireDocument questionnaire)
        {
            foreach (var propagatedGroup in questionnaire.Find<IGroup>(group => group.Propagated != Propagate.None || group.IsRoster))
            {
                var scopeOfPropagationId = this.GetScopeOfPropagatedScreen(propagatedGroup.PublicKey);
                foreach (var headQuestion in propagatedGroup.Find<IQuestion>(question => question.Capital))
                {
                    this.listOfHeadQuestionsMappedOnScope.Add(headQuestion.PublicKey, scopeOfPropagationId);
                }
            }
        }

        public Guid GetScopeOfPropagatedScreen(Guid itemKey)
        {
            var itemScope = this.rosterStructure.RosterScopes.FirstOrDefault(s => s.Value.Contains(itemKey));
            if (itemScope.Equals(default(KeyValuePair<Guid, HashSet<Guid>>)))
                throw new ArgumentException("item is absent in any scope");
            return itemScope.Key;
        }

        private void SetAnswers(InterviewSynchronizationDto interview)
        {
            foreach (var answeredQuestion in interview.Answers)
            {
                var questionKey = new InterviewItemId(answeredQuestion.Id, answeredQuestion.PropagationVector);
                if (this.Questions.ContainsKey(questionKey))
                {
                    this.SetAnswer(questionKey, answeredQuestion.Answer);
                    this.SetComment(questionKey, answeredQuestion.Comments);
                }
                else if (this.FeaturedQuestions.ContainsKey(questionKey))
                {
                    var question = this.FeaturedQuestions[questionKey];
                    question.SetAnswer(answeredQuestion.Answer);
                }

                if (!IsQuestionReferencedByAnyLinkedQuestion(answeredQuestion.Id))
                    continue;
                AddInstanceOfAnsweredQuestionUsableAsLinkedQuestionsOption(answeredQuestion.Id, answeredQuestion.PropagationVector);
            }
        }

        private void MarkAnswersAsInvalid(InterviewSynchronizationDto interview)
        {
            foreach (var question in interview.InvalidAnsweredQuestions)
            {
                this.SetQuestionValidity(new InterviewItemId(question.Id, question.PropagationVector), false);
            }
        }

        private void DisableInterviewElements(InterviewSynchronizationDto interview)
        {
            foreach (var group in interview.DisabledGroups)
            {
                this.SetScreenStatus(new InterviewItemId(@group.Id, @group.PropagationVector), false);
            }

            foreach (var question in interview.DisabledQuestions)
            {
                this.SetQuestionStatus(new InterviewItemId(question.Id, question.PropagationVector), false);
            }
        }

        private void PropagateGroups(InterviewSynchronizationDto interview)
        {
            foreach (var propagatedGroupInstanceCount in interview.PropagatedGroupInstanceCounts)
            {
                int index = 0;
                foreach (var rosterInstanceId in propagatedGroupInstanceCount.Value)
                {
                    this.AddPropagateScreen(propagatedGroupInstanceCount.Key.Id,
                       propagatedGroupInstanceCount.Key.PropagationVector, rosterInstanceId, index);
                    index++;
                }
            }
        }

        private void CreateInterviewChapters(IQuestionnaireDocument questionnarie)
        {
            this.Chapters = questionnarie.Children.OfType<IGroup>().Select(
                c => this.Screens[new InterviewItemId(c.PublicKey)]).OfType<QuestionnaireScreenViewModel>().ToList();
        }

        private void CreateInterviewTitle(IQuestionnaireDocument questionnarie)
        {
            string featuredTitle = "";
            foreach (var questionViewModel in this.FeaturedQuestions)
            {
                featuredTitle += string.Format("| {0} ", questionViewModel.Value.AnswerString);
            }

            this.Title = string.Format("{0} {1}", questionnarie.Title, featuredTitle);
        }

        protected void BuildInterviewStructureFromTemplate(IGroup document)
        {
            List<IGroup> rout = new List<IGroup>();
            rout.Add(document);
            Stack<IGroup> queue = new Stack<IGroup>(document.Children.OfType<IGroup>());
            while (queue.Count > 0)
            {
                var current = queue.Pop();

                while (rout.Count > 0 && !rout[rout.Count - 1].Children.Contains(current))
                {
                    this.AddScreen(rout, rout.Last());
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
                this.AddScreen(rout, last);
                rout.Remove(last);
                last = rout.Last();
            }

            this.CreateNextPreviousButtonsForPropagatedGroups();
        }

        protected void CreateNextPreviousButtonsForPropagatedGroups()
        {
            foreach (var propagationScopeId in this.rosterStructure.RosterScopes.Keys)
            {
                var screenSiblingByPropagationScopeIds = this.rosterStructure.RosterScopes[propagationScopeId].ToArray();

                for (int i = 0; i < screenSiblingByPropagationScopeIds.Length; i++)
                {
                    var target = this.propagatedScreenPrototypes[screenSiblingByPropagationScopeIds[i]];

                    IQuestionnaireItemViewModel next = null;
                    IQuestionnaireItemViewModel previous = null;

                    if (i > 0)
                    {
                        var item = this.propagatedScreenPrototypes[screenSiblingByPropagationScopeIds[i - 1]];
                        previous = new QuestionnaireNavigationPanelItem(item.ScreenId, item);
                    }
                    if (i < screenSiblingByPropagationScopeIds.Length - 1)
                    {
                        var item = this.propagatedScreenPrototypes[screenSiblingByPropagationScopeIds[i + 1]];
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
        public IDictionary<InterviewItemId, IQuestionnaireViewModel> Screens { get; protected set; }
        public IList<QuestionnaireScreenViewModel> Chapters { get; protected set; }

        
        protected IDictionary<InterviewItemId, QuestionViewModel> Questions { get; set; }
        private readonly Dictionary<Guid, HashSet<InterviewItemId>> instancesOfAnsweredQuestionsUsableAsLinkedQuestionsOptions;
        private readonly Dictionary<Guid, Guid> listOfHeadQuestionsMappedOnScope = new Dictionary<Guid, Guid>(); 
        private readonly Dictionary<Guid, Guid[]> referencedQuestionToLinkedQuestionsMap;
        private readonly QuestionnaireRosterStructure rosterStructure;
        private readonly Dictionary<Guid, QuestionnairePropagatedScreenViewModel> propagatedScreenPrototypes =
            new Dictionary<Guid, QuestionnairePropagatedScreenViewModel>();

        public IDictionary<InterviewItemId, QuestionViewModel> FeaturedQuestions { get;private set; }

        #endregion

        public void UpdatePropagateGroupsByTemplate(Guid publicKey, decimal[] outerScopePropagationVector, int count)
        {
            var propagatedGroupsCount = this.Screens.Keys.Count(id => id.Id == publicKey) - 1;
            if (propagatedGroupsCount == count)
                return;

            for (int i = 0; i < Math.Abs(count - propagatedGroupsCount); i++)
            {
                if (propagatedGroupsCount < count)
                {
                    var rosterInstanceId = propagatedGroupsCount + i;
                    this.AddPropagateScreen(publicKey, outerScopePropagationVector, rosterInstanceId, rosterInstanceId);
                }
                else
                {
                    this.RemovePropagatedScreen(publicKey, outerScopePropagationVector, propagatedGroupsCount - i - 1);
                }
            }
        }

        private decimal[] BuildPropagationVectorForGroup(decimal[] outerScopePropagationVector, decimal index)
        {
            var newGroupVector = new decimal[outerScopePropagationVector.Length + 1];
            outerScopePropagationVector.CopyTo(newGroupVector, 0);
            newGroupVector[newGroupVector.Length - 1] = index;
            return newGroupVector;
        }

        public void AddPropagateScreen(Guid screenId, decimal[] outerScopePropagationVector, decimal rosterInstanceId, int? sortIndex)
        {
            var propagationVector = this.BuildPropagationVectorForGroup(outerScopePropagationVector,
                rosterInstanceId);

            var screenPrototype = this.propagatedScreenPrototypes[screenId];
            var screen = screenPrototype.Clone(propagationVector, sortIndex);

            var questions = screen.Items.OfType<QuestionViewModel>().ToList();

            foreach (var question in questions)
            {
                this.UpdateQuestionHash(question);
            }

            this.SubscribeToQuestionAnswersForQuestionsWithSubstitutionReferences(questions);

            screen.PropertyChanged += screen_PropertyChanged;
            this.Screens.Add(screen.ScreenId, screen);
            this.UpdateGrid(screenId);
        }

        public void RemovePropagatedScreen(Guid screenId, decimal[] outerScopePropagationVector, decimal index)
        {
            var propagationVector = this.BuildPropagationVectorForGroup(outerScopePropagationVector,
                index);
            var key = new InterviewItemId(screenId, propagationVector);
            var screen = this.Screens[key] as QuestionnaireScreenViewModel;
            foreach (var item in screen.Items)
            {
                var question = item as QuestionViewModel;
                if (question != null)
                {
                    this.Questions.Remove(question.PublicKey);

                    this.CleanQuestionsParticipationInSubstitutionReferencesBySubscribedQuestion(question);
                }
            }
            this.Screens.Remove(key);
            this.UpdateGrid(screenId);

        }

        private void CleanQuestionsParticipationInSubstitutionReferencesBySubscribedQuestion(QuestionViewModel question)
        {
            var allQuestionsSubstitutedAtQuestionText =
                this.questionsParticipationInSubstitutionReferences.Where(q => q.Value.Contains(question));

            foreach (var referencedQuestionWithSubscribers in allQuestionsSubstitutedAtQuestionText)
            {
                referencedQuestionWithSubscribers.Value.Remove(question);
                if (referencedQuestionWithSubscribers.Value.Count == 0)
                    referencedQuestionWithSubscribers.Key.PropertyChanged -= ReferencedQuestionPropertyChanged;
            }
        }

        public IEnumerable<IQuestionnaireViewModel> RestoreBreadCrumbs(IEnumerable<InterviewItemId> breadcrumbs)
        {
            return breadcrumbs.Select(b => this.Screens[b]);
        }

        public void SetAnswer(InterviewItemId key, object answer)
        {
            QuestionViewModel question = null;
            if (this.Questions.ContainsKey(key))
                question = this.Questions[key];
            else if (this.FeaturedQuestions.ContainsKey(key))
                question = this.FeaturedQuestions[key];
            else
                return;

            question.SetAnswer(answer);
        }

        public void RemoveAnswer(InterviewItemId questionInstanceId)
        {
            if (!this.Questions.ContainsKey(questionInstanceId))
                return;

            QuestionViewModel question = this.Questions[questionInstanceId];

            question.RemoveAnswer();
        }

        public void SetComment(InterviewItemId key, string comment)
        {
            var question =
                this.Questions[key];
            question.SetComment(comment);
        }

        public void SetQuestionStatus(InterviewItemId key, bool enebled)
        {
            if (!this.Questions.ContainsKey(key))
                return;

            var question =
                this.Questions[key];
            question.SetEnabled(enebled);
        }

        public void SetQuestionValidity(InterviewItemId key, bool valid)
        {
            if (!this.Questions.ContainsKey(key))
                return;

            var question =
                this.Questions[key];
            question.SetValid(valid);
        }

        public void SetScreenStatus(InterviewItemId key, bool enabled)
        {
            var screen =
                this.Screens[key];
            screen.SetEnabled(enabled);

            var plainScreen = screen as QuestionnaireScreenViewModel;
            if (plainScreen == null)
                return;

            foreach (var child in plainScreen.Items)
            {
                var question = child as QuestionViewModel;
                if (question != null)
                {
                    question.SetParentEnabled(enabled);
                }
            }
        }

        public void UpdateRosterTitle(Guid questionId, decimal[] propagationVector, string rosterTitle)
        {
            if (this.listOfHeadQuestionsMappedOnScope.ContainsKey(questionId))
            {
                this.UpdatePropagationScopeTitleForVector(this.listOfHeadQuestionsMappedOnScope[questionId],
                propagationVector);
            }
        }

        public IEnumerable<QuestionViewModel> FindQuestion(Func<QuestionViewModel, bool> filter)
        {
            return this.Questions.Select(q => q.Value).Where(filter);
        }

        public void AddInstanceOfAnsweredQuestionUsableAsLinkedQuestionsOption(Guid questionId, decimal[] propagationVector)
        {
            if (!this.instancesOfAnsweredQuestionsUsableAsLinkedQuestionsOptions.ContainsKey(questionId))
                this.instancesOfAnsweredQuestionsUsableAsLinkedQuestionsOptions.Add(questionId, new HashSet<InterviewItemId>());

            var questionInstanceId = new InterviewItemId(questionId, propagationVector);

            this.instancesOfAnsweredQuestionsUsableAsLinkedQuestionsOptions[questionId].Add(questionInstanceId);

            this.NotifyAffectedLinkedQuestions(questionId);
        }

        public void RemoveInstanceOfAnsweredQuestionUsableAsLinkedQuestionsOption(Guid questionId, decimal[] propagationVector)
        {
            if (!this.instancesOfAnsweredQuestionsUsableAsLinkedQuestionsOptions.ContainsKey(questionId))
                return;

            var questionInstanceId = new InterviewItemId(questionId, propagationVector);

            this.instancesOfAnsweredQuestionsUsableAsLinkedQuestionsOptions[questionId].Remove(questionInstanceId);

            this.NotifyAffectedLinkedQuestions(questionId);
        }

        private void NotifyAffectedLinkedQuestions(Guid referencedQuestionId)
        {
            this.referencedQuestionToLinkedQuestionsMap[referencedQuestionId]
                .SelectMany(
                    linkedQuestionId =>
                        this.Questions.Values.OfType<LinkedQuestionViewModel>()
                            .Where(questionViewModel => questionViewModel.PublicKey.Id == linkedQuestionId))
                .ToList()
                .ForEach(linkedQuestionViewModel => linkedQuestionViewModel.HandleAnswerListChange());
        }

        private void HeadQuestionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "AnswerString")
                return;

            var question = sender as QuestionViewModel;
            if (question==null || !this.listOfHeadQuestionsMappedOnScope.ContainsKey(question.PublicKey.Id))
                return;

            this.UpdatePropagationScopeTitleForVector(this.listOfHeadQuestionsMappedOnScope[question.PublicKey.Id],
                question.PublicKey.PropagationVector);
        }

        private void UpdatePropagationScopeTitleForVector(Guid scopeId, decimal[] propagationVector)
        {
            var siblingsByPropagationScopeIds = this.rosterStructure.RosterScopes[scopeId];

            var screensSiblingByPropagationScopeWithVector =
                siblingsByPropagationScopeIds.Select(
                    screenId => this.Screens[new InterviewItemId(screenId, propagationVector)] as QuestionnairePropagatedScreenViewModel);

            var newTitle = this.BuildPropagationScopeTitleBasedOnAnswersToCapitalQuestions(propagationVector, scopeId);

            foreach (var screen in screensSiblingByPropagationScopeWithVector)
            {
                screen.UpdateScreenName(newTitle);
            }
        }

        private string BuildPropagationScopeTitleBasedOnAnswersToCapitalQuestions(decimal[] propagationVector, Guid scopeId)
        {
            return string.Concat(
                this.Questions.Where(
                    q =>
                        q.Key.CompareWithVector(propagationVector) &&
                            this.listOfHeadQuestionsMappedOnScope.Any(
                                headQuestion => headQuestion.Key == q.Key.Id && headQuestion.Value == scopeId)).
                    Select(
                        q => q.Value.AnswerString));
        }

        private void screen_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var propagatedScreen = sender as QuestionnaireScreenViewModel;
            if (propagatedScreen == null)
                return;
            if (propagatedScreen.ScreenId.IsTopLevel())
                return;
            this.UpdateGrid(propagatedScreen.ScreenId.Id);
        }

        protected void AddScreen(List<IGroup> rout,
            IGroup group)
        {
            var key = new InterviewItemId(group.PublicKey);


            if (group.Propagated == Propagate.None && !group.IsRoster)
            {
                var screenItems = this.BuildItems(group, true);
                var screen = new QuestionnaireScreenViewModel(this.PublicKey, group.Title, this.Title, true,
                    key, screenItems,
                    this.BuildSiblingsForNonPropagatedGroups(rout, key),
                    this.BuildBreadCrumbs(rout, key));
                this.Screens.Add(key, screen);
            }
            else
            {
                var gridKey = new InterviewItemId(group.PublicKey);
                if (!this.Screens.ContainsKey(gridKey))
                {
                    this.CreateGrid(group, rout);
                }
            }

        }

        protected void UpdateQuestionHash(QuestionViewModel question)
        {
            this.Questions.Add(question.PublicKey, question);

            if (this.listOfHeadQuestionsMappedOnScope.ContainsKey(question.PublicKey.Id))
            {
                question.PropertyChanged += this.HeadQuestionPropertyChanged;
            }
        }

        protected void CreateGrid(IGroup group, List<IGroup> root)
        {
            InterviewItemId rosterKey = new InterviewItemId(group.PublicKey);
            var siblings = this.BuildSiblingsForNonPropagatedGroups(root, rosterKey);
            var screenItems = this.BuildItems(group, false);
            var breadcrumbs = this.BuildBreadCrumbs(root, rosterKey);

            var roster = new QuestionnaireGridViewModel(this.PublicKey, group.Title, this.Title,
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
                        this.BuildHeader)),
                () => this.CollectPropagatedScreen(rosterKey.Id));

            breadcrumbs = breadcrumbs.Union(new InterviewItemId[1] {roster.ScreenId}).ToList();

            var screenPrototype = new QuestionnairePropagatedScreenViewModel(this.PublicKey, group.Title, true,
                rosterKey, screenItems,
                this.GetSiblings,
                breadcrumbs, -1);

            this.propagatedScreenPrototypes.Add(rosterKey.Id, screenPrototype);

            this.Screens.Add(rosterKey, roster);
        }

        protected IList<IQuestionnaireItemViewModel> BuildItems(IGroup screen, bool updateHash)
        {
            IList<IQuestionnaireItemViewModel> result = new List<IQuestionnaireItemViewModel>();
            foreach (var child in screen.Children)
            {
                var item = this.CreateView(child);
                if (item == null)
                    continue;
                var question = item as QuestionViewModel;
                if (question != null && updateHash)
                    this.UpdateQuestionHash(question);
                result.Add(item);
            }
            return result;
        }

        protected IEnumerable<InterviewItemId> GetSiblings(Guid publicKey)
        {
            return
                this.Screens.Where(s => s.Key.Id == publicKey && !s.Key.IsTopLevel()).Select(
                    s => new InterviewItemId(publicKey, s.Key.PropagationVector)).ToList();
        }

        protected void UpdateGrid(Guid key)
        {
            var gridkey = new InterviewItemId(key);
            var grid = this.Screens[gridkey] as QuestionnaireGridViewModel;
            if (grid != null)
                grid.UpdateCounters();
        }

        protected IEnumerable<QuestionnairePropagatedScreenViewModel> CollectPropagatedScreen(Guid publicKey)
        {
            return this.Screens
                .Select(s => s.Value)
                .OfType<QuestionnairePropagatedScreenViewModel>()
                .Where(s => s.ScreenId.Id == publicKey)
                .OrderBy(x => x.SortIndex)
                .ToList();
        }

        protected IList<InterviewItemId> BuildBreadCrumbs(IList<IGroup> rout, InterviewItemId key)
        {
            return
                rout.Skip(1).TakeWhile(r => r.PublicKey != key.Id).Select(
                    r => new InterviewItemId(r.PublicKey)).ToList();
        }

        protected IEnumerable<InterviewItemId> BuildSiblingsForNonPropagatedGroups(IList<IGroup> rout,
            InterviewItemId key)
        {
            var parent = rout[rout.Count - 2];
            return parent.Children.OfType<IGroup>().Select(
                g => new InterviewItemId(g.PublicKey));
        }

        protected HeaderItem BuildHeader(IQuestion question)
        {
            string text = question.GetVariablesUsedInTitle().Aggregate(question.QuestionText, (current, substitutionVariable) => current.ReplaceSubstitutionVariable(substitutionVariable, StringUtil.DefaultSubstitutionText));
            return new HeaderItem(question.PublicKey, text, question.Instructions);
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
                if (this.IfQuestionNeedToBeSkipped(question))
                    return null;

                QuestionViewModel questionView = this.CreateQuestionView(question);

                if (question.Featured)
                {
                    this.FeaturedQuestions.Add(questionView.PublicKey, questionView);
                    return null;
                }

                return questionView;
            }

            var group = item as IGroup;
            if (group != null)
            {
                var key = new InterviewItemId(group.PublicKey);
                return
                    new QuestionnaireNavigationPanelItem(
                        key, this.Screens[key]);
            }
            return null;
        }

        private bool IfQuestionNeedToBeSkipped(IQuestion question)
        {
            return question.QuestionScope != QuestionScope.Interviewer && !question.Featured;
        }

        private QuestionViewModel CreateQuestionView(IQuestion question)
        {
            var newType = this.CalculateViewType(question.QuestionType);
            if (this.selectableQuestionTypes.Contains(question.QuestionType))
            {
                if (question.LinkedToQuestionId.HasValue)
                    return this.CreateLinkedQuestion(question, newType);
                return this.CreateSelectableQuestion(question, newType);
            }
            return this.CreateValueQuestion(question, newType);
        }

        private ValueQuestionViewModel CreateValueQuestion(IQuestion question, QuestionType newType)
        {
            return new ValueQuestionViewModel(
                new InterviewItemId(question.PublicKey), question.QuestionText,
                newType,
                null,
                true, question.Instructions, null,
                true, question.Mandatory,
                question.ValidationMessage,
                question.StataExportCaption,
                question.GetVariablesUsedInTitle(),
                this.GetQuestionsIsIntegerSetting(question), this.GetQuestionsDecimalPlacesSetting(question));
        }

        private bool? GetQuestionsIsIntegerSetting(IQuestion question)
        {
            var numericQuestion = question as NumericQuestion;
            if (numericQuestion == null)
                return null;
            return numericQuestion.IsInteger;
        }

        private int? GetQuestionsDecimalPlacesSetting(IQuestion question)
        {
            var numericQuestion = question as NumericQuestion;
            if (numericQuestion == null)
                return null;
            return numericQuestion.CountOfDecimalPlaces;
        }

        private LinkedQuestionViewModel CreateLinkedQuestion(IQuestion question, QuestionType newType)
        {
            var multyOptionsQuestion = question as MultyOptionsQuestion;

            return new LinkedQuestionViewModel(
                new InterviewItemId(question.PublicKey), question.QuestionText,
                newType,
                true, question.Instructions,
                true, question.Mandatory,question.ValidationMessage,
                () => this.GetAnswerOptionsForLinkedQuestion(question.LinkedToQuestionId.Value), 
                question.StataExportCaption, question.GetVariablesUsedInTitle(),
                multyOptionsQuestion != null? multyOptionsQuestion.AreAnswersOrdered : (bool?) null, 
                multyOptionsQuestion != null ? multyOptionsQuestion.MaxAllowedAnswers : null);
        }

        private SelectebleQuestionViewModel CreateSelectableQuestion(IQuestion question, QuestionType newType)
        {
            var multyOptionsQuestion = question as MultyOptionsQuestion;

            return new SelectebleQuestionViewModel(
                new InterviewItemId(question.PublicKey), question.QuestionText,
                newType, question.Answers.Select(
                    a => new AnswerViewModel(a.PublicKey, a.AnswerText, a.AnswerValue, false, a.AnswerImage)).ToList(),
                true, question.Instructions, null,
                true, question.Mandatory,  null, question.ValidationMessage, question.StataExportCaption, question.GetVariablesUsedInTitle(),
                multyOptionsQuestion != null ? multyOptionsQuestion.AreAnswersOrdered : (bool?)null,
                multyOptionsQuestion != null ? multyOptionsQuestion.MaxAllowedAnswers : null);
        }

        protected IEnumerable<LinkedAnswerViewModel> GetAnswerOptionsForLinkedQuestion(Guid referencedQuestionId)
        {
            return !this.instancesOfAnsweredQuestionsUsableAsLinkedQuestionsOptions.ContainsKey(referencedQuestionId)
                ? Enumerable.Empty<LinkedAnswerViewModel>()
                : this
                    .instancesOfAnsweredQuestionsUsableAsLinkedQuestionsOptions[referencedQuestionId]
                    .Select(instanceId => this.Questions.ContainsKey(instanceId) ? this.Questions[instanceId] : null)
                    .Where(questionInstance => questionInstance != null && questionInstance.IsEnabled())
                    .Select(
                        questionInstance =>
                            new LinkedAnswerViewModel(questionInstance.PublicKey.PropagationVector, questionInstance.AnswerString));
        }

        protected QuestionType CalculateViewType(QuestionType questionType)
        {
            if (this.singleOptionTypeVariation.Contains(questionType))
                return QuestionType.SingleOption;

            return questionType;
        }

        private readonly QuestionType[] selectableQuestionTypes = new[]
            {QuestionType.SingleOption, QuestionType.MultyOption};

        private readonly QuestionType[] singleOptionTypeVariation = new[]
            {QuestionType.SingleOption, QuestionType.DropDownList, QuestionType.YesNo};

        public bool IsQuestionReferencedByAnyLinkedQuestion(Guid questionId)
        {
            return this.referencedQuestionToLinkedQuestionsMap.ContainsKey(questionId)
                && this.referencedQuestionToLinkedQuestionsMap[questionId].Any();
        }
    }
}