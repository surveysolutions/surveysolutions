namespace Antlr.Runtime
{
    using System;

    public interface ITokenSource
    {
        IToken NextToken();

        string SourceName { get; }
    }
}

