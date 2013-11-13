using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Main.Core.View;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.Implementation.Services.DataExport
{
    internal class DataExportService : IDataExportService
    {
        private readonly IEnvironmentSupplier<InterviewDataExportView> supplier;
        private readonly IViewFactory<InterviewDataExportInputModel, InterviewDataExportView> interviewDataExportViewFactory;
        private readonly IVersionedReadSideRepositoryReader<QuestionnaireRosterStructure> questionnaireLevelStorage;

        public DataExportService(
            IEnvironmentSupplier<InterviewDataExportView> supplier, 
            IViewFactory<InterviewDataExportInputModel, InterviewDataExportView> interviewDataExportViewFactory, 
            IVersionedReadSideRepositoryReader<QuestionnaireRosterStructure> questionnaireLevelStorage)
        {
            this.supplier = supplier;
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
                    new IterviewExporter(fileType), fileType);
            }

            supplier.AddCompletedResults(allLevels);

            return allLevels;
        }

        private HashSet<Guid?> GetAllQuestionnaireLevels(Guid questionnarieid, long version)
        {
            var levels = new HashSet<Guid?> { null };
            var questionnarieLevelStructure = this.questionnaireLevelStorage.GetById(questionnarieid, version);
            foreach (var levelId in questionnarieLevelStructure.RosterScopes.Keys)
            {
                levels.Add(levelId);
            }
            return levels;
        }

        protected void CollectLevels(
            InterviewDataExportView records,
            Dictionary<string, byte[]> container,
             IExportProvider<InterviewDataExportView> provider,
            FileType type)
        {
            string fileName = GetName(records.LevelName,type, container, 0);

            container.Add(fileName, provider.DoExportToStream(records));

            this.supplier.BuildContent(records, string.Empty, fileName, type);
        }

        private FileType GetFileTypeOrThrow(string type)
        {
            if (type != InterviewExportConstants.CSVFORMAT && type != InterviewExportConstants.TABFORMAT)
                throw new InvalidOperationException("file type doesn't support");
            return type == InterviewExportConstants.CSVFORMAT ? FileType.Csv : FileType.Tab;
        }

        protected string GetName(string name,FileType type, Dictionary<string, byte[]> container, int i)
        {
            string fileNameWithoutInvalidFileNameChars = Path.GetInvalidFileNameChars()
                                                             .Aggregate(name, (current, c) => current.Replace(c, '_'));
            string fileNameWithExtension = string.Concat(RemoveNonAscii(fileNameWithoutInvalidFileNameChars),
                                                         i == 0 ? (object)string.Empty : i, GetFileExtension(type));

            return !container.ContainsKey(fileNameWithExtension)
                       ? fileNameWithExtension
                       : this.GetName(name, type, container, i + 1);
        }

        protected string GetFileExtension(FileType type)
        {
            return "." + (type == FileType.Csv ? InterviewExportConstants.CSVFORMAT : InterviewExportConstants.TABFORMAT);
        } 

        protected string RemoveNonAscii(string s)
        {
            return Regex.Replace(s, @"[^\u0000-\u007F]", string.Empty);
        }
    }

}
