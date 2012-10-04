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

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class DefaultScreenViewSupplier:IScreenViewSupplier
    {
        #region Implementation of IScreenViewSupplier

        public virtual ScreenGroupView BuildView(CompleteQuestionnaireStoreDocument doc, ICompleteGroup currentGroup, ScreenNavigation navigation)
        {
            return new ScreenGroupView(doc, currentGroup, navigation);
        }

        #endregion
    }
}
