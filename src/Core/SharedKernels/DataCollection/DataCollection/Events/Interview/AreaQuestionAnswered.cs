using System;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class AreaQuestionAnswered : QuestionAnswered
    {
        public string Geometry { set; get; }
        public string MapName { set; get; }
        public double AreaSize { set; get; }

        public AreaQuestionAnswered(Guid userId, Guid questionId, decimal[] rosterVector,
                                    DateTime answerTimeUtc, string geometry, string mapName, double areaSize)
            : base(userId, questionId, rosterVector, answerTimeUtc)
        {
            this.Geometry = geometry;
            this.MapName = mapName;
            this.AreaSize = areaSize;
        }
    }
}