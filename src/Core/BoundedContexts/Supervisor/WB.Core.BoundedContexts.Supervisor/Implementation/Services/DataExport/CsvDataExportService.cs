using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Main.Core.Export;
using Main.Core.View;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.Implementation.Services.DataExport
{
    internal class CsvDataExportService : IDataExportService
    {
        private const string CSVFORMAT = "csv";
        private const string TABFORMAT = "tab";
        private const string CSVFILEEXTENSION = "." + CSVFORMAT;

        private readonly IEnvironmentSupplier<InterviewDataExportView> supplier;
        private readonly IExportProvider<InterviewDataExportView> provider;
        private readonly IViewFactory<InterviewDataExportInputModel, InterviewDataExportView> interviewDataExportViewFactory;
        private readonly IVersionedReadSideRepositoryReader<QuestionnairePropagationStructure> questionnaireLevelStorage;

        public CsvDataExportService(
            IEnvironmentSupplier<InterviewDataExportView> supplier, 
            IExportProvider<InterviewDataExportView> provider, 
            IViewFactory<InterviewDataExportInputModel, InterviewDataExportView> interviewDataExportViewFactory, 
            IVersionedReadSideRepositoryReader<QuestionnairePropagationStructure> questionnaireLevelStorage)
        {
            this.supplier = supplier;
            this.provider = provider;
            this.interviewDataExportViewFactory = interviewDataExportViewFactory;
            this.questionnaireLevelStorage = questionnaireLevelStorage;
        }

        public IDictionary<string, byte[]> ExportData(Guid questionnaireId, long version, string type)
        {
            var fileType = GetFileTypeOrThrow(type);
          
            var allLevels = new Dictionary<string, byte[]>();

            var questionnarieLevel = GetAllQuestionnaireLevels(questionnaireId, version);

            foreach (var levelId in questionnarieLevel)
            {
                CollectLevels(
                    interviewDataExportViewFactory.Load(new InterviewDataExportInputModel(questionnaireId, version,
                                                                                          levelId)),
                    allLevels,
                    fileType);
            }

            supplier.AddCompletedResults(allLevels);

            return allLevels;
        }

        private HashSet<Guid?> GetAllQuestionnaireLevels(Guid questionnarieid, long version)
        {
            var levels = new HashSet<Guid?> { null };
            var questionnarieLevelStructure = this.questionnaireLevelStorage.GetById(questionnarieid, version);
            foreach (var levelId in questionnarieLevelStructure.PropagationScopes.Keys)
            {
                levels.Add(levelId);
            }
            return levels;
        }

        protected void CollectLevels(
            InterviewDataExportView records,
            Dictionary<string, byte[]> container,
            FileType type)
        {
            string fileName = GetName(records.LevelName, container, 0);

            container.Add(fileName, provider.DoExportToStream(records));

            this.supplier.BuildContent(records, string.Empty, fileName, type);
        }

        private FileType GetFileTypeOrThrow(string type)
        {
            if (type != CSVFORMAT && type != TABFORMAT)
                throw new InvalidOperationException("file type doesn't support");
            return type == CSVFORMAT ? FileType.Csv : FileType.Tab;
        }

        protected string GetName(string name, Dictionary<string, byte[]> container, int i)
        {
            string fileNameWithoutInvalidFileNameChars = Path.GetInvalidFileNameChars()
                                                             .Aggregate(name, (current, c) => current.Replace(c, '_'));
            string fileNameWithExtension = string.Concat(RemoveNonAscii(fileNameWithoutInvalidFileNameChars),
                                                         i == 0 ? (object) string.Empty : i, CSVFILEEXTENSION);

            return !container.ContainsKey(fileNameWithExtension)
                       ? fileNameWithExtension
                       : this.GetName(name, container, i + 1);
        }

        protected string RemoveNonAscii(string s)
        {
            return Regex.Replace(s, @"[^\u0000-\u007F]", string.Empty);
        }
    }

}
