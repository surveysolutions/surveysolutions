namespace Antlr.Runtime.Tree
{
    using Antlr.Runtime;
    using System;

    public interface ITreeAdaptor
    {
        void AddChild(object t, object child);
        object BecomeRoot(IToken newRoot, object oldRoot);
        object BecomeRoot(object newRoot, object oldRoot);
        object Create(IToken payload);
        object Create(int tokenType, IToken fromToken);
        object Create(int tokenType, string text);
        object Create(int tokenType, IToken fromToken, string text);
        object DeleteChild(object t, int i);
        object DupNode(object treeNode);
        object DupTree(object tree);
        object ErrorNode(ITokenStream input, IToken start, IToken stop, RecognitionException e);
        object GetChild(object t, int i);
        int GetChildCount(object t);
        int GetChildIndex(object t);
        object GetNilNode();
        string GetNodeText(object t);
        int GetNodeType(object t);
        object GetParent(object t);
        IToken GetToken(object treeNode);
        int GetTokenStartIndex(object t);
        int GetTokenStopIndex(object t);
        int GetUniqueID(object node);
        bool IsNil(object tree);
        void ReplaceChildren(object parent, int startChildIndex, int stopChildIndex, object t);
        object RulePostProcessing(object root);
        void SetChild(object t, int i, object child);
        void SetChildIndex(object t, int index);
        void SetNodeText(object t, string text);
        void SetNodeType(object t, int type);
        void SetParent(object t, object parent);
        void SetTokenBoundaries(object t, IToken startToken, IToken stopToken);
    }
}

