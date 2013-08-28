using System;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class GeoLocationQuestionAnswered : QuestionAnswered
    {
        public GeoPosition Position { get; private set; }

        public GeoLocationQuestionAnswered(Guid userId, Guid questionId, int[] propagationVector, DateTime answerTime, GeoPosition answer) 
            : base(userId, questionId, propagationVector, answerTime)
        {
            this.Position = answer;
        }
    }
}
