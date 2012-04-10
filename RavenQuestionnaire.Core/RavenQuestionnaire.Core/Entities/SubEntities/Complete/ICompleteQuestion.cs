using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete
{
    public interface ICompleteQuestion : IQuestion
    {
        bool Enabled { get; set; }
        bool Valid { get; set; }
        DateTime? AnswerDate { get; set; }
    }

    public interface ICompleteQuestion<T> : ICompleteQuestion, IQuestion<T> where T : IAnswer
    {
    }
}
