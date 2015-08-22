using System;
using System.Collections.Generic;
using System.Linq;
using Android.Views;
using Cirrious.MvvmCross.Binding.BindingContext;

namespace WB.UI.Interviewer.Extensions
{
    public static class ViewExtensions
    {
        public static void DisposeChildrenAndCleanUp(this ViewGroup view)
        {
            var rowsToDispose = new List<View>();
            for (int i = 0; i < view.ChildCount; i++)
            {
                rowsToDispose.Add(view.GetChildAt(i));
            }

            view.RemoveAllViews();

            foreach (var child in rowsToDispose)
            {
                child.Dispose();
            }
        }

        public static void TryClearBindingsIfPossible(this View view)
        {
            var boundChild = view as IMvxBindingContextOwner;
            if (boundChild != null)
            {
#if DEBUG
                Console.WriteLine("clean up binding from adapter");
#endif
                boundChild.ClearAllBindings();
            }
            var groupViewChild = view as ViewGroup;
            if (groupViewChild != null)
            {

#if DEBUG
                Console.WriteLine("clean up binding from adapter for children");
#endif
                TryClearBindingsIfPossibleForChildren(groupViewChild);
            }
        }
        public static void TryClearBindingsIfPossibleForChildren(this ViewGroup view)
        {
            for (int i = 0; i < view.ChildCount; i++)
            {
                view.GetChildAt(i).TryClearBindingsIfPossible();
            }
        }

        public static void EnableDisableView(this View view, bool enabled)
        {
            bool parentEnabled = true;
            var parentView = view.Parent as View;
            if (parentView != null)
                parentEnabled = parentView.Enabled;
            view.Enabled = parentEnabled && enabled;
            ViewGroup group = view as ViewGroup;
            if (@group != null)
            {

                for (int idx = 0; idx < @group.ChildCount; idx++)
                {
                    EnableDisableView(@group.GetChildAt(idx), enabled);
                }
            }
        }

        public static IEnumerable<View> GetChildren(this ViewGroup container)
        {
            return Enumerable.Range(0, container.ChildCount)
                             .Select(container.GetChildAt);
        }
    }
}