// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICompleteGroupFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The CompleteGroupFactory interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.AbstractFactories
{
    using RavenQuestionnaire.Core.Documents;
    using RavenQuestionnaire.Core.Entities.SubEntities;
    using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
    using RavenQuestionnaire.Core.Views.Group;

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
        /// The RavenQuestionnaire.Core.Entities.SubEntities.Complete.ICompleteGroup.
        /// </returns>
        ICompleteGroup ConvertToCompleteGroup(IGroup group);

        /// <summary>
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
        CompleteGroupView CreateGroup(CompleteQuestionnaireStoreDocument doc, ICompleteGroup group);

        #endregion
    }
}