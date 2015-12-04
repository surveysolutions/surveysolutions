using System.Linq;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories
{
    public class MapReport : IViewFactory<MapReportInputModel, MapReportView>
    {
        private readonly IQueryableReadSideRepositoryReader<MapReportPoint> answersByVariableStorage;

        public MapReport(IQueryableReadSideRepositoryReader<MapReportPoint> answersByVariableStorage)
        {
            this.answersByVariableStorage = answersByVariableStorage;
        }

        public MapReportView Load(MapReportInputModel input)
        {
            var points = this.answersByVariableStorage.Query(_ => _.Where(x => x.QuestionnaireId == input.QuestionnaireId && 
                                                                               x.QuestionnaireVersion == input.QuestionnaireVersion &&
                                                                               x.Variable == input.Variable)
                                                                   .Select(x => new {x.InterviewId, x.Latitude, x.Longitude})
                                                                   .ToList());

            var mapPointViews = points.GroupBy(x => x.InterviewId).Select(x => new MapPointView {
                        InterviewId = x.Key.ToString(),
                        Answers = string.Join("|", x.Select(val => $"{val.Latitude};{val.Longitude}"))
                    }).ToArray();
            return new MapReportView
            {
                Points = mapPointViews
            };
        }
    }
}
