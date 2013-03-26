namespace Antlr.Runtime.Tree
{
    using System;
    using System.Collections;

    public interface ITree
    {
        void AddChild(ITree t);
        object DeleteChild(int i);
        ITree DupNode();
        void FreshenParentAndChildIndexes();
        ITree GetAncestor(int ttype);
        IList GetAncestors();
        ITree GetChild(int i);
        bool HasAncestor(int ttype);
        void ReplaceChildren(int startChildIndex, int stopChildIndex, object t);
        void SetChild(int i, ITree t);
        string ToString();
        string ToStringTree();

        int CharPositionInLine { get; }

        int ChildCount { get; }

        int ChildIndex { get; set; }

        bool IsNil { get; }

        int Line { get; }

        ITree Parent { get; set; }

        string Text { get; }

        int TokenStartIndex { get; set; }

        int TokenStopIndex { get; set; }

        int Type { get; }
    }
}

