namespace WB.Core.SharedKernels.DataCollection.Portable
{
    public class Section
    {
        public Section(bool isDisabled)
        {
            __isDisabled = isDisabled;
        }

        private readonly bool __isDisabled;

        public bool IsDisabled() => this.__isDisabled;
    }
}
