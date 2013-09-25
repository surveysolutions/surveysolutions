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
using CAPI.Android.Controls.QuestionnaireDetails.Roster;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails.GridItems;
using CAPI.Android.Events;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace CAPI.Android.Controls.QuestionnaireDetails
{
    public class PartialRosterRowView : LinearLayout
    {
        private readonly QuestionnairePropagatedScreenViewModel model;
        private readonly int columnCount;
        private readonly Guid questionnaireId;
        private readonly Action<ScreenChangedEventArgs> onScreenChanged;
        private readonly HeaderItem[] header;
        private readonly Button tvScreenName;

        public PartialRosterRowView(Context context, QuestionnairePropagatedScreenViewModel model, Guid questionnaireId,
            Action<ScreenChangedEventArgs> onScreenChanged, HeaderItem[] header)
            : base(context)
        {
            this.model = model;
            this.columnCount = columnCount;
            this.questionnaireId = questionnaireId;
            this.onScreenChanged = onScreenChanged;
            this.header = header;

            this.Orientation = Orientation.Horizontal;

            tvScreenName = new Button(this.Context);

            tvScreenName.SetTag(Resource.Id.PrpagationKey, model.ScreenId.ToString());
            tvScreenName.Click += this.tvScreenName_Click;
            tvScreenName.Text = model.ScreenName;

            this.AlignTableCell(tvScreenName);
            this.AddView(tvScreenName);

            for (int i = 0; i < this.header.Length; i++)
            {
                View rosterCell = header[i] != null
                    ? this.CreateRosterCellView(header[i].PublicKey, model)
                    : this.EmptyRosterItem;

                this.AlignTableCell(rosterCell);
                this.AddView(rosterCell);
            }
            model.PropertyChanged += model_PropertyChanged;
            HideIfRowDisabled(model.Enabled);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (model != null)
                {
                    model.PropertyChanged -= this.model_PropertyChanged;
                }
            }
        }

        private void HideIfRowDisabled(bool enabled)
        {
            if (!enabled)
                this.Visibility = ViewStates.Gone;
        }

        void model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (model != sender)
                return;

            if (e.PropertyName == "ScreenName")
            {
                tvScreenName.Text = model.ScreenName;
                return;
            }

            if (e.PropertyName == "Enabled")
            {
                this.Visibility = model.Enabled
                    ? ViewStates.Visible
                    : ViewStates.Gone;

            }
        }

        private View CreateRosterCellView(Guid headerId, QuestionnairePropagatedScreenViewModel rosterItem)
        {
            QuestionViewModel rowModel =
                rosterItem.Items.FirstOrDefault(q => q.PublicKey.Id == headerId) as QuestionViewModel;

            return new RosterQuestionView(Context, rowModel, this.questionnaireId);
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
            get { return new TextView(Context); }
        }

        private void AlignTableCell(View view)
        {
            view.LayoutParameters = new LinearLayout.LayoutParams(0,
                                                                  ViewGroup.LayoutParams.FillParent, 1);
        }
    }
}