using System.Collections.Generic;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Supervisor
{
    public class SupervisorsView : IListView<SupervisorsItem>
    {
        public int TotalCount { get; set; }

        public IEnumerable<SupervisorsItem> Items { get; set; }
    }
}