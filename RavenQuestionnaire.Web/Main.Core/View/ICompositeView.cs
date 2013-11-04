using System;
using System.Collections.Generic;

namespace Main.Core.View
{
    public interface ICompositeView
    {
        Guid Id { get; set; }

        string Title { get; set; }
    }
}