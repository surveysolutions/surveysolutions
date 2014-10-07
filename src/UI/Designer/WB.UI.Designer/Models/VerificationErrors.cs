namespace WB.UI.Designer.Models
{
    public class VerificationErrors
    {
        public VerificationErrors()
        {
            this.Errors = new VerificationError[0];
        }

        public VerificationError[] Errors { get; set; }
        public int ErrorsCount { get; set; }
    }
}