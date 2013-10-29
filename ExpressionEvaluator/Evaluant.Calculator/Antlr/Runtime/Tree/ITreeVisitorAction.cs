namespace Antlr.Runtime.Tree
{
    using System;

    public interface ITreeVisitorAction
    {
        object Post(object t);
        object Pre(object t);
    }
}

