namespace Main.Core.View.CompleteQuestionnaire.ScreenGroup
{
    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.View.Group;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface IScreenViewSupplier
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
        /// <param name="scope">
        /// The scope.
        /// </param>
        /// <returns>
        /// The <see cref="ScreenGroupView"/>.
        /// </returns>
        ScreenGroupView BuildView(CompleteQuestionnaireStoreDocument doc, ICompleteGroup currentGroup, ScreenNavigation navigation, QuestionScope scope);
    }
}
