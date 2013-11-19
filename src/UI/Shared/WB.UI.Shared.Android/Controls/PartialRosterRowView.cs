using System;
using System.Linq;
using Android.Content;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding.BindingContext;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails.GridItems;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Shared.Android.Controls.Roster;
using WB.UI.Shared.Android.Controls.ScreenItems;
using WB.UI.Shared.Android.Events;
using WB.UI.Shared.Android.Extensions;

namespace WB.UI.Shared.Android.Controls
{
    public class PartialRosterRowView : LinearLayout
    {
        private readonly QuestionnairePropagatedScreenViewModel model;
        private readonly Guid questionnaireId;
        private readonly Action<ScreenChangedEventArgs> onScreenChanged;
        private readonly HeaderItem[] header;
        private readonly Button tvScreenName;
        private readonly IQuestionViewFactory questionViewFactory;


        public PartialRosterRowView(Context context, QuestionnairePropagatedScreenViewModel model, Guid questionnaireId,
            Action<ScreenChangedEventArgs> onScreenChanged, HeaderItem[] header, IQuestionViewFactory questionViewFactory)
            : base(context)
        {
            this.model = model;
            this.questionnaireId = questionnaireId;
            this.onScreenChanged = onScreenChanged;
            this.header = header;
            this.questionViewFactory = questionViewFactory;

            this.Orientation = Orientation.Horizontal;

            this.tvScreenName = new Button(this.Context);

            this.tvScreenName.SetTag(Resource.Id.PrpagationKey, model.ScreenId.ToString());
            this.tvScreenName.Click += this.tvScreenName_Click;
            this.tvScreenName.Text = model.ScreenName;

            this.AlignTableCell(this.tvScreenName);
            this.AddView(this.tvScreenName);

            for (int i = 0; i < this.header.Length; i++)
            {
                View rosterCell = header[i] != null
                    ? this.CreateRosterCellView(header[i].PublicKey, model)
                    : this.EmptyRosterItem;

                this.AlignTableCell(rosterCell);
                this.AddView(rosterCell);
            }
            model.PropertyChanged += this.model_PropertyChanged;
            this.HideIfRowDisabled(model.Enabled);
            this.ChildViewRemoved += this.PartialRosterRowView_ChildViewRemoved;
        }

        void PartialRosterRowView_ChildViewRemoved(object sender, ViewGroup.ChildViewRemovedEventArgs e)
        {
            var boundChild = e.Child as IMvxBindingContextOwner;
            if (boundChild != null)
            {
                Console.WriteLine("clean up binding from roster");
                boundChild.ClearAllBindings();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.model != null)
                {
                    this.model.PropertyChanged -= this.model_PropertyChanged;
                }
                this.DisposeChildrenAndCleanUp();
            }
            base.Dispose(disposing);
        }

        private void HideIfRowDisabled(bool enabled)
        {
            if (!enabled)
                this.Visibility = ViewStates.Gone;
        }

        void model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (this.model != sender)
                return;

            if (e.PropertyName == "ScreenName")
            {
                this.tvScreenName.Text = this.model.ScreenName;
                return;
            }

            if (e.PropertyName == "Enabled")
            {
                this.Visibility = this.model.Enabled
                    ? ViewStates.Visible
                    : ViewStates.Gone;

            }
        }

        private View CreateRosterCellView(Guid headerId, QuestionnairePropagatedScreenViewModel rosterItem)
        {
            QuestionViewModel rowModel =
                rosterItem.Items.FirstOrDefault(q => q.PublicKey.Id == headerId) as QuestionViewModel;

            return new RosterQuestionView(this.Context, rowModel, this.questionnaireId, this.questionViewFactory);
        }

        private void tvScreenName_Click(object sender, EventArgs e)
        {
            var senderButton = sender as Button;
            if (senderButton == null)
                return;
            var publicKey = InterviewItemId.Parse(senderButton.GetTag(Resource.Id.PrpagationKey).ToString());
            this.onScreenChanged(new ScreenChangedEventArgs(publicKey));
        }

        private TextView EmptyRosterItem
        {
            get { return new TextView(this.Context); }
        }

        private void AlignTableCell(View view)
        {
            view.LayoutParameters = new LinearLayout.LayoutParams(0,
                                                                  ViewGroup.LayoutParams.FillParent, 1);
        }
    }
}