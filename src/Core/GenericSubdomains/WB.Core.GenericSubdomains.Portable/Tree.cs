﻿using System;
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

        public static IEnumerable<T> TreeToEnumerableDepthFirst<T>(this IEnumerable<T> tree, Func<T, IEnumerable<T>> getChildren)
        {
            foreach (var branch in tree)
            {
                foreach (var child in branch.TreeToEnumerableDepthFirst(getChildren))
                {
                    yield return child;
                }
            }
        }


        public static IEnumerable<T> TreeToEnumerableDepthFirst<T>(this T root, Func<T, IEnumerable<T>> getChildren)
        {
            yield return root;

            foreach (var node in getChildren(root))
            {
                foreach (var child in TreeToEnumerableDepthFirst(node, getChildren))
                {
                    yield return child;
                }
            }
        }

        public static IEnumerable<T> AsDepthFirstEnumerable<T>(this T head, Func<T, IEnumerable<T>> childrenFunc)
        {
            yield return head;

            foreach (var node in childrenFunc(head))
            {
                foreach (var child in AsDepthFirstEnumerable(node, childrenFunc))
                {
                    yield return child;
                }
            }

        }
    }
}