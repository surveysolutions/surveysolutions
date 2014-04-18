using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo
{
    public class GeoLocationDetailsView : QuestionDetailsView
    {
        public GeoLocationDetailsView()
        {
            Type = QuestionType.GpsCoordinates;
        }

        public override sealed QuestionType Type { get; set; }
    }
}