using System;
using System.Linq;
using Android.Content.Res;
using Android.Provider;
using Android.Runtime;
using Android.Text;
using Android.Widget;
using Cirrious.MvvmCross.Binding;
using Java.Lang;
using WB.Core.SharedKernels.DataCollection.MaskFormatter;
using Math = Java.Lang.Math;


namespace WB.UI.QuestionnaireTester.CustomBindings
{
    public class EditTextMaskBindingNew : BaseBinding<EditText, string>
    {
        public class MaskInputFilter : Java.Lang.Object, IInputFilter, ITextWatcher
        {
            private const char DigitKey = '#';
            private const char CharacterKey = '~';
            private const char AnythingKey = '*';

            private readonly char[] charRepresentationArray = { AnythingKey, DigitKey, CharacterKey };

            private string mask;
            private char maskFill = '_';

            private bool formatting;
            private int mStart;
            private int mEnd;

            public MaskInputFilter(string mask)
            {
                this.mask = mask;
            }

            public ICharSequence FilterFormatted(ICharSequence source, int start, int end, ISpanned dest, int dstart, int dend)
            {
                if (string.IsNullOrEmpty(this.mask))
                    return null;

                if (this.formatting)
                    return null;

                if (source.Length() == 0)
                    return null;

                char[] filterArray = new char[source.Length()];

                for (int i = 0; i < source.Length(); i++)
                {
                    var currentChar = source.CharAt(i);

                    var indexInMask = i + dstart;
                    if (indexInMask >= mask.Length)
                        continue;


                    var maskChar = mask[indexInMask];

                    switch (maskChar)
                    {
                        case DigitKey:
                            filterArray[i] = char.IsDigit(currentChar) ? currentChar : maskFill;
                            break;

                        case CharacterKey:
                            filterArray[i] = char.IsLetter(currentChar) ? currentChar : maskFill;
                            break;

                        case AnythingKey:
                            filterArray[i] = currentChar;
                            break;

                        default:
                            filterArray[i] = maskChar;
                            break;
                    }
                }

                var filterString = new Java.Lang.String(filterArray);
                return filterString;
            }

            public void BeforeTextChanged(ICharSequence s, int start, int count, int after)
            {
            }

            public void OnTextChanged(ICharSequence s, int start, int before, int count)
            {
                this.mStart = -1;
                if (before == 0)
                {
                    mStart = start + count;
                    mEnd = Math.Min(mStart + count, s.Length());
                }
            }

            public void AfterTextChanged(IEditable s)
            {
                if (!formatting)
                {
                    if (mStart >= 0)
                    {
                        formatting = true;
                        s.Replace(mStart, mEnd, "");
                        formatting = false;
                    }
                }
            }
        }

        public EditTextMaskBindingNew(EditText target)
            : base(target)
        {
        }

        protected override void SetValueToView(EditText view, string value)
        {
            bool isInputMasked = !string.IsNullOrWhiteSpace(value);

            var maskInputFilter = new MaskInputFilter(value);
            view.Text = value;
            view.SetFilters(new IInputFilter[] { maskInputFilter });
            view.AddTextChangedListener(maskInputFilter);

            if (isInputMasked)
            {
                //maskedWatcher = new MaskedWatcher(value, Target);
                //Target.AddTextChangedListener(maskedWatcher);
                //Target.InputType = InputTypes.TextVariationVisiblePassword; //fix for samsung 
            }
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.OneWay; }
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                var editText = Target;
                if (editText != null)
                {
                    //editText.RemoveTextChangedListener(maskedWatcher);
                }
            }
            base.Dispose(isDisposing);
        }
    }
}
