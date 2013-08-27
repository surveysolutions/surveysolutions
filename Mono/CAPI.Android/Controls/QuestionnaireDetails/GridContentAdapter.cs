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
using CAPI.Android.Controls.QuestionnaireDetails.ScreenItems;
using CAPI.Android.Core;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails.GridItems;
using CAPI.Android.Events;

namespace CAPI.Android.Controls.QuestionnaireDetails
{
    public class GridContentAdapter : SmartAdapter<RosterTable>
    {
        private readonly Context context;
        private readonly Action<ScreenChangedEventArgs> OnScreenChanged;
        private readonly TextView tvEmptyLabelDescription;
        private readonly int columnCount;
        private readonly IQuestionViewFactory questionViewFactory;
        public GridContentAdapter(QuestionnaireGridViewModel model,int columnCount, Context context,
                                  Action<ScreenChangedEventArgs> onScreenChanged,
                                  TextView tvEmptyLabelDescription)
            : base(CreateItemList(model, columnCount))
        {
            this.context = context;
            this.columnCount = columnCount;
            this.OnScreenChanged = onScreenChanged;
            this.questionViewFactory = new DefaultQuestionViewFactory();
            this.tvEmptyLabelDescription = tvEmptyLabelDescription;
        }

        private static IList<RosterTable> CreateItemList(QuestionnaireGridViewModel model, int columnCount)
        {
            var result = new List<RosterTable>();
            for (int i = 0; i < model.Header.Count; i = i + columnCount)
            {
                result.Add(new RosterTable(model.QuestionnaireId,model.Header.Skip(i).Take(columnCount).ToList(),model.Rows));
            }
            return result;
        }

        protected override View BuildViewItem(RosterTable dataItem, int position)
        {
            var view = new LinearLayout(context);
            view.Orientation = Orientation.Vertical;
            
            var llTableParentLayoutParameters = new ListView.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                             ViewGroup.LayoutParams.WrapContent);
            view.LayoutParameters = llTableParentLayoutParameters;

            CreateHeader(view, this[position]);
            CreateBody(view, this[position]);
            
            return view;
        }

        protected void CreateHeader(LinearLayout view, RosterTable dataItem)
        {
            var headerView = new LinearLayout(context);
            headerView.Orientation = Orientation.Horizontal;

            headerView.AddView(CreateHeaderViewItem(EmptyRosterItem));

            for (int i = 0; i < columnCount; i++)
            {
                TextView headerItemView = dataItem.Header.Count > i
                                              ? CreateHeaderItem(dataItem.Header[i])
                                              : EmptyRosterItem;
                headerView.AddView(CreateHeaderViewItem(headerItemView));
            }
            view.AddView(headerView);
        }

        private TextView CreateHeaderItem(HeaderItem headerItem)
        {
            var headerItemView = EmptyRosterItem;
            headerItemView.Text = headerItem.Title;
            if (!string.IsNullOrEmpty(headerItem.Instructions))
            {
                var img = context.Resources.GetDrawable(global::Android.Resource.Drawable.IcDialogInfo);
                headerItemView.SetCompoundDrawablesWithIntrinsicBounds(null, null, img, null);
                headerItemView.Click += (s, e) =>
                    {
                        var instructionsBuilder = new AlertDialog.Builder(context);
                        instructionsBuilder.SetMessage(headerItem.Instructions);
                        instructionsBuilder.Show();
                    };
            }
            headerItemView.SetTag(Resource.Id.ScreenId, headerItem.PublicKey.ToString());
            return headerItemView;
        }

        private TextView EmptyRosterItem
        {
            get { return new TextView(context); }
        }

        protected void CreateBody(LinearLayout tl, RosterTable row)
        {
            foreach (var rosterItem in row.Rows)
            {

                var th = new LinearLayout(context);
                th.Orientation=Orientation.Horizontal;
                if (!rosterItem.Enabled)
                    th.Visibility = ViewStates.Gone;

                Button first = new Button(context);
                first.SetTag(Resource.Id.PrpagationKey, rosterItem.ScreenId.ToString());
                first.Click += first_Click;
                first.Text = rosterItem.ScreenName;
                rosterItem.PropertyChanged += (sender, e) => HideOrShowTableRows(row, sender as QuestionnaireScreenViewModel, e, first, tl, th);

                AlignTableCell(first);
                th.AddView(first);

                for (int i = 0; i < row.Header.Count; i++)
                {
                    View rosterCell;
                    if (i < rosterItem.Items.Count())
                    {
                        QuestionViewModel rowModel = rosterItem.Items.FirstOrDefault(q=>q.PublicKey.PublicKey==row.Header[i].PublicKey) as QuestionViewModel;
                        RosterQuestionView rowViewItem = new RosterQuestionView(context, rowModel);
                        rowViewItem.RosterItemsClick += (s, e) => ShowPopupWithQuestion(row, e.Model);
                        rosterCell = rowViewItem;
                    }
                    else
                    {
                        rosterCell = new TextView(context);
                    }

                    AlignTableCell(rosterCell);
                    th.AddView(rosterCell);
                }


                tl.AddView(th);
            }
        }

        private void HideOrShowTableRows(RosterTable row, QuestionnaireScreenViewModel item, PropertyChangedEventArgs e, Button first, LinearLayout view, LinearLayout record)
        {
            if (e.PropertyName == "Enabled")
            {
                var visibility = item.Enabled ? ViewStates.Visible : ViewStates.Gone;
                record.Visibility = visibility;
                var tableVisible = row.Rows.Any(r => r.Enabled);
                view.Visibility = tableVisible ? ViewStates.Visible : ViewStates.Gone;
                tvEmptyLabelDescription.Visibility = !tableVisible ? ViewStates.Visible : ViewStates.Gone;
                return;
            }
            if (e.PropertyName == "ScreenName")
            {
                first.Text = item.ScreenName;
            }
        }

        private void ShowPopupWithQuestion(RosterTable row, QuestionViewModel question)
        {
            var group =
                row.Rows.FirstOrDefault(
                    r => r.ScreenId.PropagationKey == question.PublicKey.PropagationKey);
            if (@group == null)
                return;
            new RosterItemDialog(context, question, @group.ScreenName, row.QuestionnaireId,
                                 questionViewFactory);
        }

        private void AlignTableCell(View view)
        {
            view.LayoutParameters = new LinearLayout.LayoutParams(0,
                                                                  ViewGroup.LayoutParams.FillParent, 1);
        }

        private void first_Click(object sender, EventArgs e)
        {
            var senderButton = sender as Button;
            if (senderButton == null)
                return;
            var publicKey = ItemPublicKey.Parse(senderButton.GetTag(Resource.Id.PrpagationKey).ToString());
            OnScreenChanged(new ScreenChangedEventArgs(publicKey));
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
    }
}