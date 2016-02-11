namespace WB.Core.SharedKernels.SurveySolutions.Documents
{
    public class LookupTable
    {
        public string TableName { get; set; }
        public string FileName { get; set; }

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