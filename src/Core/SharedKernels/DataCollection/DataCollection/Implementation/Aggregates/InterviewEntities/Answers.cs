using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers
{
    public abstract class AbstractAnswer { }

    public class TextAnswer : AbstractAnswer
    {
        public TextAnswer(string value)
        {
            this.Value = value;
        }

        public string Value { get; }
    }

    public class NumericIntegerAnswer : AbstractAnswer
    {
        public NumericIntegerAnswer(int value)
        {
            this.Value = value;
        }

        public int Value { get; }
    }

    public class NumericRealAnswer : AbstractAnswer
    {
        public NumericRealAnswer(double value)
        {
            this.Value = value;
        }

        public double Value { get; }
    }

    public class DateTimeAnswer : AbstractAnswer
    {
        public DateTimeAnswer(DateTime value)
        {
            this.Value = value;
        }

        public DateTime Value { get; }
    }

    public class CategoricalFixedSingleOptionAnswer : AbstractAnswer
    {
        public CategoricalFixedSingleOptionAnswer(int selectedValue)
        {
            this.SelectedValue = selectedValue;
        }

        public int SelectedValue { get; }
    }

    public class CategoricalFixedMultiOptionAnswer : AbstractAnswer
    {
        public CategoricalFixedMultiOptionAnswer(IEnumerable<int> checkedValues)
        {
            this.CheckedValues = checkedValues.ToReadOnlyCollection();
        }

        public IReadOnlyCollection<int> CheckedValues { get; }
    }

    public class CategoricalLinkedSingleOptionAnswer : AbstractAnswer
    {
        public CategoricalLinkedSingleOptionAnswer(RosterVector selectedValue)
        {
            this.SelectedValue = selectedValue;
        }

        public RosterVector SelectedValue { get; }
    }

    public class CategoricalLinkedMultiOptionAnswer : AbstractAnswer
    {
        public CategoricalLinkedMultiOptionAnswer(IEnumerable<RosterVector> checkedValues)
        {
            this.CheckedValues = checkedValues.ToReadOnlyCollection();
        }

        public IReadOnlyCollection<RosterVector> CheckedValues { get; }
    }

    public class TextListAnswer : AbstractAnswer
    {
        public TextListAnswer(IEnumerable<TextListAnswerRow> rows)
        {
            this.Rows = rows.ToReadOnlyCollection();
        }

        public IReadOnlyCollection<TextListAnswerRow> Rows { get; }

        public Tuple<decimal, string>[] ToTupleArray() => this.Rows.Select(row => Tuple.Create(row.Value, row.Text)).ToArray();
    }

    public class TextListAnswerRow
    {
        public TextListAnswerRow(decimal value, string text)
        {
            this.Value = value;
            this.Text = text;
        }

        public decimal Value { get; }
        public string Text { get; }
    }

    public class GpsAnswer : AbstractAnswer
    {
        public GpsAnswer(GeoPosition value)
        {
            this.Value = value;
        }

        public GeoPosition Value { get; }
    }

    public class QRBarcodeAnswer : AbstractAnswer
    {
        public QRBarcodeAnswer(string decodedText)
        {
            this.DecodedText = decodedText;
        }

        public string DecodedText { get; }
    }

    public class MultimediaAnswer : AbstractAnswer
    {
        public MultimediaAnswer(string fileName)
        {
            this.FileName = fileName;
        }

        public string FileName { get; }
    }

    public class YesNoAnswer : AbstractAnswer
    {
        public YesNoAnswer(IEnumerable<CheckedYesNoAnswerOption> checkedOptions)
        {
            this.CheckedOptions = checkedOptions.ToReadOnlyCollection();
        }

        public YesNoAnswer(IEnumerable<AnsweredYesNoOption> answeredOptions)
            : this(answeredOptions.Select(option => new CheckedYesNoAnswerOption((int) option.OptionValue, option.Yes))) {}

        public IReadOnlyCollection<CheckedYesNoAnswerOption> CheckedOptions { get; }

        public IEnumerable<AnsweredYesNoOption> ToAnsweredYesNoOptions() => this.CheckedOptions.Select(option => new AnsweredYesNoOption(option.Value, option.Yes));
    }

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
    }
}
