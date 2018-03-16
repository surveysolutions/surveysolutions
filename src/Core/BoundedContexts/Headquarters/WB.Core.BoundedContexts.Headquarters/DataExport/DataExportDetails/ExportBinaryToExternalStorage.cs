using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails
{
    public class ExportBinaryToExternalStorage : DataExportProcessDetails
    {
        public override string Name => $"{base.Name} {Enum.GetName(typeof(ExternalStorageType), this.StorageType)}";

        public ExternalStorageType StorageType { get; set; }
        public string AccessToken { get; set; }
        public ExportBinaryToExternalStorage(DataExportFormat format, QuestionnaireIdentity questionnaire, string questionnaireTitle) : base(format, questionnaire, questionnaireTitle)
        {
        }
    }
}
