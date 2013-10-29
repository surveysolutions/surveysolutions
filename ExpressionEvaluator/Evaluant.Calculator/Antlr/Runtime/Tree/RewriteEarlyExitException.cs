namespace Antlr.Runtime.Tree
{
    using System;

    [Serializable]
    public class RewriteEarlyExitException : RewriteCardinalityException
    {
        public RewriteEarlyExitException() : base(null)
        {
        }

        public RewriteEarlyExitException(string elementDescription) : base(elementDescription)
        {
        }
    }
}

