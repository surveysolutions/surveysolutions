using System.Collections.Generic;
using System.Linq;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf
{
    public static class Extensions
    {
        public static IEnumerable<T> TreeToEnumerable<T>(this IEnumerable<T> tree) where T : PdfEntityView
        {
            var groups = new Stack<T>(tree);

            while (groups.Count > 0)
            {
                var group = groups.Pop();

                yield return group;
                foreach (T childGroup in group.Children.OfType<T>())
                {
                    groups.Push(childGroup);
                }
            }
        }
    }
}