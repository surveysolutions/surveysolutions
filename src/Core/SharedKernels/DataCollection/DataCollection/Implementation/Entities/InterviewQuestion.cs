using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Entities
{
    public class InterviewGroupModel
    {
        public Guid Id { get; set; }
        public decimal[] RosterVector { get; set; }
        public bool IsDisabled { get; set; }
    }

    public class InterviewRosterModel : InterviewGroupModel
    {
        public decimal[] ParentRosterVector { get; set; }
        public decimal RowCode { get; set; }
        public string Title { get; set; }
    }

    public abstract class AbstractInterviewAnswerModel
    {
        protected AbstractInterviewAnswerModel() { }

        protected AbstractInterviewAnswerModel(Guid id, decimal[] rosterVector)
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

    public class SingleOptionAnswerModel : AbstractInterviewAnswerModel
    {
        public decimal Answer { get; set; }

        public SingleOptionAnswerModel() { }
        public SingleOptionAnswerModel(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class LinkedSingleOptionAnswerModel : AbstractInterviewAnswerModel
    {
        public decimal[] Answer { get; set; }

        public LinkedSingleOptionAnswerModel() { }
        public LinkedSingleOptionAnswerModel(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class MultiOptionAnswerModel : AbstractInterviewAnswerModel
    {
        public decimal[] Answers { get; set; }

        public MultiOptionAnswerModel() { }
        public MultiOptionAnswerModel(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class LinkedMultiOptionAnswerModel : AbstractInterviewAnswerModel
    {
        public decimal[][] Answers { get; set; }

        public LinkedMultiOptionAnswerModel() { }
        public LinkedMultiOptionAnswerModel(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class IntegerNumericAnswerModel : AbstractInterviewAnswerModel
    {
        public long Answer { get; set; }

        public IntegerNumericAnswerModel() { }
        public IntegerNumericAnswerModel(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class RealNumericAnswerModel : AbstractInterviewAnswerModel
    {
        public decimal Answer { get; set; }

        public RealNumericAnswerModel() { }
        public RealNumericAnswerModel(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class MaskedTextAnswerModel : AbstractInterviewAnswerModel
    {
        public string Answer { get; set; }

        public MaskedTextAnswerModel() { }
        public MaskedTextAnswerModel(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class TextListAnswerModel : AbstractInterviewAnswerModel
    {
        public Tuple<decimal, string>[] Answers { get; set; }

        public TextListAnswerModel() { }
        public TextListAnswerModel(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class QrBarcodeAnswerModel : AbstractInterviewAnswerModel
    {
        public string Answer { get; set; }

        public QrBarcodeAnswerModel() { }
        public QrBarcodeAnswerModel(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class MultimediaAnswerModel : AbstractInterviewAnswerModel
    {
        public string PictureFileName { get; set; }

        public MultimediaAnswerModel() { }
        public MultimediaAnswerModel(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class DateTimeAnswerModel : AbstractInterviewAnswerModel
    {
        public DateTime Answer { get; set; }

        public DateTimeAnswerModel() { }
        public DateTimeAnswerModel(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class GpsCoordinatesAnswerModel : AbstractInterviewAnswerModel
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Accuracy { get; set; }
        public double Altitude { get; set; }

        public GpsCoordinatesAnswerModel() { }
        public GpsCoordinatesAnswerModel(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }
}
