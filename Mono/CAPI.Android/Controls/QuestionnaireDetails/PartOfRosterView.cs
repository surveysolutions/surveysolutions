using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CAPI.Android.Controls.QuestionnaireDetails.Roster;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails.GridItems;
using CAPI.Android.Events;
using CAPI.Android.Extensions;
using Cirrious.MvvmCross.Binding.BindingContext;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace CAPI.Android.Controls.QuestionnaireDetails
{
    public class PartOfRosterView : LinearLayout
    {
        private readonly Guid questionnaireId;
        private readonly Action<ScreenChangedEventArgs> onScreenChanged;
        private HeaderItem[] header;

        public PartOfRosterView(Context context, RosterTable model, int columnCount, Guid questionnaireId, Action<ScreenChangedEventArgs> onScreenChanged)
            : base(context)
        {
            this.Orientation = Orientation.Vertical;

            this.questionnaireId = questionnaireId;
            this.onScreenChanged = onScreenChanged;

            BuildHeaderData(model, columnCount);

            CreateHeader(model);
            CreateBody(model);
        }

        private void BuildHeaderData(RosterTable model, int columnCount)
        {
            this.header = new HeaderItem[columnCount];

            for (int i = 0; i < columnCount; i++)
            {
                if (i < model.Header.Count)
                    this.header[i] = model.Header[i];
                else
                    break;
            }
        }

        protected void CreateHeader(RosterTable dataItem)
        {
            var headerView = new LinearLayout(this.Context);
            headerView.Orientation = Orientation.Horizontal;

            headerView.AddView(CreateHeaderViewItem(EmptyRosterItem));

            for (int i = 0; i < header.Length; i++)
            {
                TextView headerItemView = header[i] != null
                    ? CreateHeaderItem(dataItem.Header[i])
                    : EmptyRosterItem;
                headerView.AddView(CreateHeaderViewItem(headerItemView));
            }

            this.AddView(headerView);
        }

        private TextView CreateHeaderItem(HeaderItem headerItem)
        {
            var headerItemView = EmptyRosterItem;
            headerItemView.Text = headerItem.Title;
            if (!string.IsNullOrEmpty(headerItem.Instructions))
            {
                var img = Context.Resources.GetDrawable(global::Android.Resource.Drawable.IcDialogInfo);
                headerItemView.SetCompoundDrawablesWithIntrinsicBounds(null, null, img, null);
                headerItemView.Click += (s, e) =>
                {
                    var instructionsBuilder = new AlertDialog.Builder(Context);
                    instructionsBuilder.SetMessage(headerItem.Instructions);
                    instructionsBuilder.Show();
                };
            }
            headerItemView.SetTag(Resource.Id.ScreenId, headerItem.PublicKey.ToString());
            return headerItemView;
        }

        private TextView EmptyRosterItem
        {
            get { return new TextView(Context); }
        }

        protected void CreateBody(RosterTable dataItem)
        {
            foreach (var rosterItem in dataItem.Rows)
            {
                var rowView = new PartialRosterRowView(this.Context, rosterItem, questionnaireId, onScreenChanged, header);
                this.AddView(rowView);
            }
        }

        protected TextView CreateHeaderViewItem(TextView tv)
        {
            tv.Gravity = GravityFlags.Center;

            tv.SetPadding(10, 10, 10, 10);
            tv.TextSize = 20;
            tv.SetBackgroundResource(Resource.Drawable.grid_headerItem);
            AlignTableCell(tv);
            return tv;
        }

        private void AlignTableCell(View view)
        {
            view.LayoutParameters = new LinearLayout.LayoutParams(0,
                                                                  ViewGroup.LayoutParams.FillParent, 1);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Console.WriteLine("disposing roster PartOfRosterView");
                this.DisposeChildrenAndCleanUp();
            }

            base.Dispose(disposing);
        }
    }
}