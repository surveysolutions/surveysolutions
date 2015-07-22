using System;
using System.Collections.Generic;

namespace WB.Core.GenericSubdomains.Portable
{
    public static class Tree
    {
        public static IEnumerable<T> TreeToEnumerable<T>(this T root, Func<T, IEnumerable<T>> getChildren)
        {
            return new[] { root }.TreeToEnumerable(getChildren);
        }

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

        public static IEnumerable<T> UnwrapReferences<T>(this T startItem, Func<T, T> getReferencedItem) where T: class
        {
            T referencedItem = startItem;
            while (referencedItem != null)
            {
                yield return referencedItem;
                referencedItem = getReferencedItem(referencedItem);
            }
        }
    }
}