using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Content;
using Android.Views;
using Android.Widget;
using AndroidApp.ViewModel.QuestionnaireDetails;
using AndroidApp.ViewModel.QuestionnaireDetails.GridItems;
using Cirrious.MvvmCross.Binding.Droid.ExtensionMethods;
using Cirrious.MvvmCross.Binding.Droid.Interfaces.Views;

namespace AndroidApp.Controls.QuestionnaireDetails.Roster
{
    public class RosterQuestionView : LinearLayout
    {

        protected QuestionViewModel Model { get; private set; }
        protected View Content { get; set; }

        public virtual void BindTo(object source)
        {
            BindViewTo(Content, source);
        }

        protected static void BindViewTo(View view, object source)
        {
            IDictionary<View, IList<Cirrious.MvvmCross.Binding.Interfaces.IMvxUpdateableBinding>> bindings;
            if (!TryGetJavaBindingContainer(view, out bindings))
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
        private static bool TryGetJavaBindingContainer(View view, out IDictionary<View, IList<Cirrious.MvvmCross.Binding.Interfaces.IMvxUpdateableBinding>> result)
        {
            return view.TryGetStoredBindings(out result);
        }
        private readonly IMvxBindingActivity _bindingActivity;
        public RosterQuestionView(Context context, IMvxBindingActivity bindingActivity, QuestionViewModel source)
            : base(context)
        {
            this.Model = source;
            this._bindingActivity = bindingActivity;
            Content = bindingActivity.BindingInflate(source, Resource.Layout.RosterQuestion, this);
            this.Click += rowViewItem_Click;
            this.Enabled = this.Model.Enabled;
            this.SetBackgroundResource(Resource.Drawable.grid_headerItem);
        }

        void rowViewItem_Click(object sender, EventArgs e)
        {
            var handler = RosterItemsClick;
            if(handler!=null)
            {
                handler(this, new RosterItemClickEventArgs(Model));
            }
            //   var template = Model.Header[i];
        }

        public event EventHandler<RosterItemClickEventArgs> RosterItemsClick;
    }
}
