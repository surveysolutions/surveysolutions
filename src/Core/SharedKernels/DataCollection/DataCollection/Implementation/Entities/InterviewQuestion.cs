using System;
using System.Collections.Generic;

using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Entities
{
    public abstract class AbstractInterviewQuestion
    {
        protected AbstractInterviewQuestion(Guid id, decimal[] rosterVector)
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

    public class SingleOptionQuestionModel : AbstractInterviewQuestion
    {
        public decimal Answer { get; set; }

        public SingleOptionQuestionModel(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class LinkedSingleOptionQuestionViewModel : AbstractInterviewQuestion
    {
        public decimal[] Answer { get; set; }

        public LinkedSingleOptionQuestionViewModel(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class MultiOptionQuestionViewModel : AbstractInterviewQuestion
    {
        public decimal[] Answers { get; set; }

        public MultiOptionQuestionViewModel(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class LinkedMultiOptionQuestionViewModel : AbstractInterviewQuestion
    {
        public decimal[][] Answers { get; set; }

        public LinkedMultiOptionQuestionViewModel(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class IntegerNumericQuestionViewModel : AbstractInterviewQuestion
    {
        public long Answer { get; set; }

        public IntegerNumericQuestionViewModel(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class RealNumericQuestionViewModel : AbstractInterviewQuestion
    {
        public decimal Answer { get; set; }

        public RealNumericQuestionViewModel(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class MaskedTextQuestionViewModel : AbstractInterviewQuestion
    {
        public string Answer { get; set; }

        public MaskedTextQuestionViewModel(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class TextListQuestionViewModel : AbstractInterviewQuestion
    {
        public Tuple<decimal, string>[] Answers { get; set; }

        public TextListQuestionViewModel(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class QrBarcodeQuestionViewModel : AbstractInterviewQuestion
    {
        public string Answer { get; set; }

        public QrBarcodeQuestionViewModel(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class MultimediaQuestionViewModel : AbstractInterviewQuestion
    {
        public string PictureFileName { get; set; }

        public MultimediaQuestionViewModel(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class DateTimeQuestionViewModel : AbstractInterviewQuestion
    {
        public DateTime Answer { get; set; }

        public DateTimeQuestionViewModel(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }

    public class GpsCoordinatesQuestionViewModel : AbstractInterviewQuestion
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Accuracy { get; set; }
        public double Altitude { get; set; }

        public GpsCoordinatesQuestionViewModel(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }
    }
}
