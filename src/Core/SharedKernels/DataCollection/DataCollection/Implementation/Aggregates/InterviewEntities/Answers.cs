﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Portable;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers
{
    public class InterviewAnswer
    {
        public Identity Identity { get; set; }
        public AbstractAnswer Answer { get; set; }
    }

    public abstract class AbstractAnswer
    {
    }

    [DebuggerDisplay("{ToString()}")]
    public class TextAnswer : AbstractAnswer
    {
        protected TextAnswer() { }
        private TextAnswer(string value)
        {
            this.Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string Value { get; protected set; }

        public static TextAnswer FromString(string value) => value != null ? new TextAnswer(value.Trim().RemoveControlChars()) : null;

        public override string ToString() => Value;

        protected bool Equals(TextAnswer other)
        {
            return string.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TextAnswer) obj);
        }

        public override int GetHashCode()
        {
            return (Value != null ? Value.GetHashCode() : 0);
        }
    }

    [DebuggerDisplay("{ToString()}")]
    public class NumericIntegerAnswer : AbstractAnswer
    {
        protected NumericIntegerAnswer() { }

        private NumericIntegerAnswer(int value)
        {
            this.Value = value;
        }

        public int Value { get; protected set; }

        public static NumericIntegerAnswer FromInt(int value) => new NumericIntegerAnswer(value);

        public override string ToString() => Value.ToString();

        protected bool Equals(NumericIntegerAnswer other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NumericIntegerAnswer) obj);
        }

        public override int GetHashCode()
        {
            return Value;
        }
    }

    [DebuggerDisplay("{ToString()}")]
    public class NumericRealAnswer : AbstractAnswer
    {
        protected NumericRealAnswer() { }

        private NumericRealAnswer(double value)
        {
            this.Value = value;
        }

        public double Value { get; protected set; }

        public static NumericRealAnswer FromDouble(double value) => new NumericRealAnswer(value);

        public static NumericRealAnswer FromDecimal(decimal value) => new NumericRealAnswer((double)value);

        public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);

        protected bool Equals(NumericRealAnswer other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NumericRealAnswer) obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }

    [DebuggerDisplay("{ToString()}")]
    public class DateTimeAnswer : AbstractAnswer
    {
        protected DateTimeAnswer() { }

        private DateTimeAnswer(DateTime value)
        {
            this.Value = value;
        }

        public DateTime Value { get; protected set; }

        public static DateTimeAnswer FromDateTime(DateTime value) => new DateTimeAnswer(value);

        public override string ToString() => Value.ToString("yyyy-MM-ddTHH:mm:ss");

        protected bool Equals(DateTimeAnswer other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DateTimeAnswer) obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
    
    [DebuggerDisplay("{ToString()}")]

    public class CategoricalFixedSingleOptionAnswer : AbstractAnswer
    {
        protected CategoricalFixedSingleOptionAnswer() { }

        private CategoricalFixedSingleOptionAnswer(int selectedValue)
        {
            this.SelectedValue = selectedValue;
        }

        public int SelectedValue { get; protected set; }

        public static CategoricalFixedSingleOptionAnswer FromInt(int selectedValue) => new CategoricalFixedSingleOptionAnswer(selectedValue);

        public static CategoricalFixedSingleOptionAnswer FromDecimal(decimal selectedValue) => new CategoricalFixedSingleOptionAnswer((int)selectedValue);

        public override string ToString() => SelectedValue.ToString();
                
        public string GetAnswerAsText(IQuestionnaire questionnaire, Guid questionId, int? parentAnswer)
        {
            return questionnaire.GetOptionForQuestionByOptionValue(questionId, SelectedValue, parentAnswer).Title;
        }

        protected bool Equals(CategoricalFixedSingleOptionAnswer other)
        {
            return SelectedValue == other.SelectedValue;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CategoricalFixedSingleOptionAnswer) obj);
        }

        public override int GetHashCode()
        {
            return SelectedValue;
        }
    }

    [DebuggerDisplay("{ToString()}")]

    public class CategoricalFixedMultiOptionAnswer : AbstractAnswer
    {
        protected CategoricalFixedMultiOptionAnswer() { }

        private CategoricalFixedMultiOptionAnswer(IEnumerable<int> checkedValues)
        {
            if (checkedValues == null) throw new ArgumentNullException(nameof(checkedValues));

            this.CheckedValues = checkedValues.ToReadOnlyCollection();
        }

        public IReadOnlyCollection<int> CheckedValues { get; protected set; }

        public IEnumerable<decimal> ToDecimals() => this.CheckedValues.Select(value => (decimal)value);

        public IEnumerable<int> ToInts() => this.CheckedValues;

        public static CategoricalFixedMultiOptionAnswer Convert(object obj)
        {
            switch (obj)
            {
                case int[] answerAsIntArray:
                    return answerAsIntArray.Any() 
                        ? new CategoricalFixedMultiOptionAnswer(answerAsIntArray) 
                        : null;
                case decimal[] answerAsDecimalArray:
                    return answerAsDecimalArray.Any()
                        ? new CategoricalFixedMultiOptionAnswer(answerAsDecimalArray.Select(value => (int) value))
                        : null;
                case HashSet<decimal> answerAsHashSet:
                    return answerAsHashSet.Any() 
                        ? new CategoricalFixedMultiOptionAnswer(answerAsHashSet.Select(value => (int)value)) 
                        : null;
            }

            return null;
        }

        public static CategoricalFixedMultiOptionAnswer FromIntArray(int[] checkedValues)
            => new CategoricalFixedMultiOptionAnswer(checkedValues);

        public static CategoricalFixedMultiOptionAnswer FromDecimalArray(decimal[] checkedValues)
            => checkedValues == null || !checkedValues.Any() ? null : new CategoricalFixedMultiOptionAnswer(checkedValues.Select(value => (int)value));

        public override string ToString() => string.Join(", ", CheckedValues);

        protected bool Equals(CategoricalFixedMultiOptionAnswer other)
        {
            return CheckedValues.SequenceEqual(other.CheckedValues);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CategoricalFixedMultiOptionAnswer) obj);
        }

        public override int GetHashCode()
        {
            return (CheckedValues != null ? CheckedValues.GetHashCode() : 0);
        }
    }

    [DebuggerDisplay("{ToString()}")]
    public class CategoricalLinkedSingleOptionAnswer : AbstractAnswer
    {
        protected CategoricalLinkedSingleOptionAnswer() { }

        private CategoricalLinkedSingleOptionAnswer(RosterVector selectedValue)
        {
            if (selectedValue == null) throw new ArgumentNullException(nameof(selectedValue));

            this.SelectedValue = selectedValue;
        }

        public RosterVector SelectedValue { get; protected set; }

        public static CategoricalLinkedSingleOptionAnswer FromRosterVector(RosterVector selectedValue)
            => selectedValue == null ? null : new CategoricalLinkedSingleOptionAnswer(selectedValue);

        public override string ToString() => SelectedValue.ToString();

        protected bool Equals(CategoricalLinkedSingleOptionAnswer other)
        {
            return Equals(SelectedValue, other.SelectedValue);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CategoricalLinkedSingleOptionAnswer) obj);
        }

        public override int GetHashCode()
        {
            return (SelectedValue != null ? SelectedValue.GetHashCode() : 0);
        }
    }

    [DebuggerDisplay("{ToString()}")]
    public class CategoricalLinkedMultiOptionAnswer : AbstractAnswer
    {
        protected CategoricalLinkedMultiOptionAnswer() { }

        private CategoricalLinkedMultiOptionAnswer(IEnumerable<RosterVector> checkedValues)
        {
            if (checkedValues == null) throw new ArgumentNullException(nameof(checkedValues));

            this.CheckedValues = checkedValues.ToReadOnlyCollection();
        }

        public IReadOnlyCollection<RosterVector> CheckedValues { get; protected set; }

        public static CategoricalLinkedMultiOptionAnswer FromRosterVectors(IEnumerable<RosterVector> checkedValues)
            => checkedValues == null ? null : new CategoricalLinkedMultiOptionAnswer(checkedValues);

        public RosterVector[] ToRosterVectorArray() => this.CheckedValues.ToArray();

        public override string ToString() => string.Join(", ", CheckedValues);

        protected bool Equals(CategoricalLinkedMultiOptionAnswer other)
        {
            //TODO: CHECK equality
            return CheckedValues.SequenceEqual(other.CheckedValues);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CategoricalLinkedMultiOptionAnswer) obj);
        }

        public override int GetHashCode()
        {
            return (CheckedValues != null ? CheckedValues.GetHashCode() : 0);
        }
    }

    [DebuggerDisplay("{ToString()}")]
    public class TextListAnswer : AbstractAnswer
    {
        protected TextListAnswer() { }

        private TextListAnswer(IEnumerable<TextListAnswerRow> rows)
        {
            if (rows == null) throw new ArgumentNullException(nameof(rows));

            this.Rows = rows.ToReadOnlyCollection();
        }

        public IReadOnlyList<TextListAnswerRow> Rows { get; protected set; }

        public Tuple<decimal, string>[] ToTupleArray() => this.Rows.Select(row => Tuple.Create((decimal)row.Value, row.Text)).ToArray();

        public static TextListAnswer FromTextListAnswerRows(IEnumerable<TextListAnswerRow> rows) => rows == null ? null : new TextListAnswer(rows);

        public static TextListAnswer FromTupleArray(Tuple<int, string>[] tupleArray)
            => tupleArray == null ? null : new TextListAnswer(
                tupleArray.Select(tuple => new TextListAnswerRow(tuple.Item1, tuple.Item2.Trim().RemoveControlChars())));

        public static TextListAnswer FromTupleArray(Tuple<decimal, string>[] tupleArray)
            => tupleArray == null ? null : new TextListAnswer(
               tupleArray.Select(tuple => new TextListAnswerRow(Convert.ToInt32(tuple.Item1), tuple.Item2.Trim().RemoveControlChars())));

        public override string ToString() => string.Join(", ", Rows.Select(x => x.Text));

        protected bool Equals(TextListAnswer other)
        {
            return other.ToTupleArray().SequenceEqual(this.ToTupleArray());
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TextListAnswer) obj);
        }

        public override int GetHashCode()
        {
            return (Rows != null ? Rows.GetHashCode() : 0);
        }
    }

    [DebuggerDisplay("{ToString()}")]
    public class GpsAnswer : AbstractAnswer
    {
        protected GpsAnswer() { }
        private GpsAnswer(GeoPosition value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            this.Value = value;
        }

        public GeoPosition Value { get; protected set; }

        public static GpsAnswer FromGeoPosition(GeoPosition value) => value != null ? new GpsAnswer(value) : null;

        public GeoLocation ToGeoLocation()
            => new GeoLocation(Value.Latitude, Value.Longitude, Value.Accuracy, Value.Altitude);

        public override string ToString() => Value.ToString();

        protected bool Equals(GpsAnswer other)
        {
            return Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GpsAnswer) obj);
        }

        public override int GetHashCode()
        {
            return (Value != null ? Value.GetHashCode() : 0);
        }
    }

    [DebuggerDisplay("{ToString()}")]
    public class QRBarcodeAnswer : AbstractAnswer
    {
        protected QRBarcodeAnswer() { }

        private QRBarcodeAnswer(string decodedText)
        {
            if (decodedText == null) throw new ArgumentNullException(nameof(decodedText));
            this.DecodedText = decodedText;
        }

        public string DecodedText { get; protected set; }
        public static QRBarcodeAnswer FromString(string decodedText) => decodedText != null ? new QRBarcodeAnswer(decodedText) : null;
        public override string ToString() => DecodedText;

        protected bool Equals(QRBarcodeAnswer other)
        {
            return string.Equals(DecodedText, other.DecodedText);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((QRBarcodeAnswer) obj);
        }

        public override int GetHashCode()
        {
            return (DecodedText != null ? DecodedText.GetHashCode() : 0);
        }
    }

    [DebuggerDisplay("{ToString()}")]
    public class MultimediaAnswer : AbstractAnswer
    {
        protected MultimediaAnswer() { }

        private MultimediaAnswer(string fileName, DateTime? answerTimeUtc)
        {
            this.FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            AnswerTimeUtc = answerTimeUtc;
        }

        public string FileName { get; protected set; }
        public DateTime? AnswerTimeUtc { get; protected set; }

        public static MultimediaAnswer FromString(string fileName, DateTime? answerTimeUtc) 
            => fileName != null ? new MultimediaAnswer(fileName,answerTimeUtc) : null;

        public override string ToString() => FileName;

        protected bool Equals(MultimediaAnswer other)
        {
            return string.Equals(FileName, other.FileName) && AnswerTimeUtc.Equals(other.AnswerTimeUtc);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MultimediaAnswer) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((FileName != null ? FileName.GetHashCode() : 0) * 397) ^ AnswerTimeUtc.GetHashCode();
            }
        }
    }

    [DebuggerDisplay("{ToString()}")]
    public class AudioAnswer : AbstractAnswer
    {
        protected AudioAnswer() { }

        private AudioAnswer(string fileName, TimeSpan length)
        {
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));
            if (length.Ticks == 0) throw new ArgumentNullException(nameof(length));

            this.FileName = fileName;
            this.Length = length;
        }

        public string FileName { get; protected set; }
        public TimeSpan Length { get; protected set; }

        public static AudioAnswer FromString(string fileName, TimeSpan? length)
        {
            return fileName != null ? new AudioAnswer(fileName, length.Value) : null;
        }

        public override string ToString() => $"{FileName} => {Length}";

        public AudioAnswerForConditions ToAudioAnswerForContions()
        {
            return new AudioAnswerForConditions
            {
                FileName = this.FileName,
                Length = this.Length
            };
        }

        public override bool Equals(object obj)
        {
            var target = obj as AudioAnswer;
            if (target == null) return false;

            return target.Length == this.Length && target.FileName == this.FileName;
        }

        public override int GetHashCode() => this.Length.GetHashCode() ^ this.FileName.GetHashCode();
    }

    [DebuggerDisplay("{ToString()}")]
    public class AreaAnswer : AbstractAnswer
    {
        protected AreaAnswer() { }

        private AreaAnswer(Area area)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));

            this.Value = area;
        }

        public Area Value { get; protected set; }

        public static AreaAnswer FromArea(Area area) => area != null ? new AreaAnswer(area) : null;

        public override string ToString() => Value.ToString();

        public Georgaphy ToGeorgaphy()
        {
            return new Georgaphy
            {
                Area = Value.AreaSize ?? 0,
                Length = Value.Length ?? 0,
                PointsCount = Value.NumberOfPoints ?? 0
            };
        }

        protected bool Equals(AreaAnswer other)
        {
            return Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AreaAnswer) obj);
        }

        public override int GetHashCode()
        {
            return (Value != null ? Value.GetHashCode() : 0);
        }
    }

    [DebuggerDisplay("{ToString()}")]
    public class YesNoAnswer : AbstractAnswer
    {
        protected YesNoAnswer() { }

        private YesNoAnswer(IEnumerable<CheckedYesNoAnswerOption> checkedOptions)
        {
            if (checkedOptions == null) throw new ArgumentNullException(nameof(checkedOptions));

            this.CheckedOptions = checkedOptions.ToReadOnlyCollection();
        }

        public IReadOnlyCollection<CheckedYesNoAnswerOption> CheckedOptions { get; protected set; }

        public IEnumerable<AnsweredYesNoOption> ToAnsweredYesNoOptions() => this.CheckedOptions.Select(option => new AnsweredYesNoOption(option.Value, option.Yes));

        public static YesNoAnswer FromCheckedYesNoAnswerOptions(IEnumerable<CheckedYesNoAnswerOption> checkedOptions)
            => checkedOptions == null ? null : new YesNoAnswer(checkedOptions);

        public static YesNoAnswer FromAnsweredYesNoOptions(IEnumerable<AnsweredYesNoOption> answeredOptions)
            => answeredOptions == null ? null : new YesNoAnswer(
                answeredOptions.Select(option => new CheckedYesNoAnswerOption((int)option.OptionValue, option.Yes)));

        public static YesNoAnswer FromYesNoAnswersOnly(YesNoAnswersOnly yesNoAnswersOnly)
            => yesNoAnswersOnly == null ? null : new YesNoAnswer(
                Enumerable.Concat(
                    yesNoAnswersOnly.Yes.Select(yesOption => new CheckedYesNoAnswerOption((int)yesOption, true)),
                    yesNoAnswersOnly.No.Select(noOption => new CheckedYesNoAnswerOption((int)noOption, false))));

        public YesNoAnswersOnly ToYesNoAnswersOnly()
            => new YesNoAnswersOnly(
                this.CheckedOptions.Where(x => x.Yes).Select(x => (decimal)x.Value).ToArray(),
                this.CheckedOptions.Where(x => x.No).Select(x => (decimal)x.Value).ToArray());

        public override string ToString() => string.Join(", ", CheckedOptions);

        protected bool Equals(YesNoAnswer other)
        {
            return other.ToAnsweredYesNoOptions().SequenceEqual(this.ToAnsweredYesNoOptions());
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((YesNoAnswer) obj);
        }

        public override int GetHashCode()
        {
            return (CheckedOptions != null ? CheckedOptions.GetHashCode() : 0);
        }
    }
}

