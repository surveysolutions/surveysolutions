using System;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Capi.Shared.Events;

namespace WB.UI.Capi.Shared.Controls.ScreenItems
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
            this.Model = model;
            this.Initialize();
        }

        public GroupView(Context context, QuestionnaireNavigationPanelItem model, int iconId)
            : base(context)
        {
            this.IconId = iconId;
            this.Model = model;
            this.Initialize();
        }
        public GroupView(Context context, IAttributeSet attrs, QuestionnaireNavigationPanelItem model)
            : base(context, attrs)
        {
            this.Model = model;
            this.Initialize();
        }

        public GroupView(Context context, IAttributeSet attrs, int defStyle, QuestionnaireNavigationPanelItem model)
            : base(context, attrs, defStyle)
        {
            this.Model = model;
            this.Initialize();
        }

        protected GroupView(IntPtr javaReference, JniHandleOwnership transfer, QuestionnaireNavigationPanelItem model)
            : base(javaReference, transfer)
        {
            this.Model = model;
            this.Initialize();
        }
        protected virtual void Initialize()
        {
            this.AddButton();

            this.AddCounterText();

            if (this.Model != null)
            {
                this.GroupButton.Text = this.Model.Text;
                this.GroupButton.Enabled = this.Model.Enabled;
                this.GroupButton.Click += new EventHandler(this.GroupButton_Click);
                
                if (this.IconId.HasValue)
                {
                    var img = this.Context.Resources.GetDrawable(this.IconId.Value);
                    //img.SetBounds(0, 0, 45, 45);
                    this.GroupButton.SetCompoundDrawablesWithIntrinsicBounds(img, null, img, null);
                }
                else
                {
                    this.Model.PropertyChanged += this.Model_PropertyChanged;
                }
                this.UpdateCounter();
            }
            else
            {
                this.Visibility = ViewStates.Gone;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.Model != null)
                {
                    this.Model.PropertyChanged -= this.Model_PropertyChanged;
                }
            }

            base.Dispose(disposing);
        }

        private void AddCounterText()
        {
            this.CounterText = new TextView(this.Context);
            var counterParameters = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            
            counterParameters.AddRule(LayoutRules.AlignParentRight);

            this.CounterText.LayoutParameters = counterParameters;

            this.CounterText.SetPadding(3, 3, 3, 3);
            this.CounterText.SetTextColor(Color.Black);
            this.CounterText.SetBackgroundResource(Resource.Drawable.CounterRoundShape);
            this.AddView(this.CounterText);
        }

        private void AddButton()
        {
            this.GroupButton = new Button(this.Context);
            var buttonParameters = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                                   ViewGroup.LayoutParams.WrapContent);
            this.GroupButton.LayoutParameters = buttonParameters;
            this.AddView(this.GroupButton);
        }


        public void UpdateCounter()
        {
            this.CounterText.Text = string.Format("{0}/{1}", this.Model.Answered, this.Model.Total);
            if (this.Model.Total == this.Model.Answered)
                this.CounterText.SetBackgroundResource(Resource.Drawable.donecountershape);
            else
                this.CounterText.SetBackgroundResource(Resource.Drawable.CounterRoundShape);
        }
        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();
            if (this.Model != null)
                this.GroupButton.Enabled = this.Model.Enabled;
        }
        void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Enabled")
            {
                this.GroupButton.Enabled = this.Model.Enabled;
                this.CounterText.Visibility = this.Model.Enabled ? ViewStates.Visible : ViewStates.Gone;
                return;
            }
            if (e.PropertyName == "Answered" || e.PropertyName == "Total")
            {
                this.UpdateCounter();
                return;
            }
        }

        void GroupButton_Click(object sender, EventArgs e)
        {
            this.OnScreenChanged(this.Model.PublicKey);
        }

        protected void OnScreenChanged(InterviewItemId publicKey)
        {
            var handler = this.ScreenChanged;
            if(handler!=null)
                handler(this, new ScreenChangedEventArgs(publicKey));
        }

        public event EventHandler<ScreenChangedEventArgs> ScreenChanged;
    }
}