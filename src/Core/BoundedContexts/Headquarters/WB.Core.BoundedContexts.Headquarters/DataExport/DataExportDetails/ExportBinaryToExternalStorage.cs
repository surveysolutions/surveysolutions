using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails
{
    public class ExportBinaryToExternalStorage : DataExportProcessDetails
    {
        public override string Name => $"{base.Name} {Enum.GetName(typeof(ExternalStorageType), StorageType)}";

        public ExportBinaryToExternalStorage(DataExportFormat format, QuestionnaireIdentity questionnaire, string questionnaireTitle) 
            : base(format, questionnaire, questionnaireTitle)
        {
        }
    }
}
