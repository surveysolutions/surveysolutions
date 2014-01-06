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
        private readonly IEnvironmentSupplier<InterviewDataExportLevelView> supplier;
        private readonly IViewFactory<InterviewDataExportInputModel, InterviewDataExportView> interviewDataExportViewFactory;

        public DataExportService(
            IEnvironmentSupplier<InterviewDataExportLevelView> supplier,
            IViewFactory<InterviewDataExportInputModel, InterviewDataExportView> interviewDataExportViewFactory)
        {
            this.supplier = supplier;
            this.interviewDataExportViewFactory = interviewDataExportViewFactory;
        }

        public IDictionary<string, byte[]> ExportData(Guid questionnaireId, long version, string type)
        {
            var fileType = GetFileTypeOrThrow(type);

            var allLevels = new Dictionary<string, byte[]>();

            CollectLevels(
                interviewDataExportViewFactory.Load(new InterviewDataExportInputModel(questionnaireId, version)),
                allLevels,
                new IterviewExporter(fileType), fileType);

            supplier.AddCompletedResults(allLevels);

            return allLevels;
        }

        protected void CollectLevels(
            InterviewDataExportView records,
            Dictionary<string, byte[]> container,
            IExportProvider<InterviewDataExportLevelView> provider,
            FileType type)
        {
            foreach (var interviewDataExportLevelView in records.Levels)
            {
                string fileName = GetName(interviewDataExportLevelView.LevelName, type, container, 0);

                container.Add(fileName, provider.DoExportToStream(interviewDataExportLevelView));

                this.supplier.BuildContent(interviewDataExportLevelView, string.Empty, fileName, type);
            }
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
