namespace Main.Core.ExpressionExecutors
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Entities.Composite;
    using Main.Core.Entities.SubEntities;
    using Main.Core.ExpressionExecutors.ExpressionExtentions;

    using NCalc;
    using NCalc.Domain;

    /// <summary>
    /// The expression dependency builder.
    /// </summary>
    public class ExpressionDependencyBuilder
    {
        /// <summary>
        /// The handle tree.
        /// </summary>
        /// <param name="node">
        /// The node.
        /// </param>
        /// <param name="dependentQuestions">
        /// The dependent questions.
        /// </param>
        /// <param name="dependentGroups">
        /// The dependent groups.
        /// </param>
        public static void HandleTree(IComposite node, Dictionary<Guid, List<Guid>> dependentQuestions, Dictionary<Guid, List<Guid>> dependentGroups)
        {
            if (node == null)
            {
                return;
            }

            List<Guid> elements = HandleNode(node);

            if (elements != null && elements.Count > 0)
            {
                if (node is IGroup)
                {
                    Merge(node.PublicKey, elements, dependentGroups);
                }
                else
                {
                    Merge(node.PublicKey, elements, dependentQuestions);
                }
            }

            if (node.Children != null)
            {
                foreach (IComposite child in node.Children)
                {
                    HandleTree(child, dependentQuestions, dependentGroups);
                }
            }
        }

        /// <summary>
        /// The handle node.
        /// </summary>
        /// <param name="node">
        /// The node.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        private static List<Guid> HandleNode(IComposite node)
        {
            var item = node as IConditional;
            if (item != null)
            {
               if (!string.IsNullOrWhiteSpace(item.ConditionExpression))
               {
                   return GetDependentItems(item.ConditionExpression);
               }
            }

            return null;
        }

        /// <summary>
        /// The get dependent items.
        /// </summary>
        /// <param name="conditionExpression">
        /// The condition expression.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        private static List<Guid> GetDependentItems(string conditionExpression)
        {
            if (!string.IsNullOrWhiteSpace(conditionExpression))
            {
                //// temp store for current condition.
                //// do not add if condition is failed to execute
                List<Guid> tempStore = new List<Guid>();

                var expression = new Expression(conditionExpression);
                
                expression.EvaluateParameter += (name, args) =>
                    {
                        Guid refItem;
                        if (Guid.TryParse(name, out refItem))
                        {
                            if (!tempStore.Contains(refItem))
                            {
                                tempStore.Add(refItem);
                            }
                        }

                        // dummy value for evaluation
                        // for correct collecting
                        // asumming this answer is not
                        args.Result = "0";
                    };

                expression.EvaluateFunction += ExtensionFunctions.EvaluateFunctionContains;

                try
                {
                    expression.Evaluate(new EvaluationTesterVisitor(expression.Options));
                    return tempStore;
                }
                catch
                {
                    #warning no exceptions should be ignored without at least writing to log
                    ////what must we do if expression was fault to evaluate?
                }
            }

            return null;
        }

        /// <summary>
        /// The merge.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="donor">
        /// The donor.
        /// </param>
        /// <param name="acceptor">
        /// The acceptor.
        /// </param>
        private static void Merge(Guid key, IEnumerable<Guid> donor, Dictionary<Guid, List<Guid>> acceptor)
        {
            foreach (var element in donor)
            {
                if (!acceptor.ContainsKey(element))
                {
                    acceptor.Add(element, new List<Guid>() { key });
                }
                else
                {
                    if (!acceptor[element].Contains(key))
                    {
                        acceptor[element].Add(key);
                    }
                }
            }
        }
    }
}
