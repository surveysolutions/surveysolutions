// -----------------------------------------------------------------------
// <copyright file="IScreenViewSupplier.cs" company="">
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
    public interface IScreenViewSupplier
    {
        ScreenGroupView BuildView(CompleteQuestionnaireStoreDocument doc, ICompleteGroup currentGroup,
                                          ScreenNavigation navigation);
    }
}
