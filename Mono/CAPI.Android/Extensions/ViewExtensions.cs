using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding.BindingContext;

namespace CAPI.Android.Extensions
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
                Console.WriteLine("clean up binding from adapter");
                boundChild.ClearAllBindings();
            }
            var groupViewChild = view as ViewGroup;
            if (groupViewChild != null)
            {
                Console.WriteLine("clean up binding from adapter for children");
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
    }
}