using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class AnswerAreaQuestionCommand : AnswerQuestionCommand
    {
        public string Geometry { get; private set; }
        public string MapName { get; private set; }
        public double Area { get; private set; }

        public AnswerAreaQuestionCommand(Guid interviewId, Guid userId, Guid questionId, decimal[] rosterVector, 
            DateTime answerTime, string geometry, string mapName, double area)
            : base(interviewId, userId, questionId, rosterVector, answerTime)
        {
            this.Geometry = geometry;
            this.MapName = mapName;
            this.Area = area;
        }
    }
}