namespace Main.Core.View.CompleteQuestionnaire.ScreenGroup
{
    using System;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities.Composite;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.ExpressionExecutors;
    using Main.Core.View.Group;
    using Main.Core.View.Question;

    public class ScreenGroupView
    {
        #region Constructors and Destructors
        public ScreenGroupView(
            CompleteQuestionnaireStoreDocument doc, ICompleteGroup currentGroup, ScreenNavigation navigation, QuestionScope scope)
            : this(
                doc,
                currentGroup,
                new ScreenNavigationView(
                    doc.Children.OfType<ICompleteGroup>().Where(g => g.HasVisibleItemsForScope(scope)).Select(g => new CompleteGroupHeaders(g)),
                    navigation))
        {
            /*var executor = new CompleteQuestionnaireConditionExecutor(doc);
            executor.ExecuteAndChangeStateRecursive(doc);*/

            var validator = new CompleteQuestionnaireValidationExecutor(doc, scope);
            validator.Execute(currentGroup);

            this.BuildScreenContent(doc, currentGroup, scope);
        }

        protected ScreenGroupView(
            CompleteQuestionnaireStoreDocument doc, ICompleteGroup currentGroup, ScreenNavigationView navigation)
        {
            this.QuestionnairePublicKey = doc.PublicKey;
            this.PublicKey = currentGroup.PublicKey;
            this.Title = currentGroup.Title;
            this.Status = doc.Status;
            this.Description = currentGroup.Description;
            this.Navigation = navigation;
        }


        public CompleteGroupMobileView Group { get; set; }

        public bool IsQuestionnaireActive
        {
            get { return !SurveyStatus.IsStatusAllowCapiSync(this.Status); }
        }

        public Guid QuestionnairePublicKey { get; set; }

        public Guid PublicKey { get; set; }

        public string Description { get; set; }

        public string Title { get; set; }

        public SurveyStatus Status { get; set; }

        public ScreenNavigationView Navigation { get; set; }

        private void BuildScreenContent(CompleteQuestionnaireStoreDocument doc, ICompleteGroup currentGroup, QuestionScope scope)
        {
            if (currentGroup.PropagationPublicKey.HasValue)
            {
                this.Group = new PropagatedGroupMobileView(doc, currentGroup, scope);
                return;
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
            foreach (IComposite composite in currentGroup.Children)
            {
                if ((composite as ICompleteQuestion) != null)
                {
                    var q = composite as ICompleteQuestion;
                    if (q.QuestionScope <= scope)
                    {
                        var question = new CompleteQuestionView(doc, q);
                        if (q.QuestionScope == scope) question.Editable = true;
                    }
                }
                else
                {
                    var g = composite as CompleteGroup;
                    if (g.Propagated == Propagate.None)
                    {
                        this.Group.Children.Add(new CompleteGroupMobileView(doc, g, scope));
                    }
                    else if (!g.PropagationPublicKey.HasValue)
                    {
                        var propagatedGroup = new CompleteGroupMobileView(doc, g, scope);
                        this.Group.Children.Add(propagatedGroup);
                        var subGroups = currentGroup.Children.OfType<ICompleteGroup>().Where(
                            p =>
                            p.PublicKey == g.PublicKey && p.PropagationPublicKey.HasValue);
                        foreach (
                            ICompleteGroup completeGroup in subGroups)
                        {
                            propagatedGroup.Children.Add(new PropagatedGroupMobileView(doc, completeGroup, scope));
                        }
                    }
                }
            }
        }

        #endregion
    }
}
