#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

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
            var itemsQueue = new Queue<T>(tree);

            while (itemsQueue.Count > 0)
            {
                T currentItem = itemsQueue.Dequeue();

                yield return currentItem;

                IEnumerable<T> childItems = getChildren(currentItem);

                if (childItems != null)
                {
                    foreach (T childItem in childItems)
                    {
                        itemsQueue.Enqueue(childItem);
                    }
                }
            }
        }

        public static IEnumerable<T> TreeToEnumerableDepthFirst<T>(this T root, Func<T, IEnumerable<T>> getChildren)
        {
            return new[] { root }.TreeToEnumerableDepthFirst(getChildren);
        }
        
        public static IEnumerable<T> TreeToEnumerableDepthFirst<T>(this IEnumerable<T> tree, Func<T, IEnumerable<T>> getChildren)
        {
            var itemsQueue = new Stack<T>(tree.Reverse());

            while (itemsQueue.Count > 0)
            {
                T currentItem = itemsQueue.Pop();

                yield return currentItem;

                IEnumerable<T> childItems = getChildren(currentItem);

                if (childItems != null)
                {
                    foreach (T childItem in childItems.Reverse())
                    {
                        itemsQueue.Push(childItem);
                    }
                }
            }
        }
        
        public static IEnumerable<T> UnwrapReferences<T>(this T startItem, Func<T, T?> getReferencedItem) where T : class
        {
            T? referencedItem = startItem;
            while (referencedItem != null)
            {
                yield return referencedItem;
                referencedItem = getReferencedItem(referencedItem);
            }
        }

        public static void ForEachTreeElement<T>(this T root, Func<T, IEnumerable<T>> getChildren, Action<T, T> parentWithChildren)
        {
            var stack = new Stack<Tuple<T, T>>();
            stack.Push(new Tuple<T, T>(default(T)!, root));
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                parentWithChildren(current.Item1, current.Item2);
                IEnumerable<T> childItems = getChildren(current.Item2);
                childItems = childItems.Reverse();
                foreach (var child in childItems)
                    stack.Push(new Tuple<T, T>(current.Item2, child));
            }
        }
    }
}
