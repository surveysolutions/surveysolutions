// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICompositeView.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The CompositeView interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Main.Core.Documents;

namespace Main.Core.View
{
    public interface ICompositeView
    {
        List<ICompositeView> Children { get; set; }

        Guid? Parent { get; set; }
        
        Guid PublicKey { get; set; }

        string Title { get; set; }

        [IgnoreDataMember]
        ICompositeView ParentView { get; }
    }
}