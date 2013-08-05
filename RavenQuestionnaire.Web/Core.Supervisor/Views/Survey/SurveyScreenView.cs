namespace Core.Supervisor.Views.Survey
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.ExpressionExecutors;
    using Main.Core.Utility;
    using Main.Core.View.Group;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SurveyScreenView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyScreenView"/> class.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="currentGroup">
        /// The current group.
        /// </param>
        /// <param name="navigation">
        /// The navigation.
        /// </param>
        /// <param name="scope">
        /// The scope.
        /// </param>
        public SurveyScreenView(
            CompleteQuestionnaireStoreDocument doc, ICompleteGroup currentGroup, ScreenNavigationView navigation, QuestionScope scope)
        {
            this.QuestionnairePublicKey = doc.PublicKey;
            this.PublicKey = currentGroup.PublicKey;
            this.Title = doc.Title;
            this.Status = doc.Status;
            this.Description = currentGroup.Description;
            this.Navigation = navigation;
            this.Responsible = doc.Responsible;

            var validator = new CompleteQuestionnaireValidationExecutor(doc, scope);
            validator.Execute(doc);
            this.Screens = new List<SurveyScreen>();
            this.BuildScreenContent(doc, currentGroup, scope);            
            this.StatusHistory = doc.StatusChangeComments.Select(s => new ChangeStatusHistoryView(s.Responsible, s.Status, s.ChangeDate)).ToList();
        }

        public UserLight Responsible { get; set; }

        /// <summary>
        /// Gets or sets Screens.
        /// </summary>
        public List<SurveyScreen> Screens { get; set; }

        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        public CompleteGroupMobileView Group { get; set; }

        /// <summary>
        /// get or set questionnaire active status - active if allow to edit, not error or completed
        /// </summary>
        public bool IsQuestionnaireActive
        {
            get { return !SurveyStatus.IsStatusAllowCapiSync(this.Status); }
        }

        /// <summary>
        /// Gets or sets the questionnaire public key.
        /// </summary>
        public Guid QuestionnairePublicKey { get; set; }

        /// <summary>
        /// Gets or sets PublicKey.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets Description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets Title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets Status.
        /// </summary>
        public SurveyStatus Status { get; set; }

        /// <summary>
        /// Gets or sets Navigation.
        /// </summary>
        public ScreenNavigationView Navigation { get; set; }

        public UserLight User { get; set; }

        public List<ChangeStatusHistoryView> StatusHistory { get; set; }

        /// <summary>
        /// The build screen content.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="currentGroup">
        /// The current group.
        /// </param>
        /// <param name="scope">
        /// The scope.
        /// </param>
        private void BuildScreenContent(CompleteQuestionnaireStoreDocument doc, ICompleteGroup currentGroup, QuestionScope scope)
        {
            if (SurveyStatus.Unassign.PublicId != doc.Status.PublicId &&
                SurveyStatus.Initial.PublicId != doc.Status.PublicId)
            {
                var validator = new CompleteQuestionnaireValidationExecutor(doc, QuestionScope.Interviewer);
                validator.Execute(doc);
            }

            this.Group = new CompleteGroupMobileView()
            {
                PublicKey = currentGroup.PublicKey,
                Title = currentGroup.Title,
                Propagated = currentGroup.Propagated,
                Enabled = currentGroup.Enabled,
                Description = currentGroup.Description,
                QuestionnairePublicKey = doc.PublicKey
            };

            var treeStack = new Stack<NodeWithLevel>();
            treeStack.Push(new NodeWithLevel(currentGroup, 0));

            var screens = new List<SurveyScreen>();

            while (treeStack.Count > 0)
            {
                NodeWithLevel node = treeStack.Pop();

                if (node.Group.Propagated != Propagate.None && !node.Group.PropagationPublicKey.HasValue)
                {
                    continue;
                }

                var screen = new SurveyScreen(doc, node);
                screens.Add(screen);

                var subGroups = node.Group.Children.OfType<ICompleteGroup>().Reverse().ToArray();
                foreach (var completeGroup in subGroups.Where(completeGroup => completeGroup.HasVisibleItemsForScope(scope)))
                {
                    var validator = new CompleteQuestionnaireValidationExecutor(doc, QuestionScope.Interviewer);
                    validator.Execute(completeGroup);
                    treeStack.Push(new NodeWithLevel(completeGroup, node.Level + 1));
                }
            }

            var groupedScreens = screens.GroupBy(s => s.Key.PublicKey)
                .ToDictionary(g => g.Key, g => g.ToList())
                .Values.ToList();

            foreach (var screenList in groupedScreens)
            {
                var screen = screenList.First();
                foreach (var surveyScreen in screenList.Skip(1))
                {
                    if (screen.Captions.ContainsKey(surveyScreen.Captions.Keys.First()))
                        continue;
                    for (int i = 0; i < surveyScreen.Questions.Count; i++)
                    {
                        screen.Questions[i].Answers.Add(surveyScreen.Questions[i].Answers[0]);
                    }

                    screen.Captions.Add(surveyScreen.Captions.Keys.First(), surveyScreen.Captions.Values.First());
                }

                if (screen.Key.Propagated != Propagate.None)
                {
                    screen.Key.PropagationKey = Guid.Empty;
                }
                
                this.Screens.Add(screen);
            }
        }

        #endregion
    }
}
