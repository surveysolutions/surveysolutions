namespace Main.Core.AbstractFactories
{
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;

    /// <summary>
    /// The CompleteGroupFactory interface.
    /// </summary>
    public interface ICompleteGroupFactory
    {
        #region Public Methods and Operators

        /// <summary>
        /// The convert to complete group.
        /// </summary>
        /// <param name="group">
        /// The group.
        /// </param>
        /// <returns>
        /// The Main.Core.Entities.SubEntities.Complete.ICompleteGroup.
        /// </returns>
        ICompleteGroup ConvertToCompleteGroup(IGroup group);

        #endregion

        /*/// <summary>
        /// The create group.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="group">
        /// The group.
        /// </param>
        /// <returns>
        /// The RavenQuestionnaire.Core.Views.Group.CompleteGroupView.
        /// </returns>
        CompleteGroupView CreateGroup(CompleteQuestionnaireStoreDocument doc, ICompleteGroup group);*/
    }
}