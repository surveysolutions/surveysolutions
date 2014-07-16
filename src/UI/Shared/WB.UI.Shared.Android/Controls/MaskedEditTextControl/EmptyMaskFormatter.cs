using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace WB.UI.Shared.Android.Controls.MaskedEditTextControl
{
    public class EmptyMaskFormatter : IMaskedFormatter
    {
        public string Mask
        {
            get { return string.Empty; }
        }

        public string ValueToString(string value, ref int oldCurstorPosition)
        {
            return value ?? "";
        }

        public bool IsTextMaskMatched(string text)
        {
            return true;
        }

        public string GetCleanText(string text)
        {
            return text.Trim();
        }
    }
}