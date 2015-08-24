using System;
using Android.Views;
using Cirrious.MvvmCross.Binding.BindingContext;

namespace WB.UI.Interviewer.Extensions
{
    public static class ViewExtensions
    {
        
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
    }
}