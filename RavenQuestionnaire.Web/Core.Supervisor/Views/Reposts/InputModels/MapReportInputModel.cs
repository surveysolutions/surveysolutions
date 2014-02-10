using System;

namespace Core.Supervisor.Views.Reposts.InputModels
{
    public class MapReportInputModel
    {
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }
        public string Variable { get; set; }
    }
}