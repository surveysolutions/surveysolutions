namespace Antlr.Runtime
{
    using System;

    public class UnwantedTokenException : MismatchedTokenException
    {
        public UnwantedTokenException()
        {
        }

        public UnwantedTokenException(int expecting, IIntStream input) : base(expecting, input)
        {
        }

        public override string ToString()
        {
            string str = ", expected " + base.Expecting;
            if (base.Expecting == 0)
            {
                str = "";
            }
            if (base.token == null)
            {
                return ("UnwantedTokenException(found=" + str + ")");
            }
            return ("UnwantedTokenException(found=" + base.token.Text + str + ")");
        }

        public IToken UnexpectedToken
        {
            get
            {
                return base.token;
            }
        }
    }
}

