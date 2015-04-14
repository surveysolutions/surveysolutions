using System;
using System.Collections.Generic;

using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Entities
{
    public class InterviewRosterModel
    {
        public Guid Id { get; set; }
        public decimal[] ParentRosterVector { get; set; }
        public decimal RowCode { get; set; }
        public decimal[] RosterVector { get; set; }
    }

    public class InterviewGroupModel
    {
        public Guid Id { get; set; }
        public decimal[] RosterVector { get; set; }
    }

    public abstract class AbstractInterviewQuestionModel
    {
        protected AbstractInterviewQuestionModel() { }

        protected AbstractInterviewQuestionModel(Guid id, decimal[] rosterVector)
            : this()
        {
            this.Id = id;
            this.RosterVector = rosterVector;
            this.QuestionState = QuestionState.Valid | QuestionState.Enabled;
        }

        public Guid Id { get; set; }
        public decimal[] RosterVector { get; set; }

        public List<string> Comments { get; set; }

        public QuestionState QuestionState { get; set; }

        public bool IsInvalid()
        {
            return !this.QuestionState.HasFlag(QuestionState.Valid);
        }

        public bool IsDisabled()
        {
            return !this.QuestionState.HasFlag(QuestionState.Enabled);
        }

        public bool IsAnswered()
        {
            return this.QuestionState.HasFlag(QuestionState.Answered);
        }
    }

    public class SingleOptionQuestionModel : AbstractInterviewQuestionModel
    {
        public decimal Answer { get; set; }

        public SingleOptionQuestionModel() { }
        public SingleOptionQuestionModel(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class LinkedSingleOptionQuestionModel : AbstractInterviewQuestionModel
    {
        public decimal[] Answer { get; set; }

        public LinkedSingleOptionQuestionModel() { }
        public LinkedSingleOptionQuestionModel(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class MultiOptionQuestionModel : AbstractInterviewQuestionModel
    {
        public decimal[] Answers { get; set; }

        public MultiOptionQuestionModel() { }
        public MultiOptionQuestionModel(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class LinkedMultiOptionQuestionModel : AbstractInterviewQuestionModel
    {
        public decimal[][] Answers { get; set; }

        public LinkedMultiOptionQuestionModel() { }
        public LinkedMultiOptionQuestionModel(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class IntegerNumericQuestionModel : AbstractInterviewQuestionModel
    {
        public long Answer { get; set; }

        public IntegerNumericQuestionModel() { }
        public IntegerNumericQuestionModel(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class RealNumericQuestionModel : AbstractInterviewQuestionModel
    {
        public decimal Answer { get; set; }

        public RealNumericQuestionModel() { }
        public RealNumericQuestionModel(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class MaskedTextQuestionModel : AbstractInterviewQuestionModel
    {
        public string Answer { get; set; }

        public MaskedTextQuestionModel() { }
        public MaskedTextQuestionModel(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class TextListQuestionModel : AbstractInterviewQuestionModel
    {
        public Tuple<decimal, string>[] Answers { get; set; }

        public TextListQuestionModel() { }
        public TextListQuestionModel(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class QrBarcodeQuestionModel : AbstractInterviewQuestionModel
    {
        public string Answer { get; set; }

        public QrBarcodeQuestionModel() { }
        public QrBarcodeQuestionModel(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class MultimediaQuestionModel : AbstractInterviewQuestionModel
    {
        public string PictureFileName { get; set; }

        public MultimediaQuestionModel() { }
        public MultimediaQuestionModel(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class DateTimeQuestionModel : AbstractInterviewQuestionModel
    {
        public DateTime Answer { get; set; }

        public DateTimeQuestionModel() { }
        public DateTimeQuestionModel(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class GpsCoordinatesQuestionModel : AbstractInterviewQuestionModel
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Accuracy { get; set; }
        public double Altitude { get; set; }

        public GpsCoordinatesQuestionModel() { }
        public GpsCoordinatesQuestionModel(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }
}
