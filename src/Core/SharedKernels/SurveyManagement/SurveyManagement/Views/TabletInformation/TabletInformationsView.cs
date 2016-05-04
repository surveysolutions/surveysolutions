using System.Collections.Generic;

namespace WB.Core.SharedKernels.SurveyManagement.Views.TabletInformation
{
    public class TabletInformationsView : IListView<TabletInformationView>
    {
        public int TotalCount { get; set; }

        public IEnumerable<TabletInformationView> Items { get; set; }
    }
}