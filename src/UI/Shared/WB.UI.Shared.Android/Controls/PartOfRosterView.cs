using System;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails.GridItems;
using WB.UI.Shared.Android.Controls.ScreenItems;
using WB.UI.Shared.Android.Events;
using WB.UI.Shared.Android.Extensions;

namespace WB.UI.Shared.Android.Controls
{
    public class PartOfRosterView : LinearLayout
    {
        private readonly Guid questionnaireId;
        private readonly Action<ScreenChangedEventArgs> onScreenChanged;
        private readonly IQuestionViewFactory questionViewFactory;
        private HeaderItem[] header;

        public PartOfRosterView(Context context, RosterTable model, int columnCount, Guid questionnaireId, Action<ScreenChangedEventArgs> onScreenChanged, IQuestionViewFactory questionViewFactory)
            : base(context)
        {
            this.Orientation = Orientation.Vertical;

            this.questionnaireId = questionnaireId;
            this.onScreenChanged = onScreenChanged;
            this.questionViewFactory = questionViewFactory;

            this.BuildHeaderData(model, columnCount);

            this.CreateHeader(model);
            this.CreateBody(model);
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

            headerView.AddView(this.CreateHeaderViewItem(this.EmptyRosterItem));

            for (int i = 0; i < this.header.Length; i++)
            {
                TextView headerItemView = this.header[i] != null
                    ? this.CreateHeaderItem(dataItem.Header[i])
                    : this.EmptyRosterItem;
                headerView.AddView(this.CreateHeaderViewItem(headerItemView));
            }

            this.AddView(headerView);
        }

        private TextView CreateHeaderItem(HeaderItem headerItem)
        {
            var headerItemView = this.EmptyRosterItem;
            headerItemView.Text = headerItem.Title;
            if (!string.IsNullOrEmpty(headerItem.Instructions))
            {
                var img = this.Context.Resources.GetDrawable(global::Android.Resource.Drawable.IcDialogInfo);
                headerItemView.SetCompoundDrawablesWithIntrinsicBounds(null, null, img, null);
                headerItemView.Click += (s, e) =>
                {
                    var instructionsBuilder = new AlertDialog.Builder(this.Context);
                    instructionsBuilder.SetMessage(headerItem.Instructions);
                    instructionsBuilder.Show();
                };
            }
            headerItemView.SetTag(Resource.Id.ScreenId, headerItem.PublicKey.ToString());
            return headerItemView;
        }

        private TextView EmptyRosterItem
        {
            get { return new TextView(this.Context); }
        }

        protected void CreateBody(RosterTable dataItem)
        {
            foreach (var rosterItem in dataItem.Rows)
            {
                var rowView = new PartialRosterRowView(this.Context, rosterItem, this.questionnaireId, this.onScreenChanged, this.header, this.questionViewFactory);
                this.AddView(rowView);
            }
        }

        protected TextView CreateHeaderViewItem(TextView tv)
        {
            tv.Gravity = GravityFlags.Center;

            tv.SetPadding(10, 10, 10, 10);
            tv.TextSize = 20;
            tv.SetBackgroundResource(Resource.Drawable.grid_headerItem);
            this.AlignTableCell(tv);
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