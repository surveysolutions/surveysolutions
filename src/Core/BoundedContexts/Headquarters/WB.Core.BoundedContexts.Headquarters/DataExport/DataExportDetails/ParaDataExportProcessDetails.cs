using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails
{
    public class ParaDataExportProcessDetails : AbstractDataExportProcessDetails
    {
        public ParaDataExportProcessDetails(DataExportFormat format)
            : base(format) { }

        public override string NaturalId => $"Paradata${this.Format}";

        public override string Name => "Paradata";
    }
}