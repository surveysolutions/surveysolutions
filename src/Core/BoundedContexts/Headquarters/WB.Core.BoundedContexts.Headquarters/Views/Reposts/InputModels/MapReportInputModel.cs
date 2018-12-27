using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels
{
    public class MapReportInputModel
    {
        public Guid QuestionnaireId { get; set; }
        public long? QuestionnaireVersion { get; set; }

        public string Variable { get; set; }

        public double East { get; set; }
        public double North { get; set; }

        public double West { get; set; }
        public double South { get; set; }

        /// <summary>
        /// Will be -1 if zoom is not known at that moment
        /// </summary>
        public int Zoom { get; set; }
        public int ClientMapWidth { get; set; }
    }
}
