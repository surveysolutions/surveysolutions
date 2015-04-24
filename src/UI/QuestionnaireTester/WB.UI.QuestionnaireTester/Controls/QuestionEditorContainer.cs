using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Binding.Attributes;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.Views;
using WB.UI.QuestionnaireTester.Views.Adapters;

namespace WB.UI.QuestionnaireTester.Controls
{
    public class QuestionEditorContainer : FrameLayout, IMvxBindingContextOwner
    {
        private readonly IMvxAndroidBindingContext bindingContext;

        public QuestionEditorContainer(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            if (!(context is IMvxLayoutInflater))
            {
                throw Mvx.Exception("The owning Context for a MvxFrameControl must implement LayoutInflater");
            }

            var viewAdaptor = Mvx.Create<IQuestionEditorViewAdapter>();
            if (viewAdaptor == null)
            {
                throw Mvx.Exception("QuestionEditorContainer requires to setup IQuestionEditorViewAdapter");
            }

            this.bindingContext = new MvxAndroidBindingContext(context, (IMvxLayoutInflater)context);
            this.DelayBind(() =>
            {
                if (this.Content != null || this.DataContext == null) return;

                Mvx.Trace("DataContext is {0}", this.DataContext.ToString());
                var templateId = viewAdaptor.GetItemViewType(this.DataContext);
                if (templateId != 0)
                {
                    this.Content = this.bindingContext.BindingInflate(templateId, this);
                }
            });
        }

        protected QuestionEditorContainer(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        protected IMvxAndroidBindingContext AndroidBindingContext
        {
            get { return this.bindingContext; }
        }

        public IMvxBindingContext BindingContext
        {
            get { return this.bindingContext; }
            set { throw new NotImplementedException("BindingContext is readonly in the list item"); }
        }

        private object cachedDataContext;
        private bool isAttachedToWindow;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.ClearAllBindings();
                this.cachedDataContext = null;
            }

            base.Dispose(disposing);
        }

        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();
            this.isAttachedToWindow = true;
            if (this.cachedDataContext != null
                && this.DataContext == null)
            {
                this.DataContext = this.cachedDataContext;
            }
        }

        protected override void OnDetachedFromWindow()
        {
            this.cachedDataContext = this.DataContext;
            this.DataContext = null;
            base.OnDetachedFromWindow();
            this.isAttachedToWindow = false;
        }


        protected View Content { get; set; }

        [MvxSetToNullAfterBinding]
        public object DataContext
        {
            get { return this.bindingContext.DataContext; }
            set
            {
                if (this.isAttachedToWindow)
                {
                    this.bindingContext.DataContext = value;
                }
                else
                {
                    this.cachedDataContext = value;
                    if (this.bindingContext.DataContext != null)
                    {
                        this.bindingContext.DataContext = null;
                    }
                }
            }
        }
    }
}