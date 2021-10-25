
using System;

namespace WB.UI.Designer.Code.ImportExport.Models.Question
{
    public class DateTimeQuestion : AbstractQuestion
    {
        public DateTime? DefaultDate { get; set; }

        public bool IsTimestamp { get; set; }
   }
}
