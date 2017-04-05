using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;

namespace WB.Core.SharedKernels.DataCollection.Utils
{
    public static class CompositeHelper
    {
        public static List<IQuestion> GetFeaturedQuestions(this IComposite document)
        {
            return document.GetAllQuestions().Where(x => x.Featured).ToList();
        }

        public static IEnumerable<IQuestion> GetAllQuestions(this IComposite element, bool skipPropagateGroups = false)
        {
            var treeStack = new Stack<IComposite>();
            treeStack.Push(element);
            while (treeStack.Count > 0)
            {
                var node = treeStack.Pop();

                foreach (var child in node.Children)
                {
                    var @group = child as IGroup;
                    if (@group != null)
                    {
                        if (!skipPropagateGroups || !@group.IsRoster)
                        {
                            treeStack.Push(@group);
                        }
                    }
                    else if (child is IQuestion)
                    {
                        yield return (child as IQuestion);
                    }
                }
            }
        }
    }
}