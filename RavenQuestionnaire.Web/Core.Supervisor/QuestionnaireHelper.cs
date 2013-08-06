using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;

namespace Core.Supervisor
{
    public static class QuestionnaireHelper
    {
        public static List<IQuestion> GetFeaturedQuestions(this IQuestionnaireDocument document)
        {
            return document.GetAllQuestions().Where(x => x.Featured).ToList();
        }

        public static IEnumerable<IQuestion> GetAllQuestions(this IQuestionnaireDocument document, bool skipPropagateGroups = false)
        {
            var treeStack = new Stack<IComposite>();
            treeStack.Push(document);
            while (treeStack.Count > 0)
            {
                var node = treeStack.Pop();

                foreach (var child in node.Children)
                {
                    if (child is IGroup)
                    {
                        if (!skipPropagateGroups || ((IGroup) child).Propagated == Propagate.None)
                        {
                            treeStack.Push(child);
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