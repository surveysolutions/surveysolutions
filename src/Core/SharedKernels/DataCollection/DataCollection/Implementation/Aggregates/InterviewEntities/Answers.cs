﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers
{
    public abstract class AbstractAnswer { }

    [DebuggerDisplay("{ToString()}")]
    public class TextAnswer : AbstractAnswer
    {
        private TextAnswer(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            this.Value = value;
        }

        public string Value { get; }

        public static TextAnswer FromString(string value) => value != null ? new TextAnswer(value) : null;

        public override string ToString() => Value;
    }

    [DebuggerDisplay("{ToString()}")]
    public class NumericIntegerAnswer : AbstractAnswer
    {
        private NumericIntegerAnswer(int value)
        {
            this.Value = value;
        }

        public int Value { get; }

        public static NumericIntegerAnswer FromInt(int value) => new NumericIntegerAnswer(value);

        public override string ToString() => Value.ToString();
    }

    [DebuggerDisplay("{ToString()}")]
    public class NumericRealAnswer : AbstractAnswer
    {
        private NumericRealAnswer(double value)
        {
            this.Value = value;
        }

        public double Value { get; }

        public static NumericRealAnswer FromDouble(double value) => new NumericRealAnswer(value);

        public static NumericRealAnswer FromDecimal(decimal value) => new NumericRealAnswer((double) value);

        public override string ToString() => Value.ToString();
    }

    [DebuggerDisplay("{ToString()}")]
    public class DateTimeAnswer : AbstractAnswer
    {
        private DateTimeAnswer(DateTime value)
        {
            this.Value = value;
        }

        public DateTime Value { get; }

        public static DateTimeAnswer FromDateTime(DateTime value) => new DateTimeAnswer(value);

        public override string ToString() => Value.ToString("yyyy-MM-ddTHH:mm:ss");
    }

    [DebuggerDisplay("{ToString()}")]
    public class CategoricalFixedSingleOptionAnswer : AbstractAnswer
    {
        private CategoricalFixedSingleOptionAnswer(int selectedValue)
        {
            this.SelectedValue = selectedValue;
        }

        public int SelectedValue { get; }

        public static CategoricalFixedSingleOptionAnswer FromInt(int selectedValue) => new CategoricalFixedSingleOptionAnswer(selectedValue);
        public static CategoricalFixedSingleOptionAnswer FromDecimal(decimal selectedValue) => new CategoricalFixedSingleOptionAnswer((int) selectedValue);

        public override string ToString() => SelectedValue.ToString();
    }

    [DebuggerDisplay("{ToString()}")]
    public class CategoricalFixedMultiOptionAnswer : AbstractAnswer
    {
        private CategoricalFixedMultiOptionAnswer(IEnumerable<int> checkedValues)
        {
            if (checkedValues == null) throw new ArgumentNullException(nameof(checkedValues));
            this.CheckedValues = checkedValues.ToReadOnlyCollection();
        }

        public IReadOnlyCollection<int> CheckedValues { get; }

        public IEnumerable<decimal> ToDecimals() => this.CheckedValues.Select(value => (decimal) value);

        public static CategoricalFixedMultiOptionAnswer FromInts(int[] checkedValues)
            => checkedValues == null || !checkedValues.Any() ? null : new CategoricalFixedMultiOptionAnswer(checkedValues);

        public static CategoricalFixedMultiOptionAnswer FromDecimalArray(decimal[] checkedValues)
            => checkedValues == null || !checkedValues.Any() ? null : new CategoricalFixedMultiOptionAnswer(checkedValues.Select(value => (int) value));

        public override string ToString() => string.Join(", ", CheckedValues);
    }

    [DebuggerDisplay("{ToString()}")]
    public class CategoricalLinkedSingleOptionAnswer : AbstractAnswer
    {
        private CategoricalLinkedSingleOptionAnswer(RosterVector selectedValue)
        {
            if (selectedValue == null) throw new ArgumentNullException(nameof(selectedValue));
            this.SelectedValue = selectedValue;
        }

        public RosterVector SelectedValue { get; }

        public static CategoricalLinkedSingleOptionAnswer FromRosterVector(RosterVector selectedValue)
            => selectedValue == null ? null : new CategoricalLinkedSingleOptionAnswer(selectedValue);

        public override string ToString() => SelectedValue.ToString();
    }

    [DebuggerDisplay("{ToString()}")]
    public class CategoricalLinkedMultiOptionAnswer : AbstractAnswer
    {
        private CategoricalLinkedMultiOptionAnswer(IEnumerable<RosterVector> checkedValues)
        {
            if (checkedValues == null) throw new ArgumentNullException(nameof(checkedValues));
            this.CheckedValues = checkedValues.ToReadOnlyCollection();
        }

        public IReadOnlyCollection<RosterVector> CheckedValues { get; }

        public static CategoricalLinkedMultiOptionAnswer FromRosterVectors(IEnumerable<RosterVector> checkedValues)
            => checkedValues == null ? null : new CategoricalLinkedMultiOptionAnswer(checkedValues);

        public RosterVector[] ToRosterVectorArray() => this.CheckedValues.ToArray();

        public override string ToString() => string.Join(", ", CheckedValues);
    }

    [DebuggerDisplay("{ToString()}")]
    public class TextListAnswer : AbstractAnswer
    {
        private TextListAnswer(IEnumerable<TextListAnswerRow> rows)
        {
            if (rows == null) throw new ArgumentNullException(nameof(rows));
            this.Rows = rows.ToReadOnlyCollection();
        }

        public IReadOnlyCollection<TextListAnswerRow> Rows { get; }

        public Tuple<decimal, string>[] ToTupleArray() => this.Rows.Select(row => Tuple.Create(row.Value, row.Text)).ToArray();

        public static TextListAnswer FromTextListAnswerRows(IEnumerable<TextListAnswerRow> rows) => rows == null ? null : new TextListAnswer(rows);

        public static TextListAnswer FromTupleArray(Tuple<decimal, string>[] tupleArray)
            => tupleArray == null ? null : new TextListAnswer(
                tupleArray.Select(tuple => new TextListAnswerRow(tuple.Item1, tuple.Item2)));
        
        public override string ToString() => string.Join(", ", Rows.Select(x => x.Text));
    }

    [DebuggerDisplay("{ToString()}")]
    public class TextListAnswerRow
    {
        public TextListAnswerRow(decimal value, string text)
        {
            this.Value = value;
            this.Text = text;
        }

        public decimal Value { get; }
        public string Text { get; }

        public override string ToString() => $"{Value} -> {Text}";
    }

    [DebuggerDisplay("{ToString()}")]
    public class GpsAnswer : AbstractAnswer
    {
        private GpsAnswer(GeoPosition value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            this.Value = value;
        }

        public GeoPosition Value { get; }

        public static GpsAnswer FromGeoPosition(GeoPosition value) => value != null ? new GpsAnswer(value) : null;

        public override string ToString() => Value.ToString();
    }

    [DebuggerDisplay("{ToString()}")]
    public class QRBarcodeAnswer : AbstractAnswer
    {
        private QRBarcodeAnswer(string decodedText)
        {
            if (decodedText == null) throw new ArgumentNullException(nameof(decodedText));
            this.DecodedText = decodedText;
        }

        public string DecodedText { get; }

        public static QRBarcodeAnswer FromString(string decodedText) => decodedText != null ? new QRBarcodeAnswer(decodedText) : null;

        public override string ToString() => DecodedText;
    }

    [DebuggerDisplay("{ToString()}")]
    public class MultimediaAnswer : AbstractAnswer
    {
        private MultimediaAnswer(string fileName)
        {
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));
            this.FileName = fileName;
        }

        public string FileName { get; }

        public static MultimediaAnswer FromString(string fileName) => fileName != null ? new MultimediaAnswer(fileName) : null;

        public override string ToString() => FileName;
    }
    
    [DebuggerDisplay("{ToString()}")]
    public class AreaAnswer : AbstractAnswer
    {
        private AreaAnswer(Area area)
        {
            if (area == null) throw new ArgumentNullException(nameof(area));
            this.Value = area;
        }

        //public static AreaAnswer FromGeoPosition(GeoPosition value) => value != null ? new AreaAnswer(value) : null;

        public Area Value { get; }

        public static AreaAnswer FromArea(Area area) => area!= null ? new AreaAnswer(area) : null;

        public override string ToString() => Value.ToString();
    }

    [DebuggerDisplay("{ToString()}")]
    public class YesNoAnswer : AbstractAnswer
    {
        private YesNoAnswer(IEnumerable<CheckedYesNoAnswerOption> checkedOptions)
        {
            if (checkedOptions == null) throw new ArgumentNullException(nameof(checkedOptions));
            this.CheckedOptions = checkedOptions.ToReadOnlyCollection();
        }

        public IReadOnlyCollection<CheckedYesNoAnswerOption> CheckedOptions { get; }

        public IEnumerable<AnsweredYesNoOption> ToAnsweredYesNoOptions() => this.CheckedOptions.Select(option => new AnsweredYesNoOption(option.Value, option.Yes));

        public static YesNoAnswer FromCheckedYesNoAnswerOptions(IEnumerable<CheckedYesNoAnswerOption> checkedOptions)
            => checkedOptions == null ? null : new YesNoAnswer(checkedOptions);

        public static YesNoAnswer FromAnsweredYesNoOptions(IEnumerable<AnsweredYesNoOption> answeredOptions)
            => answeredOptions == null ? null : new YesNoAnswer(
                answeredOptions.Select(option => new CheckedYesNoAnswerOption((int) option.OptionValue, option.Yes)));

        public static YesNoAnswer FromYesNoAnswersOnly(YesNoAnswersOnly yesNoAnswersOnly)
            => yesNoAnswersOnly == null ? null : new YesNoAnswer(
                Enumerable.Concat(
                    yesNoAnswersOnly.Yes.Select(yesOption => new CheckedYesNoAnswerOption((int) yesOption, true)),
                    yesNoAnswersOnly.No.Select(noOption => new CheckedYesNoAnswerOption((int) noOption, false))));

        public YesNoAnswersOnly ToYesNoAnswersOnly()
            => new YesNoAnswersOnly(
                this.CheckedOptions.Where(x => x.Yes).Select(x => (decimal) x.Value).ToArray(),
                this.CheckedOptions.Where(x => x.No).Select(x => (decimal) x.Value).ToArray());

        public override string ToString() => string.Join(", ", CheckedOptions);
    }

    [DebuggerDisplay("{ToString()}")]
    public class CheckedYesNoAnswerOption
    {
        public CheckedYesNoAnswerOption(int value, bool yes)
        {
            this.Value = value;
            this.Yes = yes;
        }

        public int Value { get; }
        public bool Yes { get; }
        public bool No => !Yes;

        public override string ToString() => $"{this.Value} -> {(this.Yes ? "Yes" : "No")}";
    }
}
