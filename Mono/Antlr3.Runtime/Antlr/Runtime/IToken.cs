namespace Antlr.Runtime
{
    using System;

    public interface IToken
    {
        int Channel { get; set; }

        int CharPositionInLine { get; set; }

        int Line { get; set; }

        string Text { get; set; }

        int TokenIndex { get; set; }

        int Type { get; set; }
    }
}

