using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Newtonsoft.Json;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.Observers;
using RavenQuestionnaire.Core.ExpressionExecutors;

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    public interface IQuestion : IComposite, ITriggerable
    {
        string QuestionText { get; set; }
        QuestionType QuestionType { get; set; }
        string ConditionExpression { get; set; }
        string ValidationExpression { get; set; }
        string StataExportCaption { get; set; }
        string Instructions { get; set; }
        List<Image> Cards { get; set; }
        Order AnswerOrder { get; set; }
        bool Featured { get; set; }
        Dictionary<string, object> Attributes { get; set; }
    }

}
