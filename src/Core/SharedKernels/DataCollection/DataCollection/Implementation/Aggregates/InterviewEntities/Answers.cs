using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public abstract class AbstractAnswer { }

    public class TextAnswer
    {
        public TextAnswer(string value)
        {
            this.Value = value;
        }

        public string Value { get; }
    }

    public class NumericIntegerAnswer
    {
        public NumericIntegerAnswer(int value)
        {
            this.Value = value;
        }

        public int Value { get; }
    }

    public class NumericRealAnswer
    {
        public NumericRealAnswer(double value)
        {
            this.Value = value;
        }

        public double Value { get; }
    }

    public class DateTimeAnswer
    {
        public DateTimeAnswer(DateTime value)
        {
            this.Value = value;
        }

        public DateTime Value { get; }
    }

    public class CategoricalFixedSingleOptionAnswer
    {
        public CategoricalFixedSingleOptionAnswer(int selectedValue)
        {
            this.SelectedValue = selectedValue;
        }

        public int SelectedValue { get; }
    }

    public class CategoricalFixedMultiOptionAnswer
    {
        public CategoricalFixedMultiOptionAnswer(IEnumerable<decimal> checkedValues)
        {
            this.CheckedValues = checkedValues.ToReadOnlyCollection();
        }

        public IReadOnlyCollection<decimal> CheckedValues { get; }
    }

    public class CategoricalLinkedSingleOptionAnswer
    {
        public CategoricalLinkedSingleOptionAnswer(RosterVector selectedValue)
        {
            this.SelectedValue = selectedValue;
        }

        public RosterVector SelectedValue { get; }
    }

    public class CategoricalLinkedMultiOptionAnswer
    {
        public CategoricalLinkedMultiOptionAnswer(IEnumerable<RosterVector> checkedValues)
        {
            this.CheckedValues = checkedValues.ToReadOnlyCollection();
        }

        public IReadOnlyCollection<RosterVector> CheckedValues { get; }
    }

    public class TextListAnswer
    {
        public TextListAnswer(IEnumerable<TextListAnswerRow> rows)
        {
            this.Rows = rows.ToReadOnlyCollection();
        }

        public IReadOnlyCollection<TextListAnswerRow> Rows { get; }
    }

    public class TextListAnswerRow
    {
        public TextListAnswerRow(decimal code, string text)
        {
            this.Code = code;
            this.Text = text;
        }

        public decimal Code { get; }
        public string Text { get; }
    }

    public class GpsAnswer
    {
        public GpsAnswer(GeoPosition value)
        {
            this.Value = value;
        }

        public GeoPosition Value { get; }
    }

    public class QRBarcodeAnswer
    {
        public QRBarcodeAnswer(string text)
        {
            this.Text = text;
        }

        public string Text { get; }
    }
}
