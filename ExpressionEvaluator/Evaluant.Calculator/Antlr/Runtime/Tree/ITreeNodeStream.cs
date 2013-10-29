namespace Antlr.Runtime.Tree
{
    using Antlr.Runtime;
    using System;

    public interface ITreeNodeStream : IIntStream
    {
        object Get(int i);
        object LT(int k);
        void ReplaceChildren(object parent, int startChildIndex, int stopChildIndex, object t);
        string ToString(object start, object stop);

        bool HasUniqueNavigationNodes { set; }

        ITokenStream TokenStream { get; }

        ITreeAdaptor TreeAdaptor { get; }

        object TreeSource { get; }
    }
}

