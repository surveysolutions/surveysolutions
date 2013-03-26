namespace Antlr.Runtime
{
    using System;

    [Serializable]
    public class MismatchedSetException : RecognitionException
    {
        public BitSet expecting;

        public MismatchedSetException()
        {
        }

        public MismatchedSetException(BitSet expecting, IIntStream input) : base(input)
        {
            this.expecting = expecting;
        }

        public override string ToString()
        {
            return string.Concat(new object[] { "MismatchedSetException(", this.UnexpectedType, "!=", this.expecting, ")" });
        }
    }
}

