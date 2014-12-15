namespace WB.Core.SharedKernel.Structures.Synchronization.Designer
{
    public class QuestionnaireVersion
    {
        public int Major { get; set; }
        public int Minor { get; set; }
        public int Patch { get; set; }

        public override string ToString()
        {
            return string.Format("{0}.{1}.{2}", this.Major, this.Minor, this.Patch);
        }
    }
}
