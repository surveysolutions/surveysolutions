namespace Antlr.Runtime
{
    using System;

    [Serializable]
    public class FailedPredicateException : RecognitionException
    {
        public string predicateText;
        public string ruleName;

        public FailedPredicateException()
        {
        }

        public FailedPredicateException(IIntStream input, string ruleName, string predicateText) : base(input)
        {
            this.ruleName = ruleName;
            this.predicateText = predicateText;
        }

        public override string ToString()
        {
            return ("FailedPredicateException(" + this.ruleName + ",{" + this.predicateText + "}?)");
        }
    }
}

