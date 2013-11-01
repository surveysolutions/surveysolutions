namespace Antlr.Runtime
{
    using Antlr.Runtime.Tree;
    using System;

    [Serializable]
    public class MismatchedTreeNodeException : RecognitionException
    {
        public int expecting;

        public MismatchedTreeNodeException()
        {
        }

        public MismatchedTreeNodeException(int expecting, ITreeNodeStream input) : base(input)
        {
            this.expecting = expecting;
        }

        public override string ToString()
        {
            return string.Concat(new object[] { "MismatchedTreeNodeException(", this.UnexpectedType, "!=", this.expecting, ")" });
        }
    }
}

