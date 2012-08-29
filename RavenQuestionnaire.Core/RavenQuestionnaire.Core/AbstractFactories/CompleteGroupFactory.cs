// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteGroupFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   Factory creates ICompleteGroup.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.AbstractFactories
{
    using System;

    using RavenQuestionnaire.Core.Documents;
    using RavenQuestionnaire.Core.Entities.SubEntities;
    using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
    using RavenQuestionnaire.Core.Views.Group;

    /// <summary>
    /// Factory creates ICompleteGroup.
    /// </summary>
    public class CompleteGroupFactory : ICompleteGroupFactory
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
        /// <exception cref="ArgumentException">
        /// Rises ArgumentException.
        /// </exception>
        public ICompleteGroup ConvertToCompleteGroup(IGroup group)
        {
            var result = group as ICompleteGroup;
            if (result != null)
            {
                return result;
            }

            var simpleGroup = group as Group;
            if (simpleGroup != null)
            {
                return (CompleteGroup)simpleGroup;
            }

            throw new ArgumentException();
        }

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
        public CompleteGroupView CreateGroup(CompleteQuestionnaireStoreDocument doc, ICompleteGroup group)
        {
            //// PropagatableCompleteGroup propagatableGroup = group as PropagatableCompleteGroup;
            if (group.PropogationPublicKey.HasValue)
            {
                return new PropagatableCompleteGroupView(doc, group, this);
            }

            return new CompleteGroupView(doc, group, this);
        }

        #endregion
    }
}