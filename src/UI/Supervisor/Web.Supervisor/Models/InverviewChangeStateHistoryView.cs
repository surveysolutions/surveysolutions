using System.Collections.Generic;

namespace Web.Supervisor.Models
{
    public class InverviewChangeStateHistoryView
    {
        public IEnumerable<HistoryItemView> HistoryItems { get; set; }
    }
}