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
using AndroidApp.ViewModel.QuestionnaireDetails;

namespace AndroidApp.Controls.QuestionnaireDetails.ScreenItems
{
    public class GroupView:LinearLayout
    {
        protected GroupViewModel Model { get; private set; }
        protected Button GroupButton { get; private set; }

        public GroupView(Context context, GroupViewModel model)
            : base(context)
        {
            Model = model;
            Initialize();
        }

        public GroupView(Context context, IAttributeSet attrs, GroupViewModel model)
            : base(context, attrs)
        {
            Model = model;
            Initialize();
        }

        public GroupView(Context context, IAttributeSet attrs, int defStyle, GroupViewModel model)
            : base(context, attrs, defStyle)
        {
            Model = model;
            Initialize();
        }

        protected GroupView(IntPtr javaReference, JniHandleOwnership transfer, GroupViewModel model)
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
            GroupButton.Text = Model.Text;
            GroupButton.Enabled = Model.Enabled;
            GroupButton.Click += new EventHandler(GroupButton_Click);
            this.AddView(GroupButton);
        }

        void GroupButton_Click(object sender, EventArgs e)
        {
            OnScreenChanged(Model.PublicKey);
        }

        protected void OnScreenChanged(Guid publicKey)
        {
            var handler = ScreenChanged;
            if(handler!=null)
                handler(this, new ScreenChangedEventArgs(publicKey));
        }

        public event EventHandler<ScreenChangedEventArgs> ScreenChanged;
    }
}