namespace Antlr.Runtime
{
    using System;

    [Serializable]
    public class EarlyExitException : RecognitionException
    {
        public int decisionNumber;

        public EarlyExitException()
        {
        }

        public EarlyExitException(int decisionNumber, IIntStream input) : base(input)
        {
            this.decisionNumber = decisionNumber;
        }
    }
}

