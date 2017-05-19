using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Java.Lang;
using MvvmCross.Core.Platform;
using MvvmCross.Core.ViewModels;
using MvvmCross.Droid.Support.V4;
using WB.Core.BoundedContexts.Interviewer.Views;

namespace WB.UI.Interviewer.Activities
{
    public class MvxFragmentStatePagerAdapter : FragmentStatePagerAdapter
    {
        private readonly Context _context;

        public IEnumerable<FragmentInfo> Fragments { get; private set; }

        public override int Count => this.Fragments.Count();

        protected MvxFragmentStatePagerAdapter(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public MvxFragmentStatePagerAdapter(
            Context context, FragmentManager fragmentManager, IEnumerable<FragmentInfo> fragments)
            : base(fragmentManager)
        {
            this._context = context;
            this.Fragments = fragments;
        }

        public override Fragment GetItem(int position)
        {
            var fragmentInfo = this.Fragments.ElementAt(position);
            var fragment = Fragment.Instantiate(this._context,
                FragmentJavaName(fragmentInfo.FragmentType));
            ((MvxFragment)fragment).ViewModel = fragmentInfo.ViewModel;
            return fragment;
        }

        protected static string FragmentJavaName(Type fragmentType)
        {
            var namespaceText = fragmentType.Namespace ?? "";
            if (namespaceText.Length > 0)
                namespaceText = namespaceText.ToLowerInvariant() + ".";
            return namespaceText + fragmentType.Name;
        }

        public override ICharSequence GetPageTitleFormatted(int position)
            => new Java.Lang.String(this.Fragments.ElementAt(position).Title);

        public class FragmentInfo
        {
            public Type FragmentType { get; set; }
            public MvxViewModel ViewModel { get; set; }
            public string Title { get; set; }
        }
    }
}