using System;
using System.Collections.Generic;

namespace Main.Core.View
{
    public interface ICompositeView
    {
        Guid PublicKey { get; set; }

        string Title { get; set; }
    }
}