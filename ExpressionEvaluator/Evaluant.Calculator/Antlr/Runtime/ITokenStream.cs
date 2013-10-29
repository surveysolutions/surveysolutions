namespace Antlr.Runtime
{
    using System;

    public interface ITokenStream : IIntStream
    {
        IToken Get(int i);
        IToken LT(int k);
        string ToString(IToken start, IToken stop);
        string ToString(int start, int stop);

        ITokenSource TokenSource { get; }
    }
}

