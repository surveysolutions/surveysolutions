using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Views;
using Android.Widget;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.UI.Capi.Shared.Controls;
using WB.UI.Capi.Shared.Controls.ScreenItems;
using WB.UI.Capi.Shared.Events;

namespace WB.UI.Capi.Shared.Adapters
{
    public class GridContentAdapter : SmartAdapter<RosterTable>
    {
        private readonly Context context;
        private readonly Action<ScreenChangedEventArgs> onScreenChanged;

        private readonly IQuestionViewFactory questionViewFactory;
        private readonly int columnCount;
       
        private readonly Guid questionnaireId;

        public GridContentAdapter(QuestionnaireGridViewModel model,int columnCount, Context context,
                                  Action<ScreenChangedEventArgs> onScreenChanged, IQuestionViewFactory questionViewFactory)
            : base(CreateItemList(model, columnCount))
        {
            this.context = context;
            this.columnCount = columnCount;
            this.onScreenChanged = onScreenChanged;
            this.questionViewFactory = questionViewFactory;
            this.questionnaireId = model.QuestionnaireId;
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
            var view = new PartOfRosterView(this.context, dataItem, this.columnCount, this.questionnaireId, this.onScreenChanged,
                questionViewFactory);
            
            var llTableParentLayoutParameters = new ListView.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                             ViewGroup.LayoutParams.WrapContent);
            view.LayoutParameters = llTableParentLayoutParameters;
            
            return view;
        }
    }
}