// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICompositeView.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The CompositeView interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Main.Core.Documents;

namespace Main.Core.View
{
    public interface ICompositeView
    {
        List<ICompositeView> Children { get; set; }

        Guid? Parent { get; set; }
        
        Guid PublicKey { get; set; }

        string Title { get; set; }

        ICompositeView ParentView { get; }
    }

    public static class CompositeViewExtensions
    {
        public static int GetDepthIn(this ICompositeView view)
        {
            int result = 0;
            var parent = view.ParentView;
            while (parent != null)
            {
                result++;
                parent = parent.ParentView;
            }

            return result;
        }
    }
}