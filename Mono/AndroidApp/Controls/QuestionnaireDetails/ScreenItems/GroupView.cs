using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidApp.Core.Model.ViewModel.QuestionnaireDetails;
using AndroidApp.Events;

namespace AndroidApp.Controls.QuestionnaireDetails.ScreenItems
{
    public class GroupView:LinearLayout
    {
        protected QuestionnaireNavigationPanelItem Model { get; private set; }
        protected Button GroupButton { get; private set; }
        protected int? IconId { get; private set; }
        public GroupView(Context context, QuestionnaireNavigationPanelItem model)
            : base(context)
        {
            Model = model;
            Initialize();
        }

        public GroupView(Context context, QuestionnaireNavigationPanelItem model, int iconId)
            : base(context)
        {
            IconId = iconId;
            Model = model;
            Initialize();
        }
        public GroupView(Context context, IAttributeSet attrs, QuestionnaireNavigationPanelItem model)
            : base(context, attrs)
        {
            Model = model;
            Initialize();
        }

        public GroupView(Context context, IAttributeSet attrs, int defStyle, QuestionnaireNavigationPanelItem model)
            : base(context, attrs, defStyle)
        {
            Model = model;
            Initialize();
        }

        protected GroupView(IntPtr javaReference, JniHandleOwnership transfer, QuestionnaireNavigationPanelItem model)
            : base(javaReference, transfer)
        {
            Model = model;
            Initialize();
        }
        protected virtual void Initialize()
        {
            var layoutParams = new TableLayout.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.WrapContent);
            layoutParams.SetMargins(0, 0, 0, 10);
            this.LayoutParameters = layoutParams;
            
            GroupButton=new Button(this.Context);
            GroupButton.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.WrapContent);
            this.AddView(GroupButton);
            if (Model != null)
            {
                GroupButton.Text = Model.Text;
                GroupButton.Enabled = Model.Enabled;
                GroupButton.Click += new EventHandler(GroupButton_Click);
                if (IconId.HasValue)
                {
                    var img = Context.Resources.GetDrawable(IconId.Value);
                    //img.SetBounds(0, 0, 45, 45);
                    GroupButton.SetCompoundDrawablesWithIntrinsicBounds(img, null, img, null);
                }
            }
            else
            {
                this.Visibility = ViewStates.Gone;
            }
            
        }

        void GroupButton_Click(object sender, EventArgs e)
        {
            OnScreenChanged(Model.PublicKey);
        }

        protected void OnScreenChanged(ItemPublicKey publicKey)
        {
            var handler = ScreenChanged;
            if(handler!=null)
                handler(this, new ScreenChangedEventArgs(publicKey));
        }

        public event EventHandler<ScreenChangedEventArgs> ScreenChanged;
    }
}