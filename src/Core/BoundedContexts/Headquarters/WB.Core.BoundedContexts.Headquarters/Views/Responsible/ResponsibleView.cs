using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Views.Responsible
{
    public class ResponsibleView
    {
        public IEnumerable<ResponsiblesViewItem> Users { get; set; }
        public int TotalCountByQuery { get; set; }
    }
}