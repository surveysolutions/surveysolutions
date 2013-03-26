namespace Antlr.Runtime.Tree
{
    using System;

    public sealed class Tree
    {
        public static readonly ITree INVALID_NODE = new CommonTree(Token.INVALID_TOKEN);
    }
}

