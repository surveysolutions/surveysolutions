using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Main.Core.Export;
using Main.Core.View;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.ReadSide;

namespace WB.Core.BoundedContexts.Supervisor.Implementation.Services.DataExport
{
    internal class CsvDataExportService : IDataExportService
    {
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

        public IDictionary<string, byte[]> ExportData(Guid questionnarieid, long version, string type)
        {
            var fileType = GetFileTypeOrThrow(type);
          
            var allLevels = new Dictionary<string, byte[]>();

            var questionnarieLevel = GetAllQuestionnarieLevels(questionnarieid, version);

            foreach (var levelId in questionnarieLevel)
            {
                CollectLevels(
                    interviewDataExportViewFactory.Load(new InterviewDataExportInputModel(questionnarieid, version,
                                                                                          levelId)),
                    allLevels,
                    fileType);
            }

            supplier.AddCompletedResults(allLevels);

            return allLevels;
        }

        private HashSet<Guid?> GetAllQuestionnarieLevels(Guid questionnarieid, long version)
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
            if (type != "csv" && type != "tab")
                throw new InvalidOperationException("file type doesn't supprot");
            return type == "csv" ? FileType.Csv : FileType.Tab;
        }

        protected string GetName(string name, Dictionary<string, byte[]> container, int i)
        {
            if (i == 0)
            {
                if (!container.ContainsKey(name + ".csv"))
                {
                    return RemoveNonUnicode(name) + ".csv";
                }
                else
                {
                    return this.GetName(name, container, i + 1);
                }
            }

            if (!container.ContainsKey(name + i + ".csv"))
            {
                return RemoveNonUnicode(name) + i + ".csv";
            }
            else
            {
                return this.GetName(name, container, i + 1);
            }
        }
        protected string RemoveNonUnicode(string s)
        {
            return Regex.Replace(s, @"[^\u0000-\u007F]", string.Empty);
        }
    }

}
