using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails.GridItems;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Core.SharedKernels.SurveySolutions.Services;

namespace WB.Core.BoundedContexts.Capi.Views.InterviewDetails
{
    public class InterviewViewModel : MvxViewModel, IView
    {
        protected InterviewViewModel(Guid id)
        {
            this.PublicKey = id;
            this.Screens = new Dictionary<string, IQuestionnaireViewModel>();
            this.Questions = new Dictionary<string, QuestionViewModel>();
            this.FeaturedQuestions = new Dictionary<string, QuestionViewModel>();
            this.SuperviorQuestions = new Dictionary<string, AnsweredQuestionSynchronizationDto>();
            this.SuperviorQuestionIds = new Dictionary<Guid, ValueVector<Guid>>();
        }

        protected ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }

        protected ISubstitutionService SubstitutionService
        {
            get { return ServiceLocator.Current.GetInstance<ISubstitutionService>(); }
        }

        public InterviewViewModel(Guid id, QuestionnaireDocument questionnaire, QuestionnaireRosterStructure rosterStructure, InterviewSynchronizationDto interview)
            : this(id, questionnaire, rosterStructure)
        {
            #region interview data initialization
            this.Status = interview.Status;
            this.Title = questionnaire.Title;
            this.PropagateGroups(interview);
            this.SetAnswers(interview);
            this.DisableInterviewElements(interview);
            this.MarkAnswersAsInvalid(interview);

            this.FireSubstitutionEventsForPrefilledQuestions();
            #endregion
        }

        public InterviewViewModel(Guid id, QuestionnaireDocument questionnaire, QuestionnaireRosterStructure rosterStructure)
            : this(id)
        {
            #region interview structure initialization
            this.Title = questionnaire.Title;
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


            this.referencedQuestionToCascadingQuestionsMap = questionnaire
                .Find<SingleQuestion>(question => question.CascadeFromQuestionId.HasValue)
                .GroupBy(question => question.CascadeFromQuestionId.Value)
                .ToDictionary(
                    keySelector: grouping => grouping.Key,
                    elementSelector: grouping => grouping.Select(question => question.PublicKey).ToArray());

            this.instancesOfAnsweredQuestionsUsableAsLinkedQuestionsOptions = new Dictionary<Guid, HashSet<string>>();

            this.instancesOfAnsweredQuestionsUsableAsCascadingQuestions = new Dictionary<Guid, Dictionary<InterviewItemId, decimal>>();

            this.SubscribeToQuestionAnswersForQuestionsWithSubstitutionReferences(questionnaire);

            #endregion

            #region interview data initialization

            this.CreateInterviewChapters(questionnaire);

            this.SubscribePrefilledQuestionsOnPropertiesChanges();

            #endregion
        }

        private void SubscribePrefilledQuestionsOnPropertiesChanges()
        {
            foreach (KeyValuePair<string, QuestionViewModel> prefilledQuestion in this.FeaturedQuestions)
            {
                prefilledQuestion.Value.PropertyChanged += this.QuestionPropertyChanged;
            }
        }

        private void FireSubstitutionEventsForPrefilledQuestions()
        {
            foreach (KeyValuePair<string, QuestionViewModel> prefilledQuestion in this.FeaturedQuestions)
            {
                this.SubstituteDependantQuestions(prefilledQuestion.Value);
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
                    q => substitutionReference.Equals(q.StataExportCaption, StringComparison.CurrentCultureIgnoreCase));

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
            foreach (AnsweredQuestionSynchronizationDto answeredQuestion in interview.Answers)
            {
                var questionKey = ConvertIdAndRosterVectorToString(answeredQuestion.Id, answeredQuestion.QuestionPropagationVector);
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
                else if (this.SuperviorQuestionIds.ContainsKey(answeredQuestion.Id))
                {
                    if (!this.SuperviorQuestions.ContainsKey(questionKey))
                    {
                        this.SuperviorQuestions.Add(questionKey, null);
                    }
                    this.SuperviorQuestions[questionKey] = answeredQuestion;
                }

                if (IsQuestionReferencedByAnyLinkedQuestion(answeredQuestion.Id))
                    AddInstanceOfAnsweredQuestionUsableAsLinkedQuestionsOption(answeredQuestion.Id, answeredQuestion.QuestionPropagationVector);

                if (this.IsQuestionReferencedByAnyCascadingQuestion(answeredQuestion.Id))
                    this.AddInstanceOfAnsweredQuestionUsableAsCascadingQuestion(answeredQuestion.Id, answeredQuestion.QuestionPropagationVector, Convert.ToDecimal(answeredQuestion.Answer));
            }
        }

        private void MarkAnswersAsInvalid(InterviewSynchronizationDto interview)
        {
            foreach (var question in interview.InvalidAnsweredQuestions)
            {
                this.SetQuestionValidity(ConvertIdAndRosterVectorToString(question.Id, question.InterviewItemPropagationVector), false);
            }
        }

        private void DisableInterviewElements(InterviewSynchronizationDto interview)
        {
            foreach (var group in interview.DisabledGroups)
            {
                this.SetScreenStatus(ConvertIdAndRosterVectorToString(@group.Id, @group.InterviewItemPropagationVector), false);
            }

            foreach (var question in interview.DisabledQuestions)
            {
                this.SetQuestionStatus(ConvertIdAndRosterVectorToString(question.Id, question.InterviewItemPropagationVector), false);
            }
        }

        private void PropagateGroups(InterviewSynchronizationDto interview)
        {
            foreach (var rosterGroupInstance in interview.RosterGroupInstances)
            {
                foreach (var rosterInstance in rosterGroupInstance.Value)
                {
                    this.AddRosterScreen(rosterGroupInstance.Key.Id,
                        rosterGroupInstance.Key.InterviewItemPropagationVector, rosterInstance.RosterInstanceId, rosterInstance.SortIndex);

                    if (!String.IsNullOrEmpty(rosterInstance.RosterTitle))
                        this.UpdateRosterRowTitle(rosterGroupInstance.Key.Id, rosterGroupInstance.Key.InterviewItemPropagationVector,
                        rosterInstance.RosterInstanceId, rosterInstance.RosterTitle);
                }
            }
        }

        private void CreateInterviewChapters(IQuestionnaireDocument questionnarie)
        {
            this.Chapters = questionnarie.Children.OfType<IGroup>().Select(
                c => this.Screens[ConvertIdAndRosterVectorToString(c.PublicKey)]).OfType<QuestionnaireScreenViewModel>().ToList();
        }

        protected void BuildInterviewStructureFromTemplate(QuestionnaireDocument document)
        {
            List<IGroup> rout = new List<IGroup>();
            rout.Add(document);
            Stack<IGroup> queue = new Stack<IGroup>(document.Children.OfType<IGroup>());
            while (queue.Count > 0)
            {
                var current = queue.Pop();

                while (rout.Count > 0 && !rout[rout.Count - 1].Children.Contains(current))
                {
                    this.AddScreen(rout, rout.Last(), document);
                    rout.RemoveAt(rout.Count - 1);
                }
                rout.Add(current);
                foreach (IGroup child in current.Children.OfType<IGroup>())
                {
                    queue.Push(child);
                }
            }
            var last = rout.Last();
            while (!(last is QuestionnaireDocument))
            {
                this.AddScreen(rout, last, document);
                rout.Remove(last);
                last = rout.Last();
            }
        }

        private IQuestionnaireViewModel GetScreenViewModel(string interviewItemId)
        {
            return this.Screens[interviewItemId];
        }

        #region fields

        public Guid PublicKey { get; private set; }

        public string Title { get; set; }

        public InterviewStatus Status { get; set; }
        public IDictionary<string, IQuestionnaireViewModel> Screens { get; protected set; }
        private Dictionary<string, List<QuestionnairePropagatedScreenViewModel>> rosterScreensByParent = new Dictionary<string, List<QuestionnairePropagatedScreenViewModel>>();
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
        protected IDictionary<string, QuestionViewModel> Questions { get; set; }
        private readonly Dictionary<Guid, HashSet<string>> instancesOfAnsweredQuestionsUsableAsLinkedQuestionsOptions;
        private readonly Dictionary<Guid, Dictionary<InterviewItemId, decimal>> instancesOfAnsweredQuestionsUsableAsCascadingQuestions;
        private readonly Dictionary<Guid, ValueVector<Guid>> listOfHeadQuestionsMappedOnScope = new Dictionary<Guid, ValueVector<Guid>>();
        private readonly Dictionary<Guid, Guid[]> referencedQuestionToLinkedQuestionsMap;
        internal readonly Dictionary<Guid, Guid[]> referencedQuestionToCascadingQuestionsMap;
        private readonly QuestionnaireRosterStructure rosterStructure;
        private readonly Dictionary<Guid, QuestionnairePropagatedScreenViewModel> propagatedScreenPrototypes =
            new Dictionary<Guid, QuestionnairePropagatedScreenViewModel>();

        private readonly Dictionary<Guid, QuestionnaireGridViewModel> rosterScreenPrototypes =
          new Dictionary<Guid, QuestionnaireGridViewModel>();

        public IDictionary<string, QuestionViewModel> FeaturedQuestions { get; private set; }
        public IDictionary<string, AnsweredQuestionSynchronizationDto> SuperviorQuestions { get; private set; }
        public IDictionary<Guid, ValueVector<Guid>> SuperviorQuestionIds { get; private set; }

        #endregion

        public void UpdatePropagateGroupsByTemplate(Guid publicKey, decimal[] outerScopePropagationVector, int count)
        {
            var propagatedGroupsCount = this.Screens.Values.Count(id => id.ScreenId.Id == publicKey) - 1;
            if (propagatedGroupsCount == count)
                return;

            for (int i = 0; i < Math.Abs(count - propagatedGroupsCount); i++)
            {
                if (propagatedGroupsCount < count)
                {
                    var rosterInstanceId = propagatedGroupsCount + i;
                    this.AddRosterScreen(publicKey, outerScopePropagationVector, rosterInstanceId, rosterInstanceId);
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

        public void AddRosterScreen(Guid screenId, decimal[] outerScopePropagationVector, decimal rosterInstanceId, int? sortIndex)
        {
            var propagationVector = BuildPropagationVectorForGroup(outerScopePropagationVector,
                rosterInstanceId);

            if (this.Screens.ContainsKey(ConvertIdAndRosterVectorToString(screenId, propagationVector)))
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
                        this.Screens.Add(ConvertInterviewItemId(newGridScreen.ScreenId), newGridScreen);
                        continue;
                    }
                    this.AddRosterScreen(group.PublicKey.Id, outerScopePropagationVector, rosterInstanceId, sortIndex);
                }
            }

            screen.PropertyChanged += this.rosterScreen_PropertyChanged;
            Screens.Add(ConvertInterviewItemId(screen.ScreenId), screen);
            AddQuestionnairePropagatedScreenViewModel(screen);
            this.UpdateGrid(ConvertIdAndRosterVectorToString(screenId, outerScopePropagationVector));
            UpdateStatistics();
        }

        private void AddQuestionnairePropagatedScreenViewModel(QuestionnairePropagatedScreenViewModel screen)
        {
            var parentGridId = ConvertIdAndRosterVectorToString(screen.ScreenId.Id,
                screen.ScreenId.InterviewItemPropagationVector.Take(
                    screen.ScreenId.InterviewItemPropagationVector.Length - 1).ToArray());
            if (!rosterScreensByParent.ContainsKey(parentGridId))
            {
                rosterScreensByParent.Add(parentGridId, new List<QuestionnairePropagatedScreenViewModel>());
            }
            rosterScreensByParent[parentGridId].Add(screen);
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

                if (questionSourceOfSubstitution == null)
                {
                    questionSourceOfSubstitution =
                        this.FeaturedQuestions.Values.FirstOrDefault(q => q.PublicKey.Id == questionsUsedAsSubstitutionReference);
                }

                if (questionSourceOfSubstitution != null)
                    question.SubstituteQuestionText(questionSourceOfSubstitution);
            }
        }

        public void RemovePropagatedScreen(Guid screenId, decimal[] outerScopePropagationVector, decimal index)
        {
            var propagationVector = this.BuildPropagationVectorForGroup(outerScopePropagationVector,
                index);
            var screenIdWithVector = ConvertIdAndRosterVectorToString(screenId, propagationVector);
            this.RemoveScreen(screenIdWithVector);
            this.rosterScreensByParent[ConvertIdAndRosterVectorToString(screenId, outerScopePropagationVector)].RemoveAll(
                s => ConvertInterviewItemId(s.ScreenId) == screenIdWithVector);
            this.UpdateGrid(ConvertIdAndRosterVectorToString(screenId, outerScopePropagationVector));
            UpdateStatistics();
        }

        private void RemoveScreen(string screenId)
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
                    this.Questions.Remove(ConvertInterviewItemId(question.PublicKey));
                }
                var group = item as QuestionnaireNavigationPanelItem;
                if (group != null)
                {
                    RemoveScreen(ConvertInterviewItemId(group.PublicKey));
                }
            }
        }

        private void CleanupRosterScreen(QuestionnaireGridViewModel roster)
        {
            foreach (var rosterRow in roster.Rows)
            {
                RemoveScreen(ConvertInterviewItemId(rosterRow.ScreenId));
            }
        }

        public IEnumerable<IQuestionnaireViewModel> RestoreBreadCrumbs(IEnumerable<InterviewItemId> breadcrumbs)
        {
            return breadcrumbs.Select(b => this.Screens[ConvertInterviewItemId(b)]);
        }

        public void SetAnswer(string key, object answer)
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

        public void RemoveAnswer(string questionInstanceId)
        {
            if (!this.Questions.ContainsKey(questionInstanceId))
                return;

            QuestionViewModel question = this.Questions[questionInstanceId];

            question.RemoveAnswer();
        }

        public void SetComment(string key, string comment)
        {
            if (!this.Questions.ContainsKey(key))
                return;

            var question =
                this.Questions[key];
            question.SetComment(comment);
        }

        public void SetQuestionStatus(string key, bool enebled)
        {
            if (!this.Questions.ContainsKey(key))
                return;

            var question =
                this.Questions[key];
            question.SetEnabled(enebled);
        }

        public void SetQuestionValidity(string key, bool valid)
        {
            if (this.Questions.ContainsKey(key))
            {
                this.Questions[key].SetValid(valid);
            } 
            else if (this.FeaturedQuestions.ContainsKey(key))
            {
                this.FeaturedQuestions[key].SetValid(valid);
            }
        }

        public void SetScreenStatus(string key, bool enabled)
        {
            if (!Screens.ContainsKey(key))
            {
                return;
            }

            var screen =
                this.Screens[key];

            screen.SetEnabled(enabled);
        }

        public void UpdateRosterRowTitle(Guid groupId, decimal[] outerScopePropagationVector, decimal index, string rosterTitle)
        {
            var propagationVector = this.BuildPropagationVectorForGroup(outerScopePropagationVector, index);
            var key = ConvertIdAndRosterVectorToString(groupId, propagationVector);

            if (!this.Screens.ContainsKey(key))
                return;

            var screen = this.Screens[key] as QuestionnairePropagatedScreenViewModel;
            if (screen == null)
                return;

            screen.UpdateScreenName(rosterTitle);
            UpdateRosterTitleSubstitution(screen);
        }

        private void UpdateRosterTitleSubstitution(QuestionnairePropagatedScreenViewModel rosterScreen)
        {
            var level = rosterStructure.RosterScopes.Values.FirstOrDefault(scope => scope.RosterIdToRosterTitleQuestionIdMap.ContainsKey(rosterScreen.ScreenId.Id));
            if (level == null)
                return;

            if (!this.rostersParticipationInSubstitutionReferences.ContainsKey(level.ScopeVector))
                return;

            foreach (var participationQuestion in this.rostersParticipationInSubstitutionReferences[level.ScopeVector])
            {
                var questionKey = ConvertIdAndRosterVectorToString(participationQuestion, rosterScreen.ScreenId.InterviewItemPropagationVector);

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
                this.instancesOfAnsweredQuestionsUsableAsLinkedQuestionsOptions.Add(questionId, new HashSet<string>());

            var questionInstanceId = ConvertIdAndRosterVectorToString(questionId, propagationVector);

            this.instancesOfAnsweredQuestionsUsableAsLinkedQuestionsOptions[questionId].Add(questionInstanceId);

            this.NotifyAffectedLinkedQuestions(questionId);
        }

        public void RemoveInstanceOfAnsweredQuestionUsableAsLinkedQuestionsOption(Guid questionId, decimal[] propagationVector)
        {
            if (!this.instancesOfAnsweredQuestionsUsableAsLinkedQuestionsOptions.ContainsKey(questionId))
                return;

            var questionInstanceId = ConvertIdAndRosterVectorToString(questionId, propagationVector);

            this.instancesOfAnsweredQuestionsUsableAsLinkedQuestionsOptions[questionId].Remove(questionInstanceId);

            this.NotifyAffectedLinkedQuestions(questionId);
        }

        public virtual void AddInstanceOfAnsweredQuestionUsableAsCascadingQuestion(Guid questionId, decimal[] propagationVector, decimal selectedValue)
        {
            if (!this.instancesOfAnsweredQuestionsUsableAsCascadingQuestions.ContainsKey(questionId))
                this.instancesOfAnsweredQuestionsUsableAsCascadingQuestions.Add(questionId, new Dictionary<InterviewItemId, decimal>());

            var questionInstanceId = new InterviewItemId(questionId, propagationVector);

            this.instancesOfAnsweredQuestionsUsableAsCascadingQuestions[questionId][questionInstanceId] = selectedValue;

            this.NotifyAffectedCascadingQuestions(questionId);
        }

        public virtual void RemoveInstanceOfAnsweredQuestionUsableAsCascadingQuestion(Guid questionId, decimal[] propagationVector)
        {
            if (!this.instancesOfAnsweredQuestionsUsableAsCascadingQuestions.ContainsKey(questionId))
                return;

            var questionInstanceId = new InterviewItemId(questionId, propagationVector);

            this.instancesOfAnsweredQuestionsUsableAsCascadingQuestions[questionId].Remove(questionInstanceId);

            this.NotifyAffectedCascadingQuestions(questionId);
        }

        private void NotifyAffectedLinkedQuestions(Guid referencedQuestionId)
        {
            this.referencedQuestionToLinkedQuestionsMap[referencedQuestionId]
                .SelectMany(linkedQuestionId => this.GetQuestionModelById<LinkedQuestionViewModel>(linkedQuestionId))
                .ToList()
                .ForEach(linkedQuestionViewModel => linkedQuestionViewModel.HandleAnswerListChange());
        }

        private void NotifyAffectedCascadingQuestions(Guid referencedQuestionId)
        {
            if (!this.referencedQuestionToCascadingQuestionsMap.ContainsKey(referencedQuestionId))
                return;

            this.referencedQuestionToCascadingQuestionsMap[referencedQuestionId]
                .SelectMany(cascadingQuestionId => this.GetQuestionModelById<CascadingComboboxQuestionViewModel>(cascadingQuestionId))
                .ToList()
                .ForEach(cascadingComboboxQuestionViewModel => cascadingComboboxQuestionViewModel.HandleAnswerListChange());
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

            this.UpdateRosterTitlesForVectorByPossibleHeadQuestion(question.PublicKey.Id, question.PublicKey.InterviewItemPropagationVector, question.AnswerString);
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
                var questionsWhichUsesSubstitution = this.Questions.Values.Where(q => q.PublicKey.Id == participationQuestionId &&
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
                    screenId => this.Screens[ConvertIdAndRosterVectorToString(screenId, propagationVector)] as QuestionnairePropagatedScreenViewModel);

            foreach (var screen in screensSiblingByPropagationScopeWithVector)
            {
                screen.UpdateScreenName(newTitle);
                UpdateRosterTitleSubstitution(screen);
            }
        }

        private void rosterScreen_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var propagatedScreen = sender as QuestionnairePropagatedScreenViewModel;
            if (propagatedScreen == null)
                return;

            this.UpdateGrid(ConvertIdAndRosterVectorToString(propagatedScreen.ScreenId.Id,
                propagatedScreen.ScreenId.InterviewItemPropagationVector.Take(
                    propagatedScreen.ScreenId.InterviewItemPropagationVector.Length - 1).ToArray()));
        }

        protected void AddScreen(List<IGroup> rout,
            IGroup group, QuestionnaireDocument document)
        {
            var key = ConvertIdAndRosterVectorToString(group.PublicKey);
            var groupItemId = new InterviewItemId(group.PublicKey);
            var lastVersionOfRout = rout.ToList();

            if (!IsGroupRoster(group))
            {
                var routWithouLast = rout.Take(rout.Count - 1).ToList();
                if (routWithouLast.Any(IsGroupRoster))
                {
                    AddPropagatedScreenPrototype(group, this.BuildBreadCrumbs(lastVersionOfRout), (groupId) => this.BuildSiblingsForNonPropagatedGroups(lastVersionOfRout, groupId), document);
                }
                else
                {
                    var screenItems = this.BuildItems(group, true, document);
                    var screen = new QuestionnaireScreenViewModel(this.PublicKey, group.Title, this.Title, true,
                        groupItemId, screenItems,
                        this.BuildSiblingsForNonPropagatedGroups(lastVersionOfRout, groupItemId),
                        this.BuildBreadCrumbs(lastVersionOfRout));
                    this.Screens.Add(key, screen);
                }
            }
            else
            {
                var gridKey = ConvertIdAndRosterVectorToString(group.PublicKey);
                if (!this.Screens.ContainsKey(gridKey))
                {
                    this.CreateGrid(group, lastVersionOfRout, this.IsNestedRoster(group), document);
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
            return group.IsRoster;
        }

        protected void UpdateQuestionHash(QuestionViewModel question)
        {
            this.Questions.Add(ConvertInterviewItemId(question.PublicKey), question);

            question.PropertyChanged += this.QuestionPropertyChanged;
        }

        protected void CreateGrid(IGroup group, List<IGroup> root, bool isNestedRoster, QuestionnaireDocument document)
        {
            var rosterKey = ConvertIdAndRosterVectorToString(group.PublicKey);
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
                    header.Add(new HeaderItem(childGroup.PublicKey, childGroup.Title, String.Empty));
                }
            }

            AddPropagatedScreenPrototype(group, breadcrumbs, this.GetSiblings, document);

            var siblings = root[root.Count - 2].Children.OfType<IGroup>();

            var roster = new QuestionnaireGridViewModel(this.PublicKey, group.Title, this.Title,
                new InterviewItemId(group.PublicKey), true,
                (rosterId) => siblings.Select(
                    g => new InterviewItemId(g.PublicKey, rosterId.InterviewItemPropagationVector)),
                breadcrumbs.Take(breadcrumbs.Count - 1).ToList(), header, this.CollectPropagatedScreen);

            if (isNestedRoster)
                this.rosterScreenPrototypes.Add(group.PublicKey, roster);
            else
                this.Screens.Add(rosterKey, roster);
        }

        protected void AddPropagatedScreenPrototype(IGroup group, IList<InterviewItemId> breadcrumbs, Func<InterviewItemId, IEnumerable<InterviewItemId>> getSiblings, QuestionnaireDocument document)
        {
            var newScreenKey = new InterviewItemId(group.PublicKey);
            var screenItems = this.BuildItems(group, false, document);
            var screenName = this.IsGroupRoster(group) ? String.Empty : group.Title;
            var screenPrototype = new QuestionnairePropagatedScreenViewModel(this.PublicKey, screenName, group.Title, true,
                newScreenKey, screenItems,
                getSiblings,
                breadcrumbs);

            this.propagatedScreenPrototypes.Add(newScreenKey.Id, screenPrototype);
        }

        protected IList<IQuestionnaireItemViewModel> BuildItems(IGroup screen, bool updateHash, QuestionnaireDocument document)
        {
            IList<IQuestionnaireItemViewModel> result = new List<IQuestionnaireItemViewModel>();
            foreach (var child in screen.Children)
            {
                var item = this.CreateView(child, document);
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
                        s.Value.ScreenId.Id == publicKey.Id &&
                            s.Value.ScreenId.InterviewItemPropagationVector.Length == publicKey.InterviewItemPropagationVector.Length &&
                            s.Value.ScreenId.InterviewItemPropagationVector.Take(s.Value.ScreenId.InterviewItemPropagationVector.Length - 1)
                                .SequenceEqual(
                                    publicKey.InterviewItemPropagationVector.Take(publicKey.InterviewItemPropagationVector.Length - 1)))
                    .Select(
                        s => new InterviewItemId(publicKey.Id, s.Value.ScreenId.InterviewItemPropagationVector)).ToList();
        }

        protected void UpdateGrid(string gridkey)
        {
            if (!this.Screens.ContainsKey(gridkey))
                return;
            var grid = this.Screens[gridkey] as QuestionnaireGridViewModel;
            if (grid != null)
                grid.UpdateGridAfterRowsWereAdded();
        }

        protected IEnumerable<QuestionnairePropagatedScreenViewModel> CollectPropagatedScreen(string rosterId)
        {
            if (!rosterScreensByParent.ContainsKey(rosterId))
                return Enumerable.Empty<QuestionnairePropagatedScreenViewModel>();
            return rosterScreensByParent[rosterId].OrderBy(x => x.SortIndex)
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
            string text = String.IsNullOrEmpty(question.VariableLabel) ?
                question.GetVariablesUsedInTitle()
                    .Aggregate(question.QuestionText,
                        (current, substitutionVariable) =>
                            SubstitutionService.ReplaceSubstitutionVariable(current, substitutionVariable,
                                SubstitutionService.DefaultSubstitutionText)) : question.VariableLabel;
            return new HeaderItem(question.PublicKey, text, question.Instructions);
        }

        protected string BuildComments(IEnumerable<CommentDocument> comments)
        {
            if (comments == null)
                return String.Empty;
            return comments.Any() ? comments.Last().Comment : String.Empty;
        }

        protected IQuestionnaireItemViewModel CreateView(IComposite item, QuestionnaireDocument document)
        {
            var question = item as IQuestion;

            if (question != null)
            {
                if (question.QuestionScope != QuestionScope.Interviewer)
                {
                    this.SuperviorQuestionIds.Add(question.PublicKey, this.GetQuestionRosterScope(question));
                }

                if (this.IfQuestionNeedToBeSkipped(question))
                    return null;

                QuestionViewModel questionView = this.CreateQuestionView(question, document);

                if (question.Featured)
                {
                    this.FeaturedQuestions.Add(ConvertInterviewItemId(questionView.PublicKey), questionView);
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

            var staticText = item as IStaticText;
            if (staticText != null)
            {
                return new StaticTextViewModel(publicKey: new InterviewItemId(staticText.PublicKey),
                    text: staticText.Text);
            }

            return null;
        }

        private bool IfQuestionNeedToBeSkipped(IQuestion question)
        {
            return question.QuestionScope != QuestionScope.Interviewer && !question.Featured;
        }

        private QuestionViewModel CreateQuestionView(IQuestion question, QuestionnaireDocument document)
        {
            var questionViewType = this.CalculateViewType(question.QuestionType);
            if (this.selectableQuestionTypes.Contains(questionViewType))
            {
                if (questionViewType == QuestionType.SingleOption && question.IsFilteredCombobox.HasValue &&
                    question.IsFilteredCombobox.Value)
                {
                    return this.CreateFilteredComboboxQuestion(question);
                }

                if (questionViewType == QuestionType.SingleOption && question.CascadeFromQuestionId.HasValue)
                {
                    return this.CreateCascadingComboboxQuestion(question);
                }

                if (question.LinkedToQuestionId.HasValue)
                {
                    return this.CreateLinkedQuestion(question, questionViewType);
                }

                return this.CreateSelectableQuestion(question, questionViewType, document);
            }
            
            if (this.listQuestionTypes.Contains(question.QuestionType))
            {
                return this.CreateTextListQuestion(question, questionViewType, document);
            }
            return this.CreateValueQuestion(question, questionViewType, document);
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

        private TextListQuestionViewModel CreateTextListQuestion(IQuestion question, QuestionType newType, QuestionnaireDocument document)
        {
            var textListQuestion = question as TextListQuestion;

            return new TextListQuestionViewModel(
                new InterviewItemId(question.PublicKey),
                GetQuestionRosterScope(question),
                question.QuestionText,
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
                TextListQuestion.MaxAnswerCountLimit, triggeredRosters: CreateTriggeredRosterListByQuestion(question, document));
        }

        private ValueQuestionViewModel CreateValueQuestion(IQuestion question, QuestionType newType, QuestionnaireDocument document)
        {
            var txtQuestion = question as TextQuestion;
            var mask = "";
            if (txtQuestion != null)
            {
                mask = txtQuestion.Mask;
            }

            return new ValueQuestionViewModel(
                new InterviewItemId(question.PublicKey), GetQuestionRosterScope(question), question.QuestionText,
                newType,
                null,
                true, question.Instructions, mask, null,
                true, question.Mandatory,
                question.ValidationMessage,
                question.StataExportCaption,
                question.GetVariablesUsedInTitle(),
                this.GetQuestionsIsIntegerSetting(question), this.GetQuestionsDecimalPlacesSetting(question), triggeredRosters: CreateTriggeredRosterListByQuestion(question, document));
        }

        private SelectebleQuestionViewModel CreateSelectableQuestion(IQuestion question, QuestionType newType, QuestionnaireDocument document)
        {
            var multyOptionsQuestion = question as MultyOptionsQuestion;

            return new SelectebleQuestionViewModel(
                new InterviewItemId(question.PublicKey), GetQuestionRosterScope(question), question.QuestionText,
                newType, question.Answers.Select(
                    a => new AnswerViewModel(a.PublicKey, a.AnswerText, a.AnswerValue, false, null)).ToList(),
                true, question.Instructions, null,
                true, question.Mandatory, null, question.ValidationMessage, question.StataExportCaption, question.GetVariablesUsedInTitle(),
                multyOptionsQuestion != null ? multyOptionsQuestion.AreAnswersOrdered : (bool?)null,
                multyOptionsQuestion != null ? multyOptionsQuestion.MaxAllowedAnswers : null, triggeredRosters: CreateTriggeredRosterListByQuestion(question, document));
        }

        private string[] CreateTriggeredRosterListByQuestion(IQuestion question, QuestionnaireDocument document)
        {
            if (!new[] { QuestionType.Numeric, QuestionType.TextList, QuestionType.MultyOption }.Contains(question.QuestionType))
                return new string[0];
            return document.Find<IGroup>(g => g.IsRoster && g.RosterSizeQuestionId == question.PublicKey).Select(r => r.Title).ToArray();
        }

        private CascadingComboboxQuestionViewModel CreateCascadingComboboxQuestion(IQuestion question)
        {
            return new CascadingComboboxQuestionViewModel(
                new InterviewItemId(question.PublicKey), 
                GetQuestionRosterScope(question), 
                question.QuestionText,
                (questionRosterVecor, selectedAnswer) => this.GetFilteredAnswerOptionsForCascadingQuestion(question.CascadeFromQuestionId.Value, question.Answers, questionRosterVecor, selectedAnswer),
                true, 
                question.Instructions, 
                null,
                true, 
                question.Mandatory, 
                null, 
                question.ValidationMessage, 
                question.StataExportCaption, 
                question.GetVariablesUsedInTitle());
        }

        private FilteredComboboxQuestionViewModel CreateFilteredComboboxQuestion(IQuestion question)
        {
            return new FilteredComboboxQuestionViewModel(
                new InterviewItemId(question.PublicKey), GetQuestionRosterScope(question), question.QuestionText, question.Answers.Select(
                    a => new AnswerViewModel(a.PublicKey, a.AnswerText, a.AnswerValue, false, null)).ToList(),
                true, question.Instructions, null,
                true, question.Mandatory, null, question.ValidationMessage, question.StataExportCaption, question.GetVariablesUsedInTitle());
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

        protected IEnumerable<AnswerViewModel> GetFilteredAnswerOptionsForCascadingQuestion(Guid referencedQuestionId, List<Answer> answers, decimal[] rosterVector, object selectedAnswer)
        {
            if (!this.instancesOfAnsweredQuestionsUsableAsCascadingQuestions.ContainsKey(referencedQuestionId))
                return Enumerable.Empty<AnswerViewModel>();

            InterviewItemId referencedQuestionItemId =
                this.instancesOfAnsweredQuestionsUsableAsCascadingQuestions[referencedQuestionId]
                    .Keys
                    .SingleOrDefault(x => SequenceBeginningEqual(rosterVector, x));

            if (referencedQuestionItemId == default(InterviewItemId))
                return Enumerable.Empty<AnswerViewModel>();

            decimal filterValue = this.instancesOfAnsweredQuestionsUsableAsCascadingQuestions[referencedQuestionId][referencedQuestionItemId];

            return answers
                .Where(x => InvariantDecimalParse(x.ParentValue) == filterValue)
                .Select(x => new AnswerViewModel(x.PublicKey, x.AnswerText, x.AnswerValue, selectedAnswer != null &&
                    (InvariantDecimalParse(x.AnswerValue) == 
                    InvariantDecimalParse(selectedAnswer is decimal[] ? ((decimal[])selectedAnswer)[0].ToString(CultureInfo.InvariantCulture) :
                    ((decimal)selectedAnswer).ToString(CultureInfo.InvariantCulture))), null))
                .ToList();
        }

        private static decimal InvariantDecimalParse(string stringValue)
        {
            return Decimal.Parse(stringValue, CultureInfo.InvariantCulture);
        }

        protected IEnumerable<LinkedAnswerViewModel> GetAnswerOptionsForLinkedQuestion(Guid referencedQuestionId, decimal[] linkedQuestionRosterVector, ValueVector<Guid> linkedQuestionRosterScope)
        {
            if (!this.instancesOfAnsweredQuestionsUsableAsLinkedQuestionsOptions.ContainsKey(referencedQuestionId))
                return Enumerable.Empty<LinkedAnswerViewModel>();

            if (this.SuperviorQuestionIds.ContainsKey(referencedQuestionId))
            {
                var questionRosterScope = this.SuperviorQuestionIds[referencedQuestionId];
                return this.instancesOfAnsweredQuestionsUsableAsLinkedQuestionsOptions[referencedQuestionId]
                    .Select(instanceId => this.SuperviorQuestions.ContainsKey(instanceId) ? this.SuperviorQuestions[instanceId] : null)
                    .Where(questionInstance => questionInstance != null)
                    .Select(
                        questionInstance =>
                            new LinkedAnswerViewModel(
                                questionInstance.QuestionPropagationVector,
                                this.BuildLinkedQuestionOptionTitle(
                                    linkedQuestionRosterVector,
                                    linkedQuestionRosterScope,
                                    questionRosterScope,
                                    questionInstance.QuestionPropagationVector,
                                    AnswerUtils.AnswerToString(questionInstance.Answer)))
                    );
            }
            return this.instancesOfAnsweredQuestionsUsableAsLinkedQuestionsOptions[referencedQuestionId]
                .Select(instanceId => this.Questions.ContainsKey(instanceId) ? this.Questions[instanceId] : null)
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
                            this.BuildLinkedQuestionOptionTitle(linkedQuestionRosterVector, linkedQuestionRosterScope, questionInstance.QuestionRosterScope, questionInstance.PublicKey.InterviewItemPropagationVector, AnswerUtils.AnswerToString(questionInstance.AnswerObject))));
        }

        private string BuildLinkedQuestionOptionTitle(decimal[] linkedQuestionRosterVector,
            ValueVector<Guid> linkedQuestionRosterScope,
            ValueVector<Guid> referensedQuestionRosterScopeVector,
            decimal[] referencedQuestionPropagationVector,
            string referencedQuestionAnswer)
        {
            return LinkedQuestionUtils.BuildLinkedQuestionOptionTitle(
                referencedQuestionAnswer,
                (firstScreenInScopeId, firstScreeninScopeRosterVector) =>
                {
                    var screenFromScope = this.Screens[ConvertIdAndRosterVectorToString(firstScreenInScopeId, firstScreeninScopeRosterVector)];
                    return screenFromScope.ScreenName;
                },
                referencedQuestionPropagationVector,
                referensedQuestionRosterScopeVector,
                linkedQuestionRosterVector,
                linkedQuestionRosterScope,
                this.rosterStructure);
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

        public virtual bool IsQuestionReferencedByAnyCascadingQuestion(Guid questionId)
        {
            return this.referencedQuestionToCascadingQuestionsMap.ContainsKey(questionId)
                && this.referencedQuestionToCascadingQuestionsMap[questionId].Any();
        }

        private IEnumerable<T> GetQuestionModelById<T>(Guid cascadingQuestionId) where T : QuestionViewModel
        {
            return this.Questions.Values.OfType<T>()
                .Where(questionViewModel => questionViewModel.PublicKey.Id == cascadingQuestionId);
        }

        private static bool SequenceBeginningEqual(decimal[] linkedQuestionRosterVector, InterviewItemId x)
        {
            return x.InterviewItemPropagationVector.SequenceEqual(linkedQuestionRosterVector.Take(x.InterviewItemPropagationVector.Length));
        }

        private string ConvertIdAndRosterVectorToString(Guid id, decimal[] rosterVector = null)
        {
            return ConversionHelper.ConvertIdAndRosterVectorToString(id, rosterVector);
        }

        private string ConvertInterviewItemId(InterviewItemId interviewItemId)
        {
            return ConvertIdAndRosterVectorToString(interviewItemId.Id, interviewItemId.InterviewItemPropagationVector);
        }
    }
}