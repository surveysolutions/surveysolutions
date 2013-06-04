using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;

namespace LoadTestDataGenerator
{
    public static class QuestionnaireHelper
    {
        public static List<IQuestion> GetFeaturedQuestions(this QuestionnaireDocument document)
        {
            return document.GetAllQuestions().Where(x => x.Featured).ToList();
        }

        public static IEnumerable<IQuestion> GetAllQuestions(this QuestionnaireDocument document)
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
                        treeStack.Push(child);
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