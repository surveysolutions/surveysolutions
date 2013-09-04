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
using Ninject;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace CAPI.Android.Controls.QuestionnaireDetails
{
    public class GridContentAdapter : SmartAdapter<RosterTable>
    {
        private readonly Context context;
        private readonly Action<ScreenChangedEventArgs> OnScreenChanged;
        private readonly TextView tvEmptyLabelDescription;
        private readonly int columnCount;
        private readonly IQuestionViewFactory questionViewFactory;
        private readonly Guid QuestionnaireId;
        private readonly ListView listView;
        public GridContentAdapter(QuestionnaireGridViewModel model,int columnCount, Context context,
                                  Action<ScreenChangedEventArgs> onScreenChanged,
                                  TextView tvEmptyLabelDescription, ListView listView)
            : base(CreateItemList(model, columnCount))
        {
            this.context = context;
            this.columnCount = columnCount;
            this.OnScreenChanged = onScreenChanged;
            this.questionViewFactory = new DefaultQuestionViewFactory(CapiApplication.Kernel.Get<IAnswerOnQuestionCommandService>());
            this.tvEmptyLabelDescription = tvEmptyLabelDescription;
            this.QuestionnaireId = model.QuestionnaireId;
            this.listView = listView;
        }

        private static IList<RosterTable> CreateItemList(QuestionnaireGridViewModel model, int columnCount)
        {
            var result = new List<RosterTable>();
            for (int i = 0; i < model.Header.Count; i = i + columnCount)
            {
                result.Add(new RosterTable(model.Header.Skip(i).Take(columnCount).ToList(),model.Rows));
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
                TextView headerItemView = IsHeaderForIndexAvalibleInRosterTable(dataItem, i)
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


        protected void CreateBody(LinearLayout view, RosterTable dataItem)
        {
            foreach (var rosterItem in dataItem.Rows)
            {
                var rowView = new LinearLayout(context);
                rowView.Orientation = Orientation.Horizontal;

                var firstItem = CreateFirstItemAsButton(rosterItem);
                rowView.AddView(firstItem);

                for (int i = 0; i < columnCount; i++)
                {
                    View rosterCell = IsHeaderForIndexAvalibleInRosterTable(dataItem, i)
                                          ? CreateRosterCellView(dataItem.Header[i].PublicKey, rosterItem)
                                          : EmptyRosterItem;

                    AlignTableCell(rosterCell);
                    rowView.AddView(rosterCell);
                }

                rosterItem.PropertyChanged +=
                    (sender, e) =>
                    HideOrShowTableRows(dataItem, sender as QuestionnaireScreenViewModel, e.PropertyName, rowView);

                view.AddView(rowView);

                HideIfRowDisabled(rosterItem.Enabled, rowView);
            }
        }

        private View CreateRosterCellView(Guid headerId, QuestionnairePropagatedScreenViewModel rosterItem)
        {
            View rosterCell;
            QuestionViewModel rowModel =
                rosterItem.Items.FirstOrDefault(q => q.PublicKey.Id == headerId) as QuestionViewModel;
            RosterQuestionView rowViewItem = new RosterQuestionView(context, rowModel);
            rowViewItem.RosterItemsClick += (s, e) => ShowPopupWithQuestion(rosterItem.ScreenName, e.Model);
            rosterCell = rowViewItem;
            return rosterCell;
        }

        private bool IsHeaderForIndexAvalibleInRosterTable(RosterTable dataItem, int i)
        {
            return i < dataItem.Header.Count;
        }

        private TextView EmptyRosterItem
        {
            get { return new TextView(context); }
        }

        private Button CreateFirstItemAsButton(QuestionnairePropagatedScreenViewModel rosterItem)
        {
            Button first = new Button(context);

            first.SetTag(Resource.Id.PrpagationKey, rosterItem.ScreenId.ToString());
            first.Click += first_Click;
            first.Text = rosterItem.ScreenName;

            rosterItem.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == "ScreenName")
                    {
                        first.Text = rosterItem.ScreenName;
                    }
                };

            AlignTableCell(first);
            return first;
        }

        private void HideIfRowDisabled(bool enabled, LinearLayout rowView)
        {
            if (!enabled)
                rowView.Visibility = ViewStates.Gone;
        }

        private void HideOrShowTableRows(RosterTable row, QuestionnaireScreenViewModel item, string propertyName, LinearLayout rosterRecordView)
        {
            if (propertyName != "Enabled")
            {
                return;
            }

            rosterRecordView.Visibility = GetVisibilityFromEnabledStatus(item.Enabled);
            var tableVisible = row.Rows.Any(r => r.Enabled);
            listView.Visibility = tableVisible ? ViewStates.Visible : ViewStates.Invisible;
            tvEmptyLabelDescription.Visibility = GetVisibilityFromEnabledStatus(!tableVisible);
        }

        private static ViewStates GetVisibilityFromEnabledStatus(bool enabled)
        {
            return enabled ? ViewStates.Visible : ViewStates.Gone;
        }

        private void ShowPopupWithQuestion(string popupName, QuestionViewModel question)
        {
            new RosterItemDialog(context, question, popupName, QuestionnaireId,
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
            var publicKey = InterviewItemId.Parse(senderButton.GetTag(Resource.Id.PrpagationKey).ToString());
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