using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Services.Export.Questionnaire
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
    }
}
