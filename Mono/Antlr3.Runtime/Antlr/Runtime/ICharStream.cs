namespace Antlr.Runtime
{
    using System;

    public interface ICharStream : IIntStream
    {
        int LT(int i);
        string Substring(int start, int stop);

        int CharPositionInLine { get; set; }

        int Line { get; set; }
    }
}

