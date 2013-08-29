namespace Core.Supervisor.Views.Survey
{
    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.View.CompleteQuestionnaire.ScreenGroup;
    using Main.Core.View.Group;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface ISurveyScreenSupplier
    {
        /// <summary>
        /// The build view.
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
        /// The <see cref="ScreenGroupView"/>.
        /// </returns>
        SurveyScreenView BuildView(CompleteQuestionnaireStoreDocument doc, ICompleteGroup currentGroup, ScreenNavigationView navigation);
    }
}
