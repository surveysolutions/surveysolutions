using System;
using System.Collections.Generic;
using System.Linq;

using WB.Core.GenericSubdomains.Utils;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities
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
            this.IsEnabled = true;
            this.IsValid = true;
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

        public bool IsValid { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsAnswered { get; set; }

        protected void MarkAnswered()
        {
            IsAnswered = true;
        }

        protected void MarkUnAnswered()
        {
            IsAnswered = false;
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
            this.Answer = answer;
            this.MarkAnswered();
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
            this.Answer = answer;

            if (this.Answer.Any())
                this.MarkAnswered();
            else
                this.MarkUnAnswered();
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
            this.Answers = answer;

            if (this.Answers.Any())
                this.MarkAnswered();
            else
                this.MarkUnAnswered();
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
            this.Answers = answer;

            if (this.Answers.Any())
                this.MarkAnswered();
            else
                this.MarkUnAnswered();
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
            this.Answer = answer;

            if (this.Answer.HasValue)
                this.MarkAnswered();
            else
                this.MarkUnAnswered();
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
            this.Answer = answer;

            if (this.Answer.HasValue)
                this.MarkAnswered();
            else
                this.MarkUnAnswered();
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
            this.Answer = answer;

            if (this.Answer.IsNullOrEmpty())
                this.MarkUnAnswered();
            else
                this.MarkAnswered();
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
            this.Answers = answer;

            if (this.Answers.Any())
                this.MarkUnAnswered();
            else
                this.MarkAnswered();
        }
    }

    public class QRBarcodeAnswer : BaseInterviewAnswer
    {
        public string Answer { get; private set; }

        public QRBarcodeAnswer() { }
        public QRBarcodeAnswer(Guid id, decimal[] rosterVector)
            : base(id, rosterVector)
        {
        }

        public void SetAnswer(string answer)
        {
            this.Answer = answer;

            if (this.Answer.IsNullOrEmpty())
                this.MarkUnAnswered();
            else
                this.MarkAnswered();
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
            this.PictureFileName = answer;

            if (this.PictureFileName.IsNullOrEmpty())
                this.MarkUnAnswered();
            else
                this.MarkAnswered();
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
            this.Answer = answer;
            this.MarkAnswered();
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
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Accuracy = accuracy;
            this.Altitude = altitude;

            this.MarkAnswered();
        }
    }
}
