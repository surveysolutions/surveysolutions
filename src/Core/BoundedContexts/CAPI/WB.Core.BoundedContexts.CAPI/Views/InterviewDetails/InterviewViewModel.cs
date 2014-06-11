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
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails.GridItems;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.ExpressionProcessor;
using WB.Core.SharedKernels.ExpressionProcessor.Implementation.Services;
using WB.Core.SharedKernels.ExpressionProcessor.Services;

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

        protected ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }

        protected ISubstitutionService SubstitutionService
        {
            get { return ServiceLocator.Current.GetInstance<ISubstitutionService>(); }
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
            this.FireSubstitutionEventsForPrefilledQuestions();
            #endregion
        }

        public InterviewViewModel(Guid id, IQuestionnaireDocument questionnaire, QuestionnaireRosterStructure rosterStructure)
            : this(id)
        {
            #region interview structure initialization

            questionnaire.ConnectChildrenWithParent();

            this.rosterStructure = rosterStructure;

            this.BuildInterviewStructureFromTemplate(questionnaire);

            this.BuildRosterTitleQuestions(rosterStructure);

            this.referencedQuestionToLinkedQuestionsMap = questionnaire
                .Find<IQuestion>(question => question.LinkedToQuestionId != null)
                .GroupBy(question => question.LinkedToQuestionId.Value)
                .ToDictionary(
                    keySelector: grouping => grouping.Key,
                    elementSelector: grouping => grouping.Select(question => question.PublicKey).ToArray());

            this.instancesOfAnsweredQuestionsUsableAsLinkedQuestionsOptions = new Dictionary<Guid, HashSet<InterviewItemId>>();

            this.SubscribeToQuestionAnswersForQuestionsWithSubstitutionReferences(questionnaire);

            #endregion

            #region interview data initialization

            this.CreateInterviewChapters(questionnaire);

            this.CreateInterviewTitle(questionnaire);

            this.UpdateHashForFeaturedQuestions();

            #endregion
        }

        private void UpdateHashForFeaturedQuestions()
        {
            foreach (KeyValuePair<InterviewItemId, QuestionViewModel> featuredQuestion in this.FeaturedQuestions)
            {
                featuredQuestion.Value.PropertyChanged += this.QuestionPropertyChanged;
            }
        }

        private void FireSubstitutionEventsForPrefilledQuestions()
        {
            foreach (KeyValuePair<InterviewItemId, QuestionViewModel> featuredQuestion in this.FeaturedQuestions)
            {
                this.SubstituteDependantQuestions(featuredQuestion.Value);
            }
        }

        private Dictionary<Guid, IList<Guid>> questionsParticipationInSubstitutionReferences =
            new Dictionary<Guid, IList<Guid>>();

        private Dictionary<ValueVector<Guid>, IList<Guid>> rostersParticipationInSubstitutionReferences = new Dictionary<ValueVector<Guid>, IList<Guid>>();

        private void SubscribeToQuestionAnswersForQuestionsWithSubstitutionReferences(IQuestionnaireDocument questionnaire)
        {
            var allQuestions = questionnaire.Find<IQuestion>(q => true).ToArray();
            foreach (var questionsWithSubstitution in allQuestions)
            {
                var substitutionReferences = SubstitutionService.GetAllSubstitutionVariableNames(questionsWithSubstitution.QuestionText);
                if (!substitutionReferences.Any())
                    continue;

                foreach (var substitutionReference in substitutionReferences)
                {
                    if (substitutionReference == SubstitutionService.RosterTitleSubstitutionReference)
                        HandleRosterTitleInSubstitutions(questionsWithSubstitution);
                    else
                        HandleQuestionReferenceInSubstitution(questionnaire, questionsWithSubstitution, substitutionReference);
                }
            }
        }

        private void HandleRosterTitleInSubstitutions(IQuestion questionsWithSubstitution)
        {
            var rosterVector = GetQuestionRosterScope(questionsWithSubstitution);
            if (rostersParticipationInSubstitutionReferences.ContainsKey(rosterVector))
            {
                if (!rostersParticipationInSubstitutionReferences[rosterVector].Contains(questionsWithSubstitution.PublicKey))
                    rostersParticipationInSubstitutionReferences[rosterVector].Add(questionsWithSubstitution.PublicKey);
            }
            else
            {
                rostersParticipationInSubstitutionReferences.Add(rosterVector,
                    new List<Guid> { questionsWithSubstitution.PublicKey });
            }
        }

        private void HandleQuestionReferenceInSubstitution(IQuestionnaireDocument questionnaire, IQuestion questionsWithSubstitution,
            string substitutionReference)
        {
            var referencedQuestion =
                questionnaire.FirstOrDefault<IQuestion>(
                    q => substitutionReference.Equals(q.StataExportCaption, StringComparison.InvariantCultureIgnoreCase));

            if (referencedQuestion == null)
                return;

            if (this.questionsParticipationInSubstitutionReferences.ContainsKey(referencedQuestion.PublicKey))
                this.questionsParticipationInSubstitutionReferences[referencedQuestion.PublicKey].Add(questionsWithSubstitution.PublicKey);
            else
                this.questionsParticipationInSubstitutionReferences.Add(referencedQuestion.PublicKey,
                    new List<Guid> { questionsWithSubstitution.PublicKey });
        }

        private void BuildRosterTitleQuestions(QuestionnaireRosterStructure rosterStructure)
        {
            foreach (var rosterDescription in rosterStructure.RosterScopes.Values)
            {
                foreach (var headQuestionId in rosterDescription.RosterIdToRosterTitleQuestionIdMap.Values)
                {
                    if (headQuestionId != null)
                        this.listOfHeadQuestionsMappedOnScope[headQuestionId.QuestionId] = rosterDescription.ScopeVector;
                }
            }
        }

        public ValueVector<Guid> GetScopeOfPropagatedScreen(Guid itemKey)
        {
            var itemScope = this.rosterStructure.RosterScopes.FirstOrDefault(s => s.Value.RosterIdToRosterTitleQuestionIdMap.ContainsKey(itemKey));
            if (itemScope.Equals(default(KeyValuePair<ValueVector<Guid>, RosterScopeDescription>)))
                throw new ArgumentException("item is absent in any scope");
            return itemScope.Key;
        }

        private void SetAnswers(InterviewSynchronizationDto interview)
        {
            foreach (var answeredQuestion in interview.Answers)
            {
                var questionKey = new InterviewItemId(answeredQuestion.Id, answeredQuestion.QuestionPropagationVector);
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

                if (IsQuestionReferencedByAnyLinkedQuestion(answeredQuestion.Id))
                    AddInstanceOfAnsweredQuestionUsableAsLinkedQuestionsOption(answeredQuestion.Id, answeredQuestion.QuestionPropagationVector);
            }
        }

        private void MarkAnswersAsInvalid(InterviewSynchronizationDto interview)
        {
            foreach (var question in interview.InvalidAnsweredQuestions)
            {
                this.SetQuestionValidity(new InterviewItemId(question.Id, question.InterviewItemPropagationVector), false);
            }
        }

        private void DisableInterviewElements(InterviewSynchronizationDto interview)
        {
            foreach (var group in interview.DisabledGroups)
            {
                this.SetScreenStatus(new InterviewItemId(@group.Id, @group.InterviewItemPropagationVector), false);
            }

            foreach (var question in interview.DisabledQuestions)
            {
                this.SetQuestionStatus(new InterviewItemId(question.Id, question.InterviewItemPropagationVector), false);
            }
        }

        private void PropagateGroups(InterviewSynchronizationDto interview)
        {
            foreach (var rosterGroupInstance in interview.RosterGroupInstances)
            {
                foreach (var rosterInstance in rosterGroupInstance.Value)
                {
                    this.AddPropagateScreen(rosterGroupInstance.Key.Id,
                        rosterGroupInstance.Key.InterviewItemPropagationVector, rosterInstance.RosterInstanceId, rosterInstance.SortIndex);

                    if (!string.IsNullOrEmpty(rosterInstance.RosterTitle))
                        this.UpdateRosterRowTitle(rosterGroupInstance.Key.Id, rosterGroupInstance.Key.InterviewItemPropagationVector,
                        rosterInstance.RosterInstanceId, rosterInstance.RosterTitle);
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
        }

        private IQuestionnaireViewModel GetScreenViewModel(InterviewItemId interviewItemId)
        {
            return this.Screens[interviewItemId];
        }

        #region fields

        public Guid PublicKey { get; private set; }
        public string Title { get; private set; }
        public InterviewStatus Status { get; set; }
        public IDictionary<InterviewItemId, IQuestionnaireViewModel> Screens { get; protected set; }
        public IList<QuestionnaireScreenViewModel> Chapters { get; protected set; }

        public InterviewStatistics Statistics
        {
            get
            {
                if (statistics == null)
                    statistics = new InterviewStatistics(Questions.Values);

                return statistics;
            }
        }

        private InterviewStatistics statistics;
        protected IDictionary<InterviewItemId, QuestionViewModel> Questions { get; set; }
        private readonly Dictionary<Guid, HashSet<InterviewItemId>> instancesOfAnsweredQuestionsUsableAsLinkedQuestionsOptions;
        private readonly Dictionary<Guid, ValueVector<Guid>> listOfHeadQuestionsMappedOnScope = new Dictionary<Guid, ValueVector<Guid>>();
        private readonly Dictionary<Guid, Guid[]> referencedQuestionToLinkedQuestionsMap;
        private readonly QuestionnaireRosterStructure rosterStructure;
        private readonly Dictionary<Guid, QuestionnairePropagatedScreenViewModel> propagatedScreenPrototypes =
            new Dictionary<Guid, QuestionnairePropagatedScreenViewModel>();

        private readonly Dictionary<Guid, QuestionnaireGridViewModel> rosterScreenPrototypes =
          new Dictionary<Guid, QuestionnaireGridViewModel>();

        public IDictionary<InterviewItemId, QuestionViewModel> FeaturedQuestions { get; private set; }

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
            var propagationVector = BuildPropagationVectorForGroup(outerScopePropagationVector,
                rosterInstanceId);

            if (this.Screens.ContainsKey(new InterviewItemId(screenId, propagationVector)))
                return;

            var screenPrototype = propagatedScreenPrototypes[screenId];
            var screen = screenPrototype.Clone(propagationVector, sortIndex);

            var questions = new List<QuestionViewModel>();

            foreach (var child in screen.Items)
            {
                var question = child as QuestionViewModel;
                if (question != null)
                {
                    UpdateQuestionHash(question);

                    questions.Add(question);

                    ApplySubstitutionsOnAddedQuestions(question);

                    continue;
                }
                var group = child as QuestionnaireNavigationPanelItem;
                if (group != null)
                {
                    if (this.rosterScreenPrototypes.ContainsKey(group.PublicKey.Id))
                    {
                        var gridPrototype = rosterScreenPrototypes[group.PublicKey.Id];
                        var newGridScreen = gridPrototype.Clone(propagationVector, sortIndex);
                        this.Screens.Add(newGridScreen.ScreenId, newGridScreen);
                        continue;
                    }
                    AddPropagateScreen(group.PublicKey.Id, outerScopePropagationVector, rosterInstanceId, sortIndex);
                }
            }

            screen.PropertyChanged += this.rosterScreen_PropertyChanged;
            Screens.Add(screen.ScreenId, screen);
            this.UpdateGrid(new InterviewItemId(screenId, outerScopePropagationVector));
            UpdateStatistics();
        }

        private void ApplySubstitutionsOnAddedQuestions(QuestionViewModel question)
        {
            var questionsUsedAsSubstitutionReferences =
                this.questionsParticipationInSubstitutionReferences.Where(r => r.Value.Contains(question.PublicKey.Id))
                    .Select(r => r.Key)
                    .ToList();

            foreach (var questionsUsedAsSubstitutionReference in questionsUsedAsSubstitutionReferences)
            {
                var questionSourceOfSubstitution = this.Questions.Values.FirstOrDefault(q => q.PublicKey.Id == questionsUsedAsSubstitutionReference &&
                    question.PublicKey.InterviewItemPropagationVector.Take(q.PublicKey.InterviewItemPropagationVector.Length)
                        .SequenceEqual(q.PublicKey.InterviewItemPropagationVector));

                if (questionSourceOfSubstitution != null)
                    question.SubstituteQuestionText(questionSourceOfSubstitution);
            }
        }

        public void RemovePropagatedScreen(Guid screenId, decimal[] outerScopePropagationVector, decimal index)
        {
            var propagationVector = this.BuildPropagationVectorForGroup(outerScopePropagationVector,
                index);

            this.RemoveScreen(new InterviewItemId(screenId, propagationVector));
            this.UpdateGrid(new InterviewItemId(screenId, outerScopePropagationVector));
            UpdateStatistics();
        }

        private void RemoveScreen(InterviewItemId screenId)
        {
            if (!this.Screens.ContainsKey(screenId))
                return;

            var screen = this.Screens[screenId];

            var simpleScreen = screen as QuestionnaireScreenViewModel;
            if (simpleScreen != null)
                CleanupSimpleScreen(simpleScreen);

            var rosterScreen = screen as QuestionnaireGridViewModel;
            if (rosterScreen != null)
                CleanupRosterScreen(rosterScreen);

            this.Screens.Remove(screenId);
        }

        private void CleanupSimpleScreen(QuestionnaireScreenViewModel simpleScreen)
        {
            foreach (var item in simpleScreen.Items)
            {
                var question = item as QuestionViewModel;
                if (question != null)
                {
                    this.Questions.Remove(question.PublicKey);
                }
                var group = item as QuestionnaireNavigationPanelItem;
                if (group != null)
                {
                    RemoveScreen(group.PublicKey);
                }
            }
        }

        private void CleanupRosterScreen(QuestionnaireGridViewModel roster)
        {
            foreach (var rosterRow in roster.Rows)
            {
                RemoveScreen(rosterRow.ScreenId);
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
            if (!this.Questions.ContainsKey(key))
                return;

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
            if (!Screens.ContainsKey(key))
            {
                Logger.Error(string.Format("screen '{0}', '{1}' is missing", key.Id, string.Join(",", key.InterviewItemPropagationVector)));
                return;
            }

            var screen =
                this.Screens[key];

            screen.SetEnabled(enabled);
        }

        public void UpdateRosterRowTitle(Guid groupId, decimal[] outerScopePropagationVector, decimal index, string rosterTitle)
        {
            var propagationVector = this.BuildPropagationVectorForGroup(outerScopePropagationVector, index);
            var key = new InterviewItemId(groupId, propagationVector);

            if (!this.Screens.ContainsKey(key))
                return;

            var screen = this.Screens[key] as QuestionnairePropagatedScreenViewModel;
            if (screen == null)
                return;

            screen.UpdateScreenName(rosterTitle);
            UpdateRosterTitleSubstitution(key);
        }

        private void UpdateRosterTitleSubstitution(InterviewItemId rosterId)
        {
            var level = rosterStructure.RosterScopes.Values.FirstOrDefault(scope => scope.RosterIdToRosterTitleQuestionIdMap.ContainsKey(rosterId.Id));
            if (level == null)
                return;

            if (!this.rostersParticipationInSubstitutionReferences.ContainsKey(level.ScopeVector))
                return;

            foreach (var participationQuestion in this.rostersParticipationInSubstitutionReferences[level.ScopeVector])
            {
                var rosterScreen =
                    Screens[rosterId] as
                        QuestionnairePropagatedScreenViewModel;
                if (rosterScreen == null)
                    continue;

                var questionKey = new InterviewItemId(participationQuestion, rosterId.InterviewItemPropagationVector);

                if (!Questions.ContainsKey(questionKey))
                    continue;
                Questions[questionKey].SubstituteRosterTitle(rosterScreen.ScreenName);
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

        private void QuestionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var question = sender as QuestionViewModel;
            if (question == null)
                return;
         
            if (e.PropertyName == "Status")
            {
                UpdateStatistics();
                return;
            }

            if (e.PropertyName != "AnswerString")
                return;

            SubstituteDependantQuestions(question);

            this.UpdateRosterTitlesForVectorByPossibleHeadQuestion(question.PublicKey.Id,question.PublicKey.InterviewItemPropagationVector, question.AnswerString);
        }

        private void UpdateStatistics()
        {
            statistics = null;
            this.RaisePropertyChanged("Statistics");
        }

        private void SubstituteDependantQuestions(QuestionViewModel question)
        {
            if (!this.questionsParticipationInSubstitutionReferences.ContainsKey(question.PublicKey.Id))
                return;

            foreach (var participationQuestionId in this.questionsParticipationInSubstitutionReferences[question.PublicKey.Id])
            {
                var questionsWhichUsesSubstitution= this.Questions.Values.Where(q => q.PublicKey.Id == participationQuestionId &&
                    q.PublicKey.InterviewItemPropagationVector.Take(question.PublicKey.InterviewItemPropagationVector.Length)
                        .SequenceEqual(question.PublicKey.InterviewItemPropagationVector));
                foreach (var participationQuestion in questionsWhichUsesSubstitution)
                {
                    participationQuestion.SubstituteQuestionText(question);
                }
            }
        }

        private void UpdateRosterTitlesForVectorByPossibleHeadQuestion(Guid possibleHeadQuestionId, decimal[] propagationVector, string newTitle)
        {
            if (!this.listOfHeadQuestionsMappedOnScope.ContainsKey(possibleHeadQuestionId))
                return;

            var scopeId = this.listOfHeadQuestionsMappedOnScope[possibleHeadQuestionId];

            var siblingsByPropagationScopeIds = this.rosterStructure.RosterScopes[scopeId].RosterIdToRosterTitleQuestionIdMap.Keys;

            var screensSiblingByPropagationScopeWithVector =
                siblingsByPropagationScopeIds.Select(
                    screenId => this.Screens[new InterviewItemId(screenId, propagationVector)] as QuestionnairePropagatedScreenViewModel);

            foreach (var screen in screensSiblingByPropagationScopeWithVector)
            {
                screen.UpdateScreenName(newTitle);
                UpdateRosterTitleSubstitution(screen.ScreenId);
            }
        }

        private void rosterScreen_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var propagatedScreen = sender as QuestionnairePropagatedScreenViewModel;
            if (propagatedScreen == null)
                return;

            this.UpdateGrid(new InterviewItemId(propagatedScreen.ScreenId.Id,
                propagatedScreen.ScreenId.InterviewItemPropagationVector.Take(
                    propagatedScreen.ScreenId.InterviewItemPropagationVector.Length - 1).ToArray()));
        }

        protected void AddScreen(List<IGroup> rout,
            IGroup group)
        {
            var key = new InterviewItemId(group.PublicKey);
            var lastVersionOfRout = rout.ToList();

            if (!IsGroupRoster(group))
            {
                var routWithouLast = rout.Take(rout.Count - 1).ToList();
                if (routWithouLast.Any(IsGroupRoster))
                {
                    AddPropagatedScreenPrototype(group, this.BuildBreadCrumbs(lastVersionOfRout), (groupId) => this.BuildSiblingsForNonPropagatedGroups(lastVersionOfRout, groupId));
                }
                else
                {
                    var screenItems = this.BuildItems(group, true);
                    var screen = new QuestionnaireScreenViewModel(this.PublicKey, group.Title, this.Title, true,
                        key, screenItems,
                        this.BuildSiblingsForNonPropagatedGroups(lastVersionOfRout, key),
                        this.BuildBreadCrumbs(lastVersionOfRout));
                    this.Screens.Add(key, screen);
                }
            }
            else
            {
                var gridKey = new InterviewItemId(group.PublicKey);
                if (!this.Screens.ContainsKey(gridKey))
                {
                    this.CreateGrid(group, lastVersionOfRout, this.IsNestedRoster(group));
                }
            }

        }

        protected bool IsNestedRoster(IGroup group)
        {
            var currentParent = (IGroup)group.GetParent();
            while (currentParent != null)
            {
                if (currentParent.IsRoster)
                    return true;

                currentParent = (IGroup)currentParent.GetParent();
            }
            return false;
        }

        protected bool IsGroupRoster(IGroup group)
        {
            return group.Propagated != Propagate.None || group.IsRoster;
        }

        protected void UpdateQuestionHash(QuestionViewModel question)
        {
            this.Questions.Add(question.PublicKey, question);

            question.PropertyChanged += this.QuestionPropertyChanged;
        }

        protected void CreateGrid(IGroup group, List<IGroup> root, bool isNestedRoster)
        {
            InterviewItemId rosterKey = new InterviewItemId(group.PublicKey);
            var breadcrumbs = this.BuildBreadCrumbs(root);
            var header = new List<HeaderItem>();

            foreach (var child in @group.Children)
            {
                var question = child as IQuestion;
                if (question != null)
                {
                    if (question.QuestionScope == QuestionScope.Interviewer)
                        header.Add(this.BuildHeader(question));
                    continue;
                }
                var childGroup = child as IGroup;
                if (childGroup != null)
                {
                    header.Add(new HeaderItem(childGroup.PublicKey, childGroup.Title, string.Empty));
                }
            }

            AddPropagatedScreenPrototype(group, breadcrumbs, this.GetSiblings);

            var siblings = root[root.Count - 2].Children.OfType<IGroup>();

            var roster = new QuestionnaireGridViewModel(this.PublicKey, group.Title, this.Title,
                rosterKey, true,
                (rosterId) => siblings.Select(
                    g => new InterviewItemId(g.PublicKey, rosterId.InterviewItemPropagationVector)),
                breadcrumbs.Take(breadcrumbs.Count - 1).ToList(), header, this.CollectPropagatedScreen);

            if (isNestedRoster)
                this.rosterScreenPrototypes.Add(group.PublicKey, roster);
            else
                this.Screens.Add(rosterKey, roster);
        }

        protected void AddPropagatedScreenPrototype(IGroup group, IList<InterviewItemId> breadcrumbs, Func<InterviewItemId, IEnumerable<InterviewItemId>> getSiblings)
        {
            var newScreenKey = new InterviewItemId(group.PublicKey);
            var screenItems = this.BuildItems(group, false);
            var screenName = this.IsGroupRoster(group) ? string.Empty : group.Title;
            var screenPrototype = new QuestionnairePropagatedScreenViewModel(this.PublicKey, screenName, group.Title, true,
                newScreenKey, screenItems,
                getSiblings,
                breadcrumbs);

            this.propagatedScreenPrototypes.Add(newScreenKey.Id, screenPrototype);
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

        protected IEnumerable<InterviewItemId> GetSiblings(InterviewItemId publicKey)
        {
            return
                this.Screens.Where(
                    s =>
                        s.Key.Id == publicKey.Id &&
                            s.Key.InterviewItemPropagationVector.Length == publicKey.InterviewItemPropagationVector.Length &&
                            s.Key.InterviewItemPropagationVector.Take(s.Key.InterviewItemPropagationVector.Length - 1)
                                .SequenceEqual(
                                    publicKey.InterviewItemPropagationVector.Take(publicKey.InterviewItemPropagationVector.Length - 1)))
                    .Select(
                        s => new InterviewItemId(publicKey.Id, s.Key.InterviewItemPropagationVector)).ToList();
        }

        protected void UpdateGrid(InterviewItemId gridkey)
        {
            if (!this.Screens.ContainsKey(gridkey))
                return;
            var grid = this.Screens[gridkey] as QuestionnaireGridViewModel;
            if (grid != null)
                grid.UpdateGridAfterRowsWereAdded();
        }

        protected IEnumerable<QuestionnairePropagatedScreenViewModel> CollectPropagatedScreen(InterviewItemId rosterId)
        {
            var allPropagatedScreens = this.Screens
                .Select(s => s.Value)
                .OfType<QuestionnairePropagatedScreenViewModel>()
                .Where(
                    s =>
                        new InterviewItemId(s.ScreenId.Id,
                            s.ScreenId.InterviewItemPropagationVector.Take(s.ScreenId.InterviewItemPropagationVector.Length - 1).ToArray()) ==
                            rosterId);

            return allPropagatedScreens.OrderBy(x => x.SortIndex)
                .ToList();
        }

        private IList<InterviewItemId> BuildBreadCrumbs(List<IGroup> rout)
        {
            var result = new List<InterviewItemId>();

            foreach (var groupInRout in rout.Skip(1))
            {
                result.Add(new InterviewItemId(groupInRout.PublicKey));

                if (IsGroupRoster(groupInRout))
                {
                    result.Add(EmptyBreadcrumbForRosterRow.CreateEmptyBreadcrumbForRosterRow(groupInRout.PublicKey));
                }
            }
            return result;
        }

        protected IEnumerable<InterviewItemId> BuildSiblingsForNonPropagatedGroups(IList<IGroup> rout, InterviewItemId key)
        {
            var parent = rout[rout.Count - 2];
            return parent.Children.OfType<IGroup>().Select(
                g => new InterviewItemId(g.PublicKey, key.InterviewItemPropagationVector));
        }

        protected HeaderItem BuildHeader(IQuestion question)
        {
            string text = question.GetVariablesUsedInTitle().Aggregate(question.QuestionText, (current, substitutionVariable) => SubstitutionService.ReplaceSubstitutionVariable(current, substitutionVariable, SubstitutionService.DefaultSubstitutionText));
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
                        key, GetScreenViewModel);
            }
            return null;
        }

        private bool IfQuestionNeedToBeSkipped(IQuestion question)
        {
            return question.QuestionScope != QuestionScope.Interviewer && !question.Featured;
        }

        private QuestionViewModel CreateQuestionView(IQuestion question)
        {
            var questionViewType = this.CalculateViewType(question.QuestionType);
            if (this.selectableQuestionTypes.Contains(question.QuestionType))
            {
                if (question.LinkedToQuestionId.HasValue)
                    return this.CreateLinkedQuestion(question, questionViewType);
                return this.CreateSelectableQuestion(question, questionViewType);
            }
            else if (this.listQuestionTypes.Contains(question.QuestionType))
            {
                return this.CreateTextListQuestion(question, questionViewType);
            }
            return this.CreateValueQuestion(question, questionViewType);
        }

        private ValueQuestionViewModel CreateValueQuestion(IQuestion question, QuestionType newType)
        {
            return new ValueQuestionViewModel(
                new InterviewItemId(question.PublicKey), GetQuestionRosterScope(question), question.QuestionText,
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
                new InterviewItemId(question.PublicKey), GetQuestionRosterScope(question), question.QuestionText,
                newType,
                true, question.Instructions,
                true, question.Mandatory, question.ValidationMessage,
                (questionRosterVecor, questionRosterScope) => this.GetAnswerOptionsForLinkedQuestion(question.LinkedToQuestionId.Value, questionRosterVecor, questionRosterScope),
                question.StataExportCaption, question.GetVariablesUsedInTitle(),
                multyOptionsQuestion != null ? multyOptionsQuestion.AreAnswersOrdered : (bool?)null,
                multyOptionsQuestion != null ? multyOptionsQuestion.MaxAllowedAnswers : null);
        }

        private TextListQuestionViewModel CreateTextListQuestion(IQuestion question, QuestionType newType)
        {
            var textListQuestion = question as TextListQuestion;

            return new TextListQuestionViewModel(
                new InterviewItemId(question.PublicKey), GetQuestionRosterScope(question), question.QuestionText,
                newType,
                true,
                textListQuestion.Instructions,
                textListQuestion.Comments,
                true,
                textListQuestion.Mandatory,
                textListQuestion.ValidationMessage,
                textListQuestion.StataExportCaption,
                textListQuestion.GetVariablesUsedInTitle(),
                textListQuestion.MaxAnswerCount,
                TextListQuestion.MaxAnswerCountLimit);
        }


        private SelectebleQuestionViewModel CreateSelectableQuestion(IQuestion question, QuestionType newType)
        {
            var multyOptionsQuestion = question as MultyOptionsQuestion;

            return new SelectebleQuestionViewModel(
                new InterviewItemId(question.PublicKey), GetQuestionRosterScope(question), question.QuestionText,
                newType, question.Answers.Select(
                    a => new AnswerViewModel(a.PublicKey, a.AnswerText, a.AnswerValue, false, a.AnswerImage)).ToList(),
                true, question.Instructions, null,
                true, question.Mandatory, null, question.ValidationMessage, question.StataExportCaption, question.GetVariablesUsedInTitle(),
                multyOptionsQuestion != null ? multyOptionsQuestion.AreAnswersOrdered : (bool?)null,
                multyOptionsQuestion != null ? multyOptionsQuestion.MaxAllowedAnswers : null);
        }

        private ValueVector<Guid> GetQuestionRosterScope(IQuestion question)
        {
            var result = new List<Guid>();

            var parentGroup = question.GetParent() as IGroup;

            while (parentGroup != null)
            {
                if (this.IsGroupRoster(parentGroup))
                {
                    if (parentGroup.RosterSizeQuestionId.HasValue)
                        result.Add(parentGroup.RosterSizeQuestionId.Value);
                    else
                        result.Add(parentGroup.PublicKey);
                }
                parentGroup = parentGroup.GetParent() as IGroup;
            }
            result.Reverse();
            return new ValueVector<Guid>(result.ToArray());
        }

        protected IEnumerable<LinkedAnswerViewModel> GetAnswerOptionsForLinkedQuestion(Guid referencedQuestionId, decimal[] linkedQuestionRosterVector, ValueVector<Guid> linkedQuestionRosterScope)
        {
            return !this.instancesOfAnsweredQuestionsUsableAsLinkedQuestionsOptions.ContainsKey(referencedQuestionId)
                ? Enumerable.Empty<LinkedAnswerViewModel>()
                : this
                    .instancesOfAnsweredQuestionsUsableAsLinkedQuestionsOptions[referencedQuestionId]
                    .Select(
                        instanceId => this.Questions.ContainsKey(instanceId) ? this.Questions[instanceId] : null)
                    .Where(
                        questionInstance =>
                            questionInstance != null && questionInstance.IsEnabled() &&
                                LinkedQuestionUtils.IsLevelAllowedToBeUsedAsLinkSourceInCurrentScope(
                                    questionInstance.PublicKey.InterviewItemPropagationVector, questionInstance.QuestionRosterScope,
                                    linkedQuestionRosterVector, linkedQuestionRosterScope)
                    )
                    .Select(
                        questionInstance =>
                            new LinkedAnswerViewModel(questionInstance.PublicKey.InterviewItemPropagationVector,
                                BuildLinkedQuestionOptionTitle(questionInstance,
                                    linkedQuestionRosterVector, linkedQuestionRosterScope)));
        }

        private string BuildLinkedQuestionOptionTitle(QuestionViewModel referencedQuestion, decimal[] linkedQuestionRosterVector, ValueVector<Guid> linkedQuestionRosterScope)
        {
            return LinkedQuestionUtils.BuildLinkedQuestionOptionTitle(referencedQuestion.AnswerString,
                (firstScreenInScopeId, firstScreeninScopeRosterVector) =>
                {
                    var screenFromScope = this.Screens[new InterviewItemId(firstScreenInScopeId, firstScreeninScopeRosterVector)];
                    return screenFromScope.ScreenName;
                }, referencedQuestion.PublicKey.InterviewItemPropagationVector, referencedQuestion.QuestionRosterScope,
                linkedQuestionRosterVector, linkedQuestionRosterScope, rosterStructure);
        }

        protected QuestionType CalculateViewType(QuestionType questionType)
        {
            if (this.singleOptionTypeVariation.Contains(questionType))
                return QuestionType.SingleOption;

            return questionType;
        }

        private readonly QuestionType[] selectableQuestionTypes = new[] { QuestionType.SingleOption, QuestionType.MultyOption };

        private readonly QuestionType[] singleOptionTypeVariation = new[] { QuestionType.SingleOption, QuestionType.DropDownList, QuestionType.YesNo };

        private readonly QuestionType[] listQuestionTypes = new[] { QuestionType.TextList };

        public bool IsQuestionReferencedByAnyLinkedQuestion(Guid questionId)
        {
            return this.referencedQuestionToLinkedQuestionsMap.ContainsKey(questionId)
                && this.referencedQuestionToLinkedQuestionsMap[questionId].Any();
        }
    }
}