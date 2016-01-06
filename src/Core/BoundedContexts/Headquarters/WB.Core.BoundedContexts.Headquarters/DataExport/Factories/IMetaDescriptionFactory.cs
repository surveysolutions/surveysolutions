using System.Collections.Generic;
using System.Globalization;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views.Labels;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Factories
{
    internal interface IMetaDescriptionFactory
    {
        IMetadataWriter CreateMetaDescription();
    }
}
