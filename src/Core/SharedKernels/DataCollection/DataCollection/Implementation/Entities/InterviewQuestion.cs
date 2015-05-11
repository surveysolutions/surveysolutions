using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Entities
{
    public class InterviewGroup
    {
        public Guid Id { get; set; }
        public decimal[] RosterVector { get; set; }
        public bool IsDisabled { get; set; }
    }

    public class InterviewRoster : InterviewGroup
    {
        public decimal[] ParentRosterVector { get; set; }
        public decimal RowCode { get; set; }
        public string Title { get; set; }
    }

    public abstract class BaseInterviewAnswer
    {
        protected BaseInterviewAnswer()
        {
            this.QuestionState = QuestionState.Valid | QuestionState.Enabled;
        }

        protected BaseInterviewAnswer(Guid id, decimal[] rosterVector)
            : this()
        {
            this.Id = id;
            this.RosterVector = rosterVector;
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

    public class SingleOptionAnswer : BaseInterviewAnswer
    {
        public decimal Answer { get; set; }

        public SingleOptionAnswer() { }
        public SingleOptionAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class LinkedSingleOptionAnswer : BaseInterviewAnswer
    {
        public decimal[] Answer { get; set; }

        public LinkedSingleOptionAnswer() { }
        public LinkedSingleOptionAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class MultiOptionAnswer : BaseInterviewAnswer
    {
        public decimal[] Answers { get; set; }

        public MultiOptionAnswer() { }
        public MultiOptionAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class LinkedMultiOptionAnswer : BaseInterviewAnswer
    {
        public decimal[][] Answers { get; set; }

        public LinkedMultiOptionAnswer() { }
        public LinkedMultiOptionAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class IntegerNumericAnswer : BaseInterviewAnswer
    {
        public int? Answer { get; set; }

        public IntegerNumericAnswer() { }
        public IntegerNumericAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class RealNumericAnswer : BaseInterviewAnswer
    {
        public decimal Answer { get; set; }

        public RealNumericAnswer() { }
        public RealNumericAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class MaskedTextAnswer : BaseInterviewAnswer
    {
        public string Answer { get; set; }

        public MaskedTextAnswer() { }
        public MaskedTextAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class TextListAnswer : BaseInterviewAnswer
    {
        public Tuple<decimal, string>[] Answers { get; set; }

        public TextListAnswer() { }
        public TextListAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class QrBarcodeAnswer : BaseInterviewAnswer
    {
        public string Answer { get; set; }

        public QrBarcodeAnswer() { }
        public QrBarcodeAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class MultimediaAnswer : BaseInterviewAnswer
    {
        public string PictureFileName { get; set; }

        public MultimediaAnswer() { }
        public MultimediaAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class DateTimeAnswer : BaseInterviewAnswer
    {
        public DateTime Answer { get; set; }

        public DateTimeAnswer() { }
        public DateTimeAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class GpsCoordinatesAnswer : BaseInterviewAnswer
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Accuracy { get; set; }
        public double Altitude { get; set; }

        public GpsCoordinatesAnswer() { }
        public GpsCoordinatesAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }
}
