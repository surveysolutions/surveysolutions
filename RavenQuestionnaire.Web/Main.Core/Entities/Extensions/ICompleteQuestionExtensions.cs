// -----------------------------------------------------------------------
// <copyright file="ICompleteQuestionExtensions.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Main.Core.Entities.SubEntities.Complete;

namespace Main.Core.Entities.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class ICompleteQuestionExtensions
    {
        public static bool IsValueQuestion(this ICompleteQuestion question)
        {
            return question.GetType().GetInterfaces().Any(x => x.IsGenericType &&
                                                               x.GetGenericTypeDefinition() ==
                                                               typeof (ICompelteValueQuestion<>));
        }
    }
}
