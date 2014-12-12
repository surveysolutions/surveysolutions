using System;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Shared.Android.Events;

namespace WB.UI.Shared.Android.Controls.ScreenItems
{
    public class GroupView : RelativeLayout  
    {
        protected QuestionnaireNavigationPanelItem Model { get; private set; }
        protected TextView GroupButton { get; private set; }
        protected TextView CounterText { get; private set; }
        protected int? IconId { get; private set; }
        public GroupView(Context context, QuestionnaireNavigationPanelItem model)
            : base(context)
        {
            this.Model = model;
            this.Initialize(context);
        }

        public GroupView(Context context, QuestionnaireNavigationPanelItem model, int iconId)
            : base(context)
        {
            this.IconId = iconId;
            this.Model = model;
            this.Initialize(context);
        }
        public GroupView(Context context, IAttributeSet attrs, QuestionnaireNavigationPanelItem model)
            : base(context, attrs)
        {
            this.Model = model;
            this.Initialize(context);
        }

        public GroupView(Context context, IAttributeSet attrs, int defStyle, QuestionnaireNavigationPanelItem model)
            : base(context, attrs, defStyle)
        {
            this.Model = model;
            this.Initialize(context);
        }
        
        protected void Initialize(Context context)
        {

            LayoutInflater layoutInflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
            
            var view = layoutInflater.Inflate(Resource.Layout.GroupView, null);
            this.GroupButton = view.FindViewById<TextView>(Resource.Id.btGroup);
            this.CounterText = view.FindViewById<TextView>(Resource.Id.tvCounterText);

            this.AddView(view);
            
            if (this.Model != null)
            {
                this.GroupButton.Text = this.Model.Text;
                this.GroupButton.Enabled = this.Model.Enabled;
                this.GroupButton.Click += new EventHandler(this.GroupButton_Click);
                
                if (this.IconId.HasValue)
                {
                    var img = this.Context.Resources.GetDrawable(this.IconId.Value);
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
        
        public void UpdateCounter()
        {
            this.CounterText.Text = string.Format("{0}/{1}", this.Model.Answered, this.Model.Total);
            this.CounterText.SetBackgroundResource(this.Model.Total == this.Model.Answered
                ? Resource.Drawable.donecountershape
                : Resource.Drawable.CounterRoundShape);
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
            if (e.PropertyName != "Answered" && e.PropertyName != "Total") 
                return;
            
            this.UpdateCounter();
            
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