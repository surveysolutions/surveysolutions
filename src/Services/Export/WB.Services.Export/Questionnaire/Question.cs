using System;
using System.Collections.Generic;

namespace WB.Services.Export.Questionnaire
{
    public class Question : IQuestionnaireEntity
    {
        public Question()
        {
            this.Answers = new List<Answer>();
            this.Children = Array.Empty<IQuestionnaireEntity>();
        }

        public Guid PublicKey { get; set; }
        public IEnumerable<IQuestionnaireEntity> Children { get; set; }
        public IQuestionnaireEntity GetParent()
        {
            return Parent;
        }

        public IQuestionnaireEntity Parent { get; set; }

        public QuestionType QuestionType { get; set; }
        public bool Featured { get; set; }
        public string VariableName { get; set; }
        public string VariableLabel { get; set; }
        public string QuestionText { get; set; }
        public List<Answer> Answers { get; set; }

        public Guid? LinkedToQuestionId { get; set; }
        public Guid? LinkedToRosterId { get; set; }

        public virtual bool IsQuestionLinked()
        {
            return LinkedToQuestionId != null || LinkedToRosterId != null;
        }
    }

    public class Answer
    {
        public string AnswerText { get; set; }

        public string AnswerValue { get; set; }
    }

    public class MultyOptionsQuestion : Question
    {
        public bool YesNoView { get; set; }
        public bool AreAnswersOrdered { get; set; }
    }

    public class DateTimeQuestion : Question
    {
        public bool IsTimestamp { get; set; }
    }

    public class SingleQuestion : Question
    {
    }

    public class TextListQuestion : Question
    {
        public int? MaxAnswerCount { get; set; }
    }

    public class GpsCoordinateQuestion : Question
    {
    }

    public enum QuestionType
    {
        SingleOption = 0,

        MultyOption = 3,

        Numeric = 4,

        DateTime = 5,

        GpsCoordinates = 6,

        Text = 7,

        TextList = 9,

        QRBarcode = 10,

        Multimedia = 11,

        Area = 12,

        Audio = 13
    }
}
