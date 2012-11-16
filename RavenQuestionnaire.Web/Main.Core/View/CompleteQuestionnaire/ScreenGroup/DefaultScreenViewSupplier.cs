// -----------------------------------------------------------------------
// <copyright file="DefaultScreenViewSupplier.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.View.Group;

namespace Main.Core.View.CompleteQuestionnaire.ScreenGroup
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class DefaultScreenViewSupplier : IScreenViewSupplier
    {
        #region Implementation of IScreenViewSupplier

        /// <summary>
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
        /// </returns>
        public virtual ScreenGroupView BuildView(CompleteQuestionnaireStoreDocument doc, ICompleteGroup currentGroup, ScreenNavigation navigation, QuestionScope scope)
        {
            return new ScreenGroupView(doc, currentGroup, navigation, scope);
        }

        #endregion
    }
}
