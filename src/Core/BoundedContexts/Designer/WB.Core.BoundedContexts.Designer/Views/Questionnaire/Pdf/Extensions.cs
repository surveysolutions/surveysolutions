using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf
{
    public static class Extensions
    {
        public static IEnumerable<T> TreeToEnumerable<T>(this IEnumerable<T> tree) where T : PdfEntityView
        {
            return Tree.TreeToEnumerable(tree, item => item.Children.OfType<T>());
        }
    }
}