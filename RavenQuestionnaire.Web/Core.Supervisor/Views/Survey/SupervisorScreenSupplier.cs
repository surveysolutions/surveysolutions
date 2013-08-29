namespace Core.Supervisor.Views.Survey
{
    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.View.Group;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SupervisorScreenSupplier : ISurveyScreenSupplier
    {
        #region Implementation of ISurveyScreenSupplier

        /// <summary>
        /// The Build View
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
        /// <returns>
        /// The SurveyScreenView
        /// </returns>
        public virtual SurveyScreenView BuildView(CompleteQuestionnaireStoreDocument doc, ICompleteGroup currentGroup, ScreenNavigationView navigation)
        {
            return new SurveyScreenView(doc, currentGroup, navigation, QuestionScope.Supervisor);
        }

        #endregion
    }
}
