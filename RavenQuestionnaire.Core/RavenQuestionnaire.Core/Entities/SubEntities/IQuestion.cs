using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities
{
    public interface IQuestion : IComposite
    {
        string QuestionText { get; set; }
        QuestionType QuestionType { get; set; }
        string ConditionExpression { get; set; }
        string ValidationExpression { get; set; }
        string ValidationMessage { get; set; }
        string StataExportCaption { get; set; }
        string Instructions { get; set; }
        List<Image> Cards { get; set; }
        Order AnswerOrder { get; set; }
        bool Featured { get; set; }
        bool Mandatory { get; set; }
        string Comments { get; set; }
    }

}
