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
using Main.Core.Utility;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails
{
    public class InterviewViewModel : MvxViewModel, IView
    {
        protected InterviewViewModel(Guid id)
        {
            this.PublicKey = id;
            this.Screens = new Dictionary<InterviewItemId, IQuestionnaireViewModel>();
            this.Questions = new Dictionary<InterviewItemId, QuestionViewModel>();
            this.FeaturedQuestions = new Dictionary<InterviewItemId, QuestionViewModel>();

            this.propagationScopeDescription = new InterviewPropagationScopeDescription();
        }

        public InterviewViewModel(Guid id, IQuestionnaireDocument questionnaire, InterviewSynchronizationDto interview)
            : this(id)
        {

            this.Status = interview.Status;

            this.BuildInterviewStructureFromTemplate(questionnaire);

            this.BuildHeadQuestionsInsidePropagaedGroupsStructure(questionnaire);

            this.PropagateGroups(interview);

            this.SetAnswers(interview);

            this.SubscribeToQuestionAnswersForQuestionsWithSubstitutionReferences();

            this.DisableInterviewElements(interview);

            this.MarkAnswersAsInvalid(interview);

            this.CreateInterviewChapters(questionnaire);

            this.CreateInterviewTitle(questionnaire);

            this.referencedQuestionToLinkedQuestionsMap = questionnaire
                .Find<IQuestion>(question => question.LinkedToQuestionId != null)
                .GroupBy(question => question.LinkedToQuestionId.Value)
                .ToDictionary(
                    keySelector: grouping => grouping.Key,
                    elementSelector: grouping => grouping.Select(question => question.PublicKey).ToArray());

            this.instancesOfAnsweredQuestionsUsableAsLinkedQuestionsOptions = interview
                .Answers
                .Where(answer => this.IsQuestionReferencedByAnyLinkedQuestion(answer.Id))
                .GroupBy(answer => answer.Id)
                .ToDictionary(
                    keySelector: grouping => grouping.Key,
                    elementSelector: grouping => new HashSet<InterviewItemId>(grouping.Select(answer => new InterviewItemId(answer.Id, answer.PropagationVector))));
        }

        private Dictionary<QuestionViewModel, IEnumerable<QuestionViewModel>> questionsParticipationInSubstitutionReferences;
        

        private void SubscribeToQuestionAnswersForQuestionsWithSubstitutionReferences()
        {
            questionsParticipationInSubstitutionReferences = allQuestionViewModels.Where(x => x.SubstitutionReferences.Any())
                .SelectMany(
                    x =>
                        x.SubstitutionReferences.Select(
                            y =>
                                new
                                {
                                    ReferencedQuestion = allQuestionViewModels.FirstOrDefault(z => z.Variable == y && (z.PublicKey.PropagationVector.SequenceEqual(x.PublicKey.PropagationVector) || x.PublicKey.PropagationVector.Length < z.PublicKey.PropagationVector.Length)),
                                    ParticipationQuestion = x
                                }))
                .GroupBy(x => x.ReferencedQuestion, y => y.ParticipationQuestion, (referencedQuestion, participationQuestions) => new
                {
                    ReferencedQuestion = referencedQuestion,
                    ParticipationQuestions =
                        participationQuestions
                })
                .ToDictionary(
                    x => x.ReferencedQuestion,
                    y => y.ParticipationQuestions);

            foreach (var substitutionQuestion in questionsParticipationInSubstitutionReferences)
            {
                foreach (var participationQuestion in substitutionQuestion.Value)
                {
                    var text = participationQuestion.SubstitutionReferences.Aggregate(participationQuestion.SourceText,
                        (current, substitutionReference) =>
                            current.ReplaceSubstitutionVariable(substitutionReference,
                                !string.IsNullOrEmpty(substitutionQuestion.Key.AnswerString)
                                    ? substitutionQuestion.Key.AnswerString
                                    : StringUtil.DefaultSubstitutionText));
                    participationQuestion.SetText(text);
                }
                substitutionQuestion.Key.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == "AnswerString")
                    {
                        var vm = sender as QuestionViewModel;
                        if (vm != null)
                        {
                            var participationQuestions = questionsParticipationInSubstitutionReferences[vm];
                            if (participationQuestions != null)
                            {
                                foreach (var participationQuestion in participationQuestions)
                                {
                                    participationQuestion.SetText(
                                        participationQuestion.SourceText.ReplaceSubstitutionVariable(vm.Variable,
                                            string.IsNullOrEmpty(vm.AnswerString)
                                                ? StringUtil.DefaultSubstitutionText
                                                : vm.AnswerString));
                                }

                            }
                        }

                    }
                };
            }
        }

        private void BuildHeadQuestionsInsidePropagaedGroupsStructure(IQuestionnaireDocument questionnaire)
        {
            foreach (var propagatedGroup in questionnaire.Find<IGroup>(group => group.Propagated != Propagate.None))
            {
                var scopeOfPropagationId = this.propagationScopeDescription.GetScopeOfPropagatedScreen(propagatedGroup.PublicKey);
                foreach (var headQuestion in propagatedGroup.Find<IQuestion>(question => question.Capital))
                {
                    listofHeadQuestionsMappedOnScope.Add(headQuestion.PublicKey, scopeOfPropagationId);
                }
            }
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
                for (int i = 0; i < propagatedGroupInstanceCount.Value; i++)
                {
                    this.AddPropagateGroup(propagatedGroupInstanceCount.Key.Id,
                        propagatedGroupInstanceCount.Key.PropagationVector, i);
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
            foreach (var questionViewModel in FeaturedQuestions)
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
                AddScreen(rout, last);
                rout.Remove(last);
                last = rout.Last();
            }

            CreateNextPrevious();
        }

        protected void CreateNextPrevious()
        {
            var templates = this.propagationScopeDescription.GetAllPropagatedScreenTemplates().Select(t => t.ScreenId.Id).ToList();
            while (templates.Count > 0)
            {
                var first = templates[0];
                var scope = this.propagationScopeDescription.GetScreenSiblingsByPropagationLevel(first).ToArray();
                templates.RemoveAll(t => scope.Any(s => s == t));
                for (int i = 0; i < scope.Length; i++)
                {
                    var target = this.propagationScopeDescription.GetTemplateOfPropagatedScreen(scope[i]);
                    IQuestionnaireItemViewModel next = null;
                    IQuestionnaireItemViewModel previous = null;
                    if (i > 0)
                    {
                        var item = this.propagationScopeDescription.GetTemplateOfPropagatedScreen(scope[i - 1]);
                        previous = new QuestionnaireNavigationPanelItem(item.ScreenId, item);
                    }
                    if (i < scope.Length - 1)
                    {
                        var item = this.propagationScopeDescription.GetTemplateOfPropagatedScreen(scope[i + 1]);
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

        protected InterviewPropagationScopeDescription propagationScopeDescription { get; set; }
        protected IDictionary<InterviewItemId, QuestionViewModel> Questions { get; set; }
        private readonly Dictionary<Guid, HashSet<InterviewItemId>> instancesOfAnsweredQuestionsUsableAsLinkedQuestionsOptions;
        private readonly Dictionary<Guid, Guid> listofHeadQuestionsMappedOnScope = new Dictionary<Guid, Guid>(); 
        private readonly Dictionary<Guid, Guid[]> referencedQuestionToLinkedQuestionsMap;
        protected IDictionary<InterviewItemId, QuestionViewModel> FeaturedQuestions { get; set; }
        private readonly List<QuestionViewModel> allQuestionViewModels = new List<QuestionViewModel>();

        #endregion


        #region public methods

        public void UpdatePropagateGroupsByTemplate(Guid publicKey, int[] outerScopePropagationVector, int count)
        {
            var propagatedGroupsCount = this.Screens.Keys.Count(id => id.Id == publicKey) - 1;
            if (propagatedGroupsCount == count)
                return;
            for (int i = 0; i < Math.Abs(count - propagatedGroupsCount); i++)
            {
                if (propagatedGroupsCount < count)
                {
                    AddPropagateGroup(publicKey, outerScopePropagationVector, propagatedGroupsCount + i);
                }
                else
                {
                    RemovePropagatedGroup(publicKey, outerScopePropagationVector, propagatedGroupsCount - i - 1);
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
            var template = this.propagationScopeDescription.GetTemplateOfPropagatedScreen(publicKey);
            var screen = template.Clone(propagationVector);
            var questions = screen.Items.OfType<QuestionViewModel>();
            foreach (var question in questions)
            {
                UpdateQuestionHash(question);

                allQuestionViewModels.Add(question);
            }

            this.SubscribeToQuestionAnswersForQuestionsWithSubstitutionReferences();

            screen.PropertyChanged += screen_PropertyChanged;
            this.Screens.Add(screen.ScreenId, screen);
            UpdateGrid(publicKey);
        }

        private void RemovePropagatedGroup(Guid publicKey, int[] outerScopePropagationVector, int index)
        {
            var propagationVector = BuildPropagationVectorForGroup(outerScopePropagationVector,
                index);
            var key = new InterviewItemId(publicKey, propagationVector);
            var screen = this.Screens[key] as QuestionnaireScreenViewModel;
            foreach (var item in screen.Items)
            {
                var question = item as QuestionViewModel;
                if (question != null)
                {
                    this.Questions.Remove(question.PublicKey);
                    allQuestionViewModels.Remove(question);
                }
                
            }
            this.Screens.Remove(key);
            UpdateGrid(publicKey);

        }

        public IEnumerable<IQuestionnaireViewModel> RestoreBreadCrumbs(IEnumerable<InterviewItemId> breadcrumbs)
        {
            return breadcrumbs.Select(b => this.Screens[b]);
        }

        public void SetAnswer(InterviewItemId key, object answer)
        {
            QuestionViewModel question = this.Questions[key];
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

        public IEnumerable<QuestionViewModel> FindQuestion(Func<QuestionViewModel, bool> filter)
        {
            return this.Questions.Select(q => q.Value).Where(filter);
        }

        public void AddInstanceOfAnsweredQuestionUsableAsLinkedQuestionsOption(Guid questionId, int[] propagationVector)
        {
            if (!this.instancesOfAnsweredQuestionsUsableAsLinkedQuestionsOptions.ContainsKey(questionId))
                this.instancesOfAnsweredQuestionsUsableAsLinkedQuestionsOptions.Add(questionId, new HashSet<InterviewItemId>());

            var questionInstanceId = new InterviewItemId(questionId, propagationVector);

            this.instancesOfAnsweredQuestionsUsableAsLinkedQuestionsOptions[questionId].Add(questionInstanceId);

            this.NotifyAffectedLinkedQuestions(questionId);
        }

        public void RemoveInstanceOfAnsweredQuestionUsableAsLinkedQuestionsOption(Guid questionId, int[] propagationVector)
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

        #endregion

        #region event handlers

        private void question_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "AnswerString")
                return;
            var question = sender as QuestionViewModel;
            if (question==null || !listofHeadQuestionsMappedOnScope.ContainsKey(question.PublicKey.Id))
                return;

            UpdatePropagatedGroupScreenName(question.PublicKey.PropagationVector, listofHeadQuestionsMappedOnScope[question.PublicKey.Id]);
        }

        private void UpdatePropagatedGroupScreenName(int[] propagationVector, Guid scopeId)
        {
            var itemsInScope = this.propagationScopeDescription.GetTemplatesOfPropagatedScreensInScope(scopeId);
            var screens =
                Screens.Select(
                    q => q.Value).OfType<QuestionnairePropagatedScreenViewModel>().Where(
                        q => q.ScreenId.CompareWithVector(propagationVector) && itemsInScope.Contains(q.ScreenId.Id));

            var newTitle = string.Concat(
                Questions.Where(
                    q =>
                        q.Key.CompareWithVector(propagationVector) &&
                            listofHeadQuestionsMappedOnScope.Any(
                                headQuestion => headQuestion.Key == q.Key.Id && headQuestion.Value == scopeId)).
                    Select(
                        q => q.Value.AnswerString));
            foreach (var screen in screens)
            {
                screen.UpdateScreenName(newTitle);
            }
        }

        private void screen_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var propagatedScreen = sender as QuestionnaireScreenViewModel;
            if (propagatedScreen == null)
                return;
            if (propagatedScreen.ScreenId.IsTopLevel())
                return;
            UpdateGrid(propagatedScreen.ScreenId.Id);
        }

        #endregion

        #region protected helper methods

        protected void AddScreen(List<IGroup> rout,
            IGroup group)
        {
            var key = new InterviewItemId(group.PublicKey);


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
                var gridKey = new InterviewItemId(group.PublicKey);
                if (!this.Screens.ContainsKey(gridKey))
                {
                    CreateGrid(group, rout);
                }
            }

        }

        protected void UpdateQuestionHash(QuestionViewModel question)
        {
            this.Questions.Add(question.PublicKey, question);

            if (listofHeadQuestionsMappedOnScope.ContainsKey(question.PublicKey.Id))
            {
                question.PropertyChanged += question_PropertyChanged;
            }
        }

        protected void CreateGrid(IGroup group, List<IGroup> rout)
        {
            InterviewItemId rosterKey = new InterviewItemId(group.PublicKey);
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
                () => CollectPropagatedScreen(rosterKey.Id));

            breadcrumbs = breadcrumbs.Union(new InterviewItemId[1] {roster.ScreenId}).ToList();

            var template = new QuestionnairePropagatedScreenViewModel(PublicKey, group.Title, true,
                rosterKey, screenItems,
                GetSiblings,
                breadcrumbs);

            this.propagationScopeDescription.AddTemplateOfPropagatedScreen(rosterKey.Id, template);

            this.Screens.Add(rosterKey, roster);
        }

        protected IList<IQuestionnaireItemViewModel> BuildItems(IGroup screen, bool updateHash)
        {
            IList<IQuestionnaireItemViewModel> result = new List<IQuestionnaireItemViewModel>();
            foreach (var child in screen.Children)
            {
                var item = CreateView(child);
                if (item == null)
                    continue;
                var question = item as QuestionViewModel;
                if (question != null && updateHash)
                    UpdateQuestionHash(question);
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
            return
                this.Screens.Select(
                    s => s.Value)
                    .OfType<QuestionnairePropagatedScreenViewModel>()
                    .Where(s => s.ScreenId.Id == publicKey)
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
                if (IfQuestionNeedToBeSkipped(question))
                    return null;

                QuestionViewModel questionView = CreateQuestionView(question);

                this.allQuestionViewModels.Add(questionView);

                if (question.Featured)
                {
                    this.FeaturedQuestions.Add(questionView.PublicKey, questionView);
                    return null;
                }

                HandleQuestionPossibleTriggers(question);

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

        private void HandleQuestionPossibleTriggers(IQuestion question)
        {
            var trigger = question as IAutoPropagateQuestion;

            if (trigger == null)
                return;

            this.propagationScopeDescription.CreateScopeOfPropagatedScreens(question.PublicKey, trigger.Triggers);
        }

        private bool IfQuestionNeedToBeSkipped(IQuestion question)
        {
            return question.QuestionScope != QuestionScope.Interviewer && !question.Featured;
        }

        private QuestionViewModel CreateQuestionView(IQuestion question)
        {
            var newType = CalculateViewType(question.QuestionType);
            if (selectableQuestionTypes.Contains(question.QuestionType))
            {
                if (question.LinkedToQuestionId.HasValue)
                    return CreateLinkedQuestion(question, newType);
                return CreateSelectableQuestion(question, newType);
            }
            return CreateValueQuestion(question, newType);
        }

        private ValueQuestionViewModel CreateValueQuestion(IQuestion question, QuestionType newType)
        {
            return new ValueQuestionViewModel(
                new InterviewItemId(question.PublicKey), question.QuestionText,
                newType,
                null,
                true, question.Instructions, null,
                true, question.Mandatory,
                question.ValidationMessage, question.StataExportCaption, question.GetVariablesUsedInTitle());
        }

        private LinkedQuestionViewModel CreateLinkedQuestion(IQuestion question, QuestionType newType)
        {
            return new LinkedQuestionViewModel(
                new InterviewItemId(question.PublicKey), question.QuestionText,
                newType,
                true, question.Instructions,
                true, question.Mandatory,question.ValidationMessage,
                () => this.GetAnswerOptionsForLinkedQuestion(question.LinkedToQuestionId.Value), 
                question.StataExportCaption, question.GetVariablesUsedInTitle());
        }

        private SelectebleQuestionViewModel CreateSelectableQuestion(IQuestion question, QuestionType newType)
        {
            return new SelectebleQuestionViewModel(
                new InterviewItemId(question.PublicKey), question.QuestionText,
                newType, question.Answers.Select(
                    a =>
                        new AnswerViewModel(a.PublicKey, a.AnswerText, a.AnswerValue, false, a.AnswerImage))
                    .ToList(),
                true, question.Instructions, null,
                true, question.Mandatory,  null, question.ValidationMessage, question.StataExportCaption, question.GetVariablesUsedInTitle());
        }

        protected IEnumerable<LinkedAnswerViewModel> GetAnswerOptionsForLinkedQuestion(Guid referencedQuestionId)
        {
            return !this.instancesOfAnsweredQuestionsUsableAsLinkedQuestionsOptions.ContainsKey(referencedQuestionId)
                ? Enumerable.Empty<LinkedAnswerViewModel>()
                : this
                    .instancesOfAnsweredQuestionsUsableAsLinkedQuestionsOptions[referencedQuestionId]
                    .Select(instanceId => this.Questions[instanceId])
                    .Where(questionInstance => questionInstance.IsEnabled())
                    .Select(questionInstance => new LinkedAnswerViewModel(questionInstance.PublicKey.PropagationVector, questionInstance.AnswerString));
        }

        protected QuestionType CalculateViewType(QuestionType questionType)
        {
            if (singleOptionTypeVariation.Contains(questionType))
                return QuestionType.SingleOption;

            return questionType;
        }

        private readonly QuestionType[] selectableQuestionTypes = new[]
            {QuestionType.SingleOption, QuestionType.MultyOption};

        private readonly QuestionType[] singleOptionTypeVariation = new[]
            {QuestionType.SingleOption, QuestionType.DropDownList, QuestionType.YesNo};


        #endregion

        public bool IsQuestionReferencedByAnyLinkedQuestion(Guid questionId)
        {
            return this.referencedQuestionToLinkedQuestionsMap.ContainsKey(questionId)
                && this.referencedQuestionToLinkedQuestionsMap[questionId].Any();
        }
    }
}