
using System;

namespace WB.Core.BoundedContexts.Designer.ImportExport.Models.Question
{
    public class DateTimeQuestion : AbstractQuestion
    {
        public DateTime? DefaultDate { get; set; }

        public bool IsTimestamp { get; set; }
   }
}
