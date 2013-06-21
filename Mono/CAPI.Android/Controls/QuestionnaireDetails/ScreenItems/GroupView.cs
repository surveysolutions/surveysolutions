using System;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using CAPI.Android.Events;

namespace CAPI.Android.Controls.QuestionnaireDetails.ScreenItems
{
    public class GroupView : RelativeLayout 
    {
        protected QuestionnaireNavigationPanelItem Model { get; private set; }
        protected Button GroupButton { get; private set; }
        protected TextView CounterText { get; private set; }
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
            var layoutParams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.WrapContent);
            layoutParams.SetMargins(0, 0, 0, 10);
            this.LayoutParameters = layoutParams;
            
            AddButton();

            AddCounterText();

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
                else
                {
                    Model.PropertyChanged += Model_PropertyChanged;
                }
                UpdateCounter();
            }
            else
            {
                this.Visibility = ViewStates.Gone;
            }

          
        }

        private void AddCounterText()
        {
            CounterText = new TextView(this.Context);
            var counterParameters = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            
            counterParameters.AddRule(LayoutRules.AlignParentRight);

            CounterText.LayoutParameters = counterParameters;

            CounterText.SetPadding(3, 3, 3, 3);
            CounterText.SetTextColor(Color.Black);
            CounterText.SetBackgroundResource(Resource.Drawable.CounterRoundShape);
            this.AddView(CounterText);
        }

        private void AddButton()
        {
            GroupButton = new Button(this.Context);
            var buttonParameters = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                                   ViewGroup.LayoutParams.WrapContent);
            GroupButton.LayoutParameters = buttonParameters;
            this.AddView(GroupButton);
        }


        public void UpdateCounter()
        {
            CounterText.Text = string.Format("{0}/{1}", Model.Answered, Model.Total);
            if (Model.Total == Model.Answered)
                CounterText.SetBackgroundResource(Resource.Drawable.donecountershape);
            else
                CounterText.SetBackgroundResource(Resource.Drawable.CounterRoundShape);
        }
        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();
            if (Model != null)
                GroupButton.Enabled = Model.Enabled;
        }
        void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Enabled")
            {
                GroupButton.Enabled = Model.Enabled;
                CounterText.Visibility = Model.Enabled ? ViewStates.Visible : ViewStates.Gone;
                return;
            }
            if (e.PropertyName == "Answered" || e.PropertyName == "Total")
            {
                UpdateCounter();
                return;
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