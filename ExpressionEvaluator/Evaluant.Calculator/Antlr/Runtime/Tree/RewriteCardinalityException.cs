namespace Antlr.Runtime.Tree
{
    using System;

    [Serializable]
    public class RewriteCardinalityException : Exception
    {
        public string elementDescription;

        public RewriteCardinalityException(string elementDescription)
        {
            this.elementDescription = elementDescription;
        }

        public override string Message
        {
            get
            {
                if (this.elementDescription != null)
                {
                    return this.elementDescription;
                }
                return null;
            }
        }
    }
}

