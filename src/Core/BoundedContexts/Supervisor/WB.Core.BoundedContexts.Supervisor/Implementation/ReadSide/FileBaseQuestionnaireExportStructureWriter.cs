using System;
using System.Collections.Generic;
using System.IO;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Utils.Serialization;

namespace WB.Core.BoundedContexts.Supervisor.Implementation.ReadSide
{
    internal class FileBaseQuestionnaireExportStructureWriter : IReadSideRepositoryWriter<QuestionnaireExportStructure>, IReadSideRepositoryCleaner
    {
        private readonly IDataExportService dataExportService;
        private const string FolderName = "QuestionnaireExportStructureStorage";
        private readonly string path;
        private readonly IJsonUtils jsonSerializer;

        public FileBaseQuestionnaireExportStructureWriter(IDataExportService dataExportService, IJsonUtils jsonSerializer, IReadSideRepositoryCleanerRegistry cleanerRegistry, string folderPath)
        {
            cleanerRegistry.Register(this);
            this.dataExportService = dataExportService;
            this.jsonSerializer = jsonSerializer;
            this.path = Path.Combine(folderPath, FolderName);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public QuestionnaireExportStructure GetById(string id)
        {
            var questionnaireFileName = GetQuestionnaireFileName(id);
            if (!File.Exists(questionnaireFileName))
                return null;
            return this.jsonSerializer.Deserrialize<QuestionnaireExportStructure>(File.ReadAllText(questionnaireFileName));
        }

        public void Remove(string id)
        {
            dataExportService.DeleteExportedData(ToGuestionnaireId(id), 1);
            File.Delete(GetQuestionnaireFileName(id));
        }

        public void Clear()
        {
            Array.ForEach(Directory.GetFiles(this.path), File.Delete);
        }

        public void Store(QuestionnaireExportStructure view, string id)
        {
            this.dataExportService.CreateExportedDataStructureByTemplate(view);
            File.WriteAllText(this.GetQuestionnaireFileName(id), this.jsonSerializer.GetItemAsContent(view));
        }

        private static Guid ToGuestionnaireId(string stringId)
        {
            return Guid.Parse(stringId);
        }

        private string GetQuestionnaireFileName(string id)
        {
            return Path.Combine(path, string.Format("{0}.txt", id));
        }
    }
}
