using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Widget;
using Cirrious.CrossCore.Droid;
using Cirrious.MvvmCross.Binding.Bindings;
using Cirrious.MvvmCross.Binding.Droid.ResourceHelpers;


namespace WB.UI.QuestionnaireTester.CustomControls.MvxBindableAutoCompleteTextViewControl
{
    public class MvxBindableListItemView
        : FrameLayout
        , IMvxBindableListItemView
    {
        private readonly View _content;
        private readonly IMvxBindingActivity _bindingActivity;

        private readonly int _templateId;

        public MvxBindableListItemView(Context context, IMvxBindingActivity bindingActivity, int templateId, object source)
            : base(context)
        {
            _templateId = templateId;
            _bindingActivity = bindingActivity;
            _content = _bindingActivity.BindingInflate(source, templateId, this);
        }

        public void ClearBindings()
        {
            _bindingActivity.ClearBindings(this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ClearBindings();
            }

            base.Dispose(disposing);
        }

        protected View Content { get { return _content; } }

        #region IMvxBindableListItemView Members

        public int TemplateId
        {
            get { return _templateId; }
        }

        public virtual void BindTo(object source)
        {
            Dictionary<View, IList<IMvxUpdateableBinding>> bindings;
            if (!TryGetJavaBindingContainer(out bindings))
            {
                return;
            }

            foreach (var binding in bindings)
            {
                foreach (var bind in binding.Value)
                {
                    bind.DataContext = source;
                }
            }
        }

        private bool TryGetJavaBindingContainer(out Dictionary<View, IList<IMvxUpdateableBinding>> result)
        {
            result = null;

            if (_content == null)
            {
                return false;
            }

            var tag = _content.GetTag(MvxAndroidBindingResource.Instance.BindingTagUnique);
            if (tag == null)
            {
                return false;
            }


            var wrappedResult = tag as MvxJavaContainer<Dictionary<View, IList<IMvxUpdateableBinding>>>;
            if (wrappedResult == null)
            {
                return false;
            }

            result = wrappedResult.Object;
            return true;
        }

        #endregion
    }
}