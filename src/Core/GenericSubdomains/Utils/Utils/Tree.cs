using System;
using System.Collections.Generic;

namespace WB.Core.GenericSubdomains.Utils
{
    public static class Tree
    {
        public static IEnumerable<T> TreeToEnumerable<T>(this IEnumerable<T> tree, Func<T, IEnumerable<T>> getChildren)
        {
            var itemsStack = new Stack<T>(tree);

            while (itemsStack.Count > 0)
            {
                var currentItem = itemsStack.Pop();

                yield return currentItem;
                foreach (T childItem in getChildren(currentItem))
                {
                    itemsStack.Push(childItem);
                }
            }
        }
    }
}