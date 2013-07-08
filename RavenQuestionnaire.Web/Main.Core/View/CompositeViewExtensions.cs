using System;
using System.Collections.Generic;
using System.Linq;

namespace Main.Core.View
{
    public static class CompositeViewExtensions
    {
        public static int GetDepth(this ICompositeView view)
        {
            if (view == null) throw new ArgumentNullException("view");
            int result = 0;
            var parent = view.ParentView;
            while (parent != null)
            {
                result++;
                parent = parent.ParentView;
            }

            return result;
        }

        public static IEnumerable<ICompositeView> Descendants(this ICompositeView root)
        {
            var nodes = new Stack<ICompositeView>(new[] { root });
            while (nodes.Any())
            {
                ICompositeView node = nodes.Pop();
                yield return node;
                if (node.Children != null)
                {
                    foreach (var n in node.Children)
                    {
                        nodes.Push(n);
                    }
                }
            }
        }
    }
}