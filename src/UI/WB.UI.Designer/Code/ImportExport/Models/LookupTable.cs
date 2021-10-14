using System;

namespace WB.UI.Designer.Code.ImportExport.Models
{
    public class LookupTable
    {
        public string TableName { get; set; } = String.Empty;
        public string FileName { get; set; } = String.Empty;

        public LookupTable Clone()
        {
            return new LookupTable
            {
                TableName = this.TableName,
                FileName =  this.FileName
            };
        }
    }
}
