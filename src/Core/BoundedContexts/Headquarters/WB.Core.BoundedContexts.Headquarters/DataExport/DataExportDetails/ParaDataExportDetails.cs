using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails
{
    public class ParaDataExportDetails : AbstractDataExportDetails
    {
        public ParaDataExportDetails(DataExportFormat format)
            : base("Paradata", format) { }
    }
}