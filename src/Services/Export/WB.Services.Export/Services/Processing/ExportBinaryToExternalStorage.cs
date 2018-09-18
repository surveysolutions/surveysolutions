using System;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services.Processing.Good;

namespace WB.Services.Export.Services.Processing
{
    public class ExportBinaryToExternalStorage : DataExportProcessDetails
    {
        public override string Name => $"{base.Name} {Enum.GetName(typeof(ExternalStorageType), this.StorageType)}";

        public ExternalStorageType StorageType { get; set; }
        public string AccessToken { get; set; }
        public ExportBinaryToExternalStorage(DataExportFormat format, QuestionnaireId questionnaire, string questionnaireTitle) 
            : base(format, questionnaire, questionnaireTitle)
        {
        }
    }
}
