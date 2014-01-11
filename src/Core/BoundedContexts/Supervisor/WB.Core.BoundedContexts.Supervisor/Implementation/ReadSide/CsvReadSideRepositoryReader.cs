using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using WB.Core.BoundedContexts.Supervisor.Implementation.Services.DataExport;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Supervisor.Implementation.ReadSide
{
    internal class CsvInterviewDataExportViewWriter : IReadSideRepositoryWriter<InterviewDataExportView>
    {
        private readonly IDataExportService dataExportService;
        private readonly IterviewExporter interviewExporter;

        public CsvInterviewDataExportViewWriter(IDataExportService dataExportService)
        {
            this.dataExportService = dataExportService;
            this.interviewExporter = new IterviewExporter();
        }

        public InterviewDataExportView GetById(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Remove(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Store(InterviewDataExportView view, Guid id)
        {
            var levelIdToDataFilePathMap = dataExportService.GetLevelIdToDataFilePathMap(view);
            foreach (var level in view.Levels)
            {
                interviewExporter.AddRecord(
                    levelIdToDataFilePathMap[level.LevelId],
                    level);
            }
        }
    }
}
