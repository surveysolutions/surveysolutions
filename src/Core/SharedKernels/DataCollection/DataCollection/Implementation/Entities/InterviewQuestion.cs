using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Utils;
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

        protected void MarkAnswered()
        {
            this.QuestionState |= QuestionState.Answered;
        }

        protected void MarkUnAnswered()
        {
            this.QuestionState &= ~QuestionState.Answered;
        }
    }

    public class SingleOptionAnswer : BaseInterviewAnswer
    {
        public decimal Answer { get; private set; }

        public SingleOptionAnswer() { }
        public SingleOptionAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }

        public void SetAnswer(decimal answer)
        {
            Answer = answer;
            MarkAnswered();
        }
    }

    public class LinkedSingleOptionAnswer : BaseInterviewAnswer
    {
        public decimal[] Answer { get; private set; }

        public LinkedSingleOptionAnswer() { }
        public LinkedSingleOptionAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }

        public void SetAnswer(decimal[] answer)
        {
            Answer = answer;

            if (Answer.Any())
                MarkAnswered();
            else
                MarkUnAnswered();
        }
    }

    public class MultiOptionAnswer : BaseInterviewAnswer
    {
        public decimal[] Answers { get; private set; }

        public MultiOptionAnswer() { }
        public MultiOptionAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }

        public void SetAnswers(decimal[] answer)
        {
            Answers = answer;

            if (Answers.Any())
                MarkAnswered();
            else
                MarkUnAnswered();
        }
    }

    public class LinkedMultiOptionAnswer : BaseInterviewAnswer
    {
        public decimal[][] Answers { get; private set; }

        public LinkedMultiOptionAnswer() { }
        public LinkedMultiOptionAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }


        public void SetAnswers(decimal[][] answer)
        {
            Answers = answer;

            if (Answers.Any())
                MarkAnswered();
            else
                MarkUnAnswered();
        }
    }

    public class IntegerNumericAnswer : BaseInterviewAnswer
    {
        public int? Answer { get; private set; }

        public IntegerNumericAnswer() { }
        public IntegerNumericAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }

        public void SetAnswer(int? answer)
        {
            Answer = answer;

            if (Answer.HasValue)
                MarkAnswered();
            else
                MarkUnAnswered();
        }
    }

    public class RealNumericAnswer : BaseInterviewAnswer
    {
        public decimal? Answer { get; private set; }

        public RealNumericAnswer() { }
        public RealNumericAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }

        public void SetAnswer(decimal? answer)
        {
            Answer = answer;

            if (Answer.HasValue)
                MarkAnswered();
            else
                MarkUnAnswered();
        }
    }

    public class MaskedTextAnswer : BaseInterviewAnswer
    {
        public string Answer { get; private set; }

        public MaskedTextAnswer() { }
        public MaskedTextAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }

        public void SetAnswer(string answer)
        {
            Answer = answer;

            if (Answer.IsNullOrEmpty())
                MarkUnAnswered();
            else
                MarkAnswered();
        }
    }

    public class TextListAnswer : BaseInterviewAnswer
    {
        public Tuple<decimal, string>[] Answers { get; private set; }

        public TextListAnswer() { }
        public TextListAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }

        public void SetAnswers(Tuple<decimal, string>[] answer)
        {
            Answers = answer;

            if (Answers.Any())
                MarkUnAnswered();
            else
                MarkAnswered();
        }
    }

    public class QrBarcodeAnswer : BaseInterviewAnswer
    {
        public string Answer { get; private set; }

        public QrBarcodeAnswer() { }
        public QrBarcodeAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }

        public void SetAnswer(string answer)
        {
            Answer = answer;

            if (Answer.IsNullOrEmpty())
                MarkUnAnswered();
            else
                MarkAnswered();
        }
    }

    public class MultimediaAnswer : BaseInterviewAnswer
    {
        public string PictureFileName { get; private set; }

        public MultimediaAnswer() { }
        public MultimediaAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }

        public void SetAnswer(string answer)
        {
            PictureFileName = answer;

            if (PictureFileName.IsNullOrEmpty())
                MarkUnAnswered();
            else
                MarkAnswered();
        }
    }

    public class DateTimeAnswer : BaseInterviewAnswer
    {
        public DateTime Answer { get; private set; }

        public DateTimeAnswer() { }
        public DateTimeAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }

        public void SetAnswer(DateTime answer)
        {
            Answer = answer;
            MarkAnswered();
        }
    }

    public class GpsCoordinatesAnswer : BaseInterviewAnswer
    {
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public double Accuracy { get; private set; }
        public double Altitude { get; private set; }

        public GpsCoordinatesAnswer() { }
        public GpsCoordinatesAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }

        public void SetAnswer(double latitude, double longitude, double accuracy, double altitude)
        {
            Latitude = latitude;
            Longitude = longitude;
            Accuracy = accuracy;
            Altitude = altitude;

            MarkAnswered();
        }
    }
}
