namespace Antlr.Runtime
{
    using System;

    [Serializable]
    public class MismatchedTokenException : RecognitionException
    {
        private int expecting;

        public MismatchedTokenException()
        {
        }

        public MismatchedTokenException(int expecting, IIntStream input) : base(input)
        {
            this.expecting = expecting;
        }

        public override string ToString()
        {
            return string.Concat(new object[] { "MismatchedTokenException(", this.UnexpectedType, "!=", this.expecting, ")" });
        }

        public int Expecting
        {
            get
            {
                return this.expecting;
            }
            set
            {
                this.expecting = value;
            }
        }
    }
}

