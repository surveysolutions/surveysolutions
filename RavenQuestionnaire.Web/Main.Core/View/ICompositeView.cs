using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Main.Core.Documents;

namespace Main.Core.View
{
    public interface ICompositeView
    {
        Guid Id { get; set; }

        string Title { get; set; }
    }
}