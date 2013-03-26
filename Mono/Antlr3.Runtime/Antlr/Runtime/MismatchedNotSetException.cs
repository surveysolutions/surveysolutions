namespace Antlr.Runtime
{
    using System;

    [Serializable]
    public class MismatchedNotSetException : MismatchedSetException
    {
        public MismatchedNotSetException()
        {
        }

        public MismatchedNotSetException(BitSet expecting, IIntStream input) : base(expecting, input)
        {
        }

        public override string ToString()
        {
            return string.Concat(new object[] { "MismatchedNotSetException(", this.UnexpectedType, "!=", base.expecting, ")" });
        }
    }
}

