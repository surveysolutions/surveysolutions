using WB.Core.Infrastructure.ReadSide;

namespace WB.Core.Infrastructure.FunctionalDenormalization.Implementation.ReadSide
{
    public class ViewWithSequence<T> : IView
    {
        public ViewWithSequence(T document, long sequence)
        {
            this.Document = document;
            this.Sequence = sequence;
        }

        public T Document { get; private set; }
        public long Sequence { get; set; }
    }
}