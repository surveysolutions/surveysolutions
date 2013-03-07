namespace Antlr.Runtime.Tree
{
    using System;

    [Serializable]
    public class RewriteEmptyStreamException : RewriteCardinalityException
    {
        public RewriteEmptyStreamException(string elementDescription) : base(elementDescription)
        {
        }
    }
}

